using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class Round2Script1 : MonoBehaviour
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
    public string speedParam = "Speed";
    public float animDamp = 0.1f;

    [Header("UI state detection (CCTV/Tablet)")]
    public GameObject[] uiOpenGameObjects;
    public CanvasGroup[] uiOpenCanvasGroups;
    public bool useCursorFallback = false;

    [Header("Freeze Animation (Q to freeze; move keys to resume)")]
    public Key freezeKey = Key.Q;
    private bool frozenByKey = false;
    private float animatorSpeedBeforeFreeze = 1f;

    [Header("Safety")]
    public bool useUnscaledWhenPaused = true;

    private CharacterController cc;
    private Vector3 verticalVelocity;
    private float yaw, pitch;
    private bool uiOpenThisFrame;
    private bool warnedTimeScale;

    // Use unscaled time if timeScale == 0 so movement still works when UI pauses the game
    float DT => (useUnscaledWhenPaused && Time.timeScale == 0f)
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cameraTransform) cameraTransform = GetComponentInChildren<Camera>()?.transform;
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (animator) animator.applyRootMotion = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = cameraTransform ? cameraTransform.localEulerAngles.x : 0f;

        if (gameObject.isStatic)
            Debug.LogWarning("[Round2Script1] FINALPLAYER is Static; movement may be blocked.");
    }

    void Start()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f; // ensure we don't start paused
    }

    void OnEnable()
    {
        if (animator) animator.speed = frozenByKey ? 0f : 1f;
    }

    void Update()
    {
        // If you have a PauseMenu flag, obey it (remove if unused)
        if (PauseMenu.GameIsPaused) return;

        if (Time.timeScale == 0f && !warnedTimeScale)
        {
            Debug.LogWarning("[Round2Script1] timeScale is 0 — using unscaled deltaTime for movement.");
            warnedTimeScale = true;
        }
        else if (Time.timeScale > 0f) warnedTimeScale = false;

        var kb = Keyboard.current;

        // --- Freeze logic: Q dominates over movement if both are held ---
        if (kb != null)
        {
            if (kb[freezeKey].wasPressedThisFrame) FreezeAnimator();

            bool freezeHeld = kb[freezeKey].isPressed;

            if (freezeHeld)
            {
                if (!frozenByKey) FreezeAnimator();
            }
            else if (frozenByKey && AnyMoveKeyPressedOrTapped(kb))
            {
                UnfreezeAnimator();
            }
        }

        uiOpenThisFrame = ComputeUIOpen();
        if (animator)
        {
            animator.SetBool("UIOpen", uiOpenThisFrame);
            if (!frozenByKey) animator.speed = 1f;
        }

        HandleLook();
        HandleMoveAndAnimate();
    }

    void FreezeAnimator()
    {
        frozenByKey = true;
        if (animator)
        {
            animatorSpeedBeforeFreeze = animator.speed;
            animator.speed = 0f;
        }
    }

    void UnfreezeAnimator()
    {
        frozenByKey = false;
        if (animator) animator.speed = (animatorSpeedBeforeFreeze > 0f) ? animatorSpeedBeforeFreeze : 1f;
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

        Vector3 move = transform.right * input.x + transform.forward * input.y;
        float targetSpeed = (allowSprint && kb != null && kb.leftShiftKey.isPressed) ? sprintSpeed : moveSpeed;
        Vector3 horizontal = move * targetSpeed;

        cc.Move(horizontal * DT);

        if (cc.isGrounded && verticalVelocity.y < 0f) verticalVelocity.y = -2f;
        verticalVelocity.y += gravity * DT;
        cc.Move(verticalVelocity * DT);

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
