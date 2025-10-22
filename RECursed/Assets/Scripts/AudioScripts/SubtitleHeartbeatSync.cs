using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SubtitleHeartbeatSync : MonoBehaviour
{
    public AudioSource source;
    [Tooltip("Beats per minute of the heartbeat thumps")]
    public float bpm = 72f;
    [Tooltip("How long the caption shows per thump")]
    public float showSeconds = 0.25f;
    [TextArea] public string caption = "[Heartbeat]";

    float period;
    float nextBeatTime;
    float lastTime;

    void Reset() { source = GetComponent<AudioSource>(); }

    void Start() {
        if (!source) source = GetComponent<AudioSource>();
        period = Mathf.Max(0.05f, 60f / Mathf.Max(1f, bpm));
        nextBeatTime = 0f;
        lastTime = 0f;
    }

    void Update() {
        if (!source || !source.clip || !source.isPlaying || SubtitleUI.Instance == null) return;

        float t = source.time;

        // Handle loop wrap
        if (t < lastTime) nextBeatTime = 0f;
        lastTime = t;

        // Trigger at beat boundaries
        while (t >= nextBeatTime) {
            SubtitleUI.Instance.ShowForSeconds(caption, showSeconds);
            nextBeatTime += period;
        }
    }
}
