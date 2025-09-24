using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonMover : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;     // drag your Main Camera (child of player)
    public Animator animator;             // drag the SkinnedMesh/rig Animator here

    [Header("Movement")]
    public float moveSpeed = 4.2f;        // walk speed (increase if you want faster)
    public float sprintSpeed = 6.0f;
    public bool allowSprint = false;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.12f;
    public float pitchMin = -89f;
    public float pitchMax = 89f;

    // Animator params
    [Header("Animation")]
    public string speedParam = "Speed";   // your controller uses "Speed"
    public float animDamp = 0.1f;         // damping to smooth Speed changes

    private CharacterController cc;
    private Vector3 verticalVelocity;
    private float yaw, pitch;

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
        HandleLook();
        HandleMoveAndAnimate();
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
        var kb = Keyboard.current;
        Vector2 input = Vector2.zero;

        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
        }
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Move relative to facing
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        float targetSpeed = (allowSprint && kb != null && kb.leftShiftKey.isPressed) ? sprintSpeed : moveSpeed;
        Vector3 horizontal = move * targetSpeed;

        // Apply horizontal first
        cc.Move(horizontal * Time.deltaTime);

        // Gravity
        if (cc.isGrounded && verticalVelocity.y < 0f) verticalVelocity.y = -2f;
        verticalVelocity.y += gravity * Time.deltaTime;
        cc.Move(verticalVelocity * Time.deltaTime);

        // ---- Animator hookup ----
        if (animator)
        {
            // Use intended horizontal speed (ignores gravity) for clean blending
            float speedValue = horizontal.magnitude;                // units/sec
            // Normalize to 0..1 so your transitions stay stable at different speeds
            float normalized = speedValue / Mathf.Max(sprintSpeed, moveSpeed);
            animator.SetFloat(speedParam, normalized, animDamp, Time.deltaTime);
        }
    }
}
