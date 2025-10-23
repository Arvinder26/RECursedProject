using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class FootstepSubtitles : MonoBehaviour
{
    public enum Mode { Off, WhileMoving, Pulse, OnPressCooldown }

    [Header("Mode")]
    public Mode mode = Mode.OnPressCooldown;

    [Header("Caption")]
    [TextArea] public string caption = "[Footsteps]";
    public float pulseSeconds = 0.18f;

    [Header("On-press settings (for OnPressCooldown mode)")]
    public float cooldownSeconds = 0.9f;

    [Header("Pulse timing (for Pulse mode)")]
    public bool autoMatchClip = true;
    public int beatsPerLoop = 2;
    public float fixedInterval = 0.5f;

    [Header("Detection")]
    public bool requireGrounded = false;
    public float minAudibleVolume = 0.01f;

    [Header("Yield to other subtitles")]
    [Tooltip("Extra buffer after other subtitles finish before footsteps can show again.")]
    public float otherBufferSeconds = 0.25f;

    AudioSource src;
    CharacterController cc;
    bool wasAudible;
    float timer;
    float cooldown;
    float suppressUntil;   // we won't show until Time.unscaledTime >= suppressUntil
    AudioClip lastClip;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        cc  = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        if (mode == Mode.Off || SubtitleUI.Instance == null || src == null) return;

        bool groundedOK = !requireGrounded || (cc && cc.isGrounded);
        bool audibleNow = groundedOK && src.isPlaying && src.volume > minAudibleVolume;

        // If ANY other subtitle is on screen, yield and hide ours if needed.
        if (SubtitleUI.Instance.IsShowingOther(caption))
        {
            SubtitleUI.Instance.HideIfTextEquals(caption);
            suppressUntil = Time.unscaledTime + SubtitleUI.Instance.VisibleRemaining + otherBufferSeconds;
            wasAudible = audibleNow;
            cooldown -= Time.deltaTime; // still tick down
            return; // bail early – don't emit footsteps
        }

        // If clip changed (W/A/S/D switch), reset local timing.
        if (src.clip != lastClip) { lastClip = src.clip; timer = 0f; }

        // WhileMoving behaviour (rarely used for footsteps, but supported)
        if (!wasAudible && audibleNow) { if (mode == Mode.WhileMoving) SubtitleUI.Instance.Show(caption); timer = 0f; }
        else if (wasAudible && !audibleNow) { if (mode == Mode.WhileMoving) SubtitleUI.Instance.Hide(); }

        cooldown -= Time.deltaTime;

        // Respect suppression window set by other subtitles
        if (Time.unscaledTime < suppressUntil) { wasAudible = audibleNow; return; }

        if (audibleNow)
        {
            if (mode == Mode.Pulse)
            {
                float interval = GetInterval();
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    SubtitleUI.Instance.ShowForSeconds(caption, pulseSeconds);
                    timer = Mathf.Max(0.1f, interval);
                }
            }
            else if (mode == Mode.OnPressCooldown)
            {
                if (cooldown <= 0f && MovementPressedThisFrame())
                {
                    SubtitleUI.Instance.ShowForSeconds(caption, pulseSeconds);
                    cooldown = Mathf.Max(0.1f, cooldownSeconds);
                }
            }
        }

        wasAudible = audibleNow;
    }

    float GetInterval()
    {
        if (!autoMatchClip || src.clip == null || beatsPerLoop <= 0)
            return Mathf.Clamp(fixedInterval, 0.1f, 1.5f);
        return Mathf.Clamp(src.clip.length / Mathf.Max(1, beatsPerLoop), 0.1f, 1.5f);
    }

    bool MovementPressedThisFrame()
    {
        var kb = Keyboard.current; if (kb == null) return false;
        return kb.wKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame ||
               kb.sKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame ||
               kb.upArrowKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame ||
               kb.downArrowKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame;
    }
}
