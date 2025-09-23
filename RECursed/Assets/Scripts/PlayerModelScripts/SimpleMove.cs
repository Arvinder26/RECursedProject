using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonMover : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;     
    public Animator animator;             

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

    private CharacterController cc;
    private Vector3 verticalVelocity;
    private float yaw, pitch;

    // Q toggles this:
    private bool freezeMovement = false;

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

    void Update()
    {
        // Toggle freeze on Q press (press once = stop, press again = resume)
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            freezeMovement = !freezeMovement;

        HandleLook();
        HandleMoveAndAnimate();

	Cursor.lockState = freezeMovement ? CursorLockMode.None : CursorLockMode.Locked;
	Cursor.visible   = freezeMovement;

    }

    void HandleLook()
    {
        if (Mouse.current == null || cameraTransform == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue();
        yaw += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleMoveAndAnimate()
    {
        // Always apply gravity so you don't float while frozen
        if (cc.isGrounded && verticalVelocity.y < 0f) verticalVelocity.y = -2f;
        verticalVelocity.y += gravity * Time.deltaTime;

        // If frozen: no horizontal movement; keep animation at 0
        if (freezeMovement)
        {
            cc.Move(verticalVelocity * Time.deltaTime);
            if (animator) animator.SetFloat(speedParam, 0f, animDamp, Time.deltaTime);
            return;
        }

        // ----- Normal movement -----
        var kb = Keyboard.current;
        Vector2 input = Vector2.zero;

        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    input.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  input.y -= 1f;
        }
        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector3 move = transform.right * input.x + transform.forward * input.y;
        float targetSpeed = (allowSprint && kb != null && kb.leftShiftKey.isPressed) ? sprintSpeed : moveSpeed;
        Vector3 horizontal = move * targetSpeed;

        cc.Move(horizontal * Time.deltaTime);
        cc.Move(verticalVelocity * Time.deltaTime);

        if (animator)
        {
            float normalized = horizontal.magnitude / Mathf.Max(sprintSpeed, moveSpeed);
            animator.SetFloat(speedParam, normalized, animDamp, Time.deltaTime);
        }
    }
}
