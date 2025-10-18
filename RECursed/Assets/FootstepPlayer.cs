using UnityEngine;

[DisallowMultipleComponent]
public class FootstepPlayer : MonoBehaviour
{
    public AudioSource audioSource;      // on player
    public AudioClip[] footstepClips;    // can be just 1 (e.g., footsteps_loop_mono)
    [Range(0f, 0.2f)] public float volumeJitter = 0.05f;
    [Range(0f, 0.2f)] public float pitchJitter  = 0.07f;

    [Header("Overlap Guard")]
    public float retriggerGuard = 0.08f; // ignore hits too close together
    float lastPlayTime;

    void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = GetComponentInChildren<AudioSource>();
        if (audioSource) { audioSource.loop = false; audioSource.spatialBlend = 1f; }
    }

    public void PlayFootstep()
    {
        if (!audioSource || footstepClips == null || footstepClips.Length == 0) return;
        if (Time.time - lastPlayTime < retriggerGuard) return;

        var idx = Mathf.Clamp(Random.Range(0, footstepClips.Length), 0, footstepClips.Length - 1);
        var clip = footstepClips[idx];
        if (!clip) return;

        audioSource.pitch  = 1f + Random.Range(-pitchJitter,  pitchJitter);
        audioSource.volume = 0.9f + Random.Range(-volumeJitter, volumeJitter);

        // Replace any playing step (prevents overlap)
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();

        lastPlayTime = Time.time;
    }
}
