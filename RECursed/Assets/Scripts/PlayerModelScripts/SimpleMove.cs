using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;   // for CanvasGroup

[RequireComponent(typeof(CharacterController))]
public class FirstPersonMover : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;           // Main Camera under the player
    public Animator animator;                   // FINALPLAYER Animator

    [Header("Movement")]
    public float moveSpeed = 4.2f;
    public float sprintSpeed = 6.0f;
    public bool allowSprint = false;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.12f;
    public float pitchMin = -89f;
    public float pitchMax = 89f;

    [Header("Animation")]
    public string speedParam = "Speed";         // matches your controller
    public float animDamp = 0.1f;

    [Header("UI state detection (CCTV/Tablet)")]
    [Tooltip("Any of these being SetActive(true) means the UI is open.")]
    public GameObject[] uiOpenGameObjects;
    [Tooltip("Or detect via visible/interactable CanvasGroups.")]
    public CanvasGroup[] uiOpenCanvasGroups;
    [Tooltip("Optional fallback: treat unlocked cursor as UI open.")]
    public bool useCursorFallback = false;

    [Header("Freeze Animation (Q to freeze; move keys to resume)")]
    public Key freezeKey = Key.Q;
    private bool frozenByKey = false;
    private float animatorSpeedBeforeFreeze = 1f;

    private CharacterController cc;
    private Vector3 verticalVelocity;
    private float yaw, pitch;
    private bool uiOpenThisFrame;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cameraTransform) cameraTransform = GetComponentInChildren<Camera>()?.transform;
        if (!animator) animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = cameraTransform ? cameraTransform.localEulerAngles.x : 0f;
    }

    void OnEnable()
    {
        if (animator) animator.speed = frozenByKey ? 0f : 1f;
    }

    void Update()
    {
        if (PauseMenu.GameIsPaused) return;

        var kb = Keyboard.current;

        // --- Freeze logic: Q dominates over movement if both are held ---
        if (kb != null)
        {
            // Tap Q to enter frozen state
            if (kb[freezeKey].wasPressedThisFrame)
                FreezeAnimator();

            bool freezeHeld = kb[freezeKey].isPressed;

            if (freezeHeld)
            {
                // If Q is held, keep/force frozen (WASD cannot unfreeze)
                if (!frozenByKey) FreezeAnimator();
            }
            else if (frozenByKey)
            {
                // Only unfreeze when Q is NOT held, and a move key is pressed/tapped
                if (AnyMoveKeyPressedOrTapped(kb))
                    UnfreezeAnimator();
            }
        }

        // 1) UI open state for your controller logic
        uiOpenThisFrame = ComputeUIOpen();
        if (animator)
        {
            animator.SetBool("UIOpen", uiOpenThisFrame);
            if (!frozenByKey) animator.speed = 1f; // keep running unless frozen
        }

        // 2) Normal look + move
        HandleLook();
        HandleMoveAndAnimate();
    }

    void FreezeAnimator()
    {
        frozenByKey = true;
        if (animator)
        {
            animatorSpeedBeforeFreeze = animator.speed;
            animator.speed = 0f; // hard pause all layers
        }
    }

    void UnfreezeAnimator()
    {
        frozenByKey = false;
        if (animator)
            animator.speed = (animatorSpeedBeforeFreeze > 0f) ? animatorSpeedBeforeFreeze : 1f;
    }

    bool AnyMoveKeyPressedOrTapped(Keyboard kb)
    {
        return
            kb.wKey.isPressed || kb.aKey.isPressed || kb.sKey.isPressed || kb.dKey.isPressed ||
            kb.upArrowKey.isPressed || kb.downArrowKey.isPressed || kb.leftArrowKey.isPressed || kb.rightArrowKey.isPressed ||
            kb.wKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame ||
            kb.upArrowKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame ||
            kb.leftArrowKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame;
    }

    bool ComputeUIOpen()
    {
        if (uiOpenGameObjects != null)
        {
            for (int i = 0; i < uiOpenGameObjects.Length; i++)
                if (uiOpenGameObjects[i] && uiOpenGameObjects[i].activeInHierarchy)
                    return true;
        }

        if (uiOpenCanvasGroups != null)
        {
            for (int i = 0; i < uiOpenCanvasGroups.Length; i++)
            {
                var cg = uiOpenCanvasGroups[i];
                if (cg && cg.alpha > 0.5f && cg.interactable && cg.blocksRaycasts)
                    return true;
            }
        }

        if (useCursorFallback && Cursor.lockState != CursorLockMode.Locked)
            return true;

        return false;
    }

    void HandleLook()
    {
        if (Mouse.current == null || cameraTransform == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue();
        yaw   += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleMoveAndAnimate()
    {
        var kb = Keyboard.current;
        Vector2 input = Vector2.zero;

        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)     input.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)   input.y -= 1f;
        }
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Move relative to facing
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        float targetSpeed = (allowSprint && kb != null && kb.leftShiftKey.isPressed) ? sprintSpeed : moveSpeed;
        Vector3 horizontal = move * targetSpeed;

        // Horizontal move first
        cc.Move(horizontal * Time.deltaTime);

        // Gravity
        if (cc.isGrounded && verticalVelocity.y < 0f) verticalVelocity.y = -2f;
        verticalVelocity.y += gravity * Time.deltaTime;
        cc.Move(verticalVelocity * Time.deltaTime);

        // Drive Speed param (UI open => hold at 0)
        if (animator)
        {
            if (uiOpenThisFrame)
            {
                animator.SetFloat(speedParam, 0f, animDamp, Time.deltaTime);
            }
            else
            {
                float speedValue  = horizontal.magnitude;
                float denom       = Mathf.Max(moveSpeed, sprintSpeed);
                float normalized  = denom > 0f ? speedValue / denom : 0f;
                animator.SetFloat(speedParam, normalized, animDamp, Time.deltaTime);
            }
        }
    }
}
