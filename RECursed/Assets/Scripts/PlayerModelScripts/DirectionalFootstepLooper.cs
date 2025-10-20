using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class DirectionalFootstepLooper : MonoBehaviour
{
    [Header("Loop Clips")]
    public AudioClip clipW, clipA, clipS, clipD;

    [Header("Behaviour")]
    public bool requireGrounded = false;              // safer default
    public float fadeSpeed = 10f;
    [Range(0f,1f)] public float volumeWhenMoving = 0.6f;
    [Tooltip("How fast the player must move before we call it 'moving' (m/s).")]
    public float velocityThreshold = 0.05f;

    private CharacterController cc;
    private AudioSource source;
    private AudioClip current;
    private Vector3 lastRoot;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        cc = GetComponentInParent<CharacterController>();

        // AudioSource safety defaults
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 1f;          // 3D
        source.volume = 0f;                // we fade it

        lastRoot = transform.root.position;
    }

    void Update()
    {
        // 1) Ground check (optional)
        bool groundedOK = !requireGrounded || (cc && cc.isGrounded);

        // 2) Input check (keyboard)
        var kb = Keyboard.current;
        bool keyMoving = kb != null && (
               kb.wKey.isPressed || kb.aKey.isPressed || kb.sKey.isPressed || kb.dKey.isPressed
            || kb.upArrowKey.isPressed || kb.leftArrowKey.isPressed || kb.downArrowKey.isPressed || kb.rightArrowKey.isPressed
        );

        // 3) Real movement check (velocity or transform delta)
        float velMag = 0f;
        if (cc) velMag = cc.velocity.magnitude;
        else
        {
            var root = transform.root.position;
            velMag = (root - lastRoot).magnitude / Mathf.Max(Time.deltaTime, 1e-4f);
            lastRoot = root;
        }
        bool velMoving = velMag > velocityThreshold;

        bool moving = groundedOK && (keyMoving || velMoving);

        // 4) Choose clip based on directional keys (keep current if moving without keys)
        AudioClip wanted = null;
        if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) wanted = clipW;
            else if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) wanted = clipA;
            else if (kb.sKey.isPressed || kb.downArrowKey.isPressed) wanted = clipS;
            else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) wanted = clipD;
        }
        if (wanted == null) wanted = current;

        bool shouldPlay = moving && wanted != null;

        // 5) Swap clip on direction change
        if (shouldPlay && wanted != current)
        {
            current = wanted;
            source.Stop();
            source.clip = current;
            source.Play();
            // Debug.Log("[Footsteps] playing: " + current.name);
        }

        // 6) Fade volume up/down
        float targetVol = shouldPlay ? volumeWhenMoving : 0f;
        source.volume = Mathf.MoveTowards(source.volume, targetVol, fadeSpeed * Time.deltaTime);

        // 7) Pause when silent, unpause when needed
        if (source.isPlaying && source.volume <= 0.0005f && !shouldPlay) source.Pause();
        if (!source.isPlaying && shouldPlay) source.UnPause();
    }

    // Right-click the component header in Play mode to call this
    [ContextMenu("Test Footstep (W)")]
    void TestFootstep()
    {
        if (!clipW) { Debug.LogWarning("No W clip assigned on DirectionalFootstepLooper."); return; }
        source.Stop();
        source.clip = clipW;
        source.volume = volumeWhenMoving;
        source.Play();
    }
}
