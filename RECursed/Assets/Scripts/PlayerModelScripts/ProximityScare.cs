using UnityEngine;

[DisallowMultipleComponent]
public class ProximityScare : MonoBehaviour
{
    [Header("References")]
    public Transform player;            // drag Player or Main Camera
    public AudioSource scareSource;     // 3D AudioSource on the anomaly (Clip empty)
    public AudioClip scareClip;         // ghost_scare_close_mono.wav

    [Header("Trigger Zone (meters)")]
    public float triggerRadius = 8f;    // distance to fire
    public float exitRadius = 10f;      // must leave this to re-arm (hysteresis)

    [Header("Cooldown")]
    public float cooldownSeconds = 6f;  // min time between scares

    [Header("Tuning")]
    [Range(0f, 0.2f)] public float pitchJitter = 0.03f;

    float lastPlayTime = -999f;
    bool armed = true;

    void Reset()
    {
        scareSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!player || !scareSource || !scareClip) return;

        float d = Vector3.Distance(player.position, transform.position);
        float now = Time.time;

        if (armed && d <= triggerRadius && (now - lastPlayTime) >= cooldownSeconds)
        {
            PlayScare();
            armed = false;
            lastPlayTime = now;
        }
        else if (!armed && d >= exitRadius)
        {
            // Player left the area; re-arm once they're sufficiently far
            armed = true;
        }
    }

    void PlayScare()
    {
        scareSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        // Replace any current playback to avoid overlap
        scareSource.Stop();
        scareSource.clip = scareClip;
        scareSource.Play();
    }
}
