using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CaptionWhenPlayed : MonoBehaviour
{
    public AudioSource source;
    [TextArea] public string caption = "[Anomaly manifests]";
    public float seconds = 1.2f;
    bool wasPlaying;

    void Reset() { source = GetComponent<AudioSource>(); }

    void Update() {
        if (!source || SubtitleUI.Instance == null) return;
        bool now = source.isPlaying;
        if (now && !wasPlaying) SubtitleUI.Instance.ShowForSeconds(caption, seconds);
        wasPlaying = now;
    }
}
