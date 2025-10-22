using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepSubtitles : MonoBehaviour
{
    public enum Mode { Off, WhileMoving, Pulse }

    [Header("Mode")]
    public Mode mode = Mode.Pulse;

    [Header("Caption")]
    [TextArea] public string caption = "[Footsteps]";
    [Tooltip("How long a single pulse subtitle stays visible.")]
    public float pulseSeconds = 0.18f;

    [Header("Pulse Timing")]
    [Tooltip("Derive the step interval from the clip length / beatsPerLoop.")]
    public bool autoMatchClip = true;
    [Tooltip("How many 'steps' per loop of the clip (e.g., 2 for left/right).")]
    public int beatsPerLoop = 2;
    [Tooltip("Used when autoMatchClip = false.")]
    public float fixedInterval = 0.5f;

    [Header("Detection")]
    [Tooltip("Minimum source volume considered 'audible' (post-fade).")]
    public float minAudibleVolume = 0.01f;

    private AudioSource src;
    private bool wasAudible;
    private float timer;
    private AudioClip lastClip;

    void Awake()
    {
        src = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (mode == Mode.Off || SubtitleUI.Instance == null || src == null)
            return;

        // "Audible" means: playing and not effectively silent.
        bool audibleNow = src.isPlaying && src.volume > minAudibleVolume;

        // Handle clip change (e.g., W/A/S/D swap)
        if (src.clip != lastClip)
        {
            lastClip = src.clip;
            timer = 0f; // restart pulse timing on new clip
        }

        // Enter/exit transitions
        if (!wasAudible && audibleNow)
        {
            if (mode == Mode.WhileMoving)
                SubtitleUI.Instance.Show(caption);
            timer = 0f; // reset pulse timer
        }
        else if (wasAudible && !audibleNow)
        {
            if (mode == Mode.WhileMoving)
                SubtitleUI.Instance.Hide();
        }

        // Pulse mode – emit short captions at step interval while audible
        if (mode == Mode.Pulse && audibleNow)
        {
            float interval = GetStepInterval();
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SubtitleUI.Instance.ShowForSeconds(caption, pulseSeconds);
                timer = Mathf.Max(0.1f, interval);
            }
        }

        wasAudible = audibleNow;
    }

    float GetStepInterval()
    {
        if (!autoMatchClip || src.clip == null || beatsPerLoop <= 0)
            return Mathf.Clamp(fixedInterval, 0.1f, 1.5f);

        // Derive interval from clip length (one loop == left+right or however many beats you set)
        float perBeat = src.clip.length / Mathf.Max(1, beatsPerLoop);
        return Mathf.Clamp(perBeat, 0.1f, 1.5f);
    }
}
