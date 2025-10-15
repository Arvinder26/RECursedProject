using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

[DisallowMultipleComponent]
public class AnomalyAudioController : MonoBehaviour
{
    [Header("Sources (assign on prefab)")]
    public AudioSource spawnSource;   // 3D one-shot source (no Clip assigned)
    public AudioSource auraSource;    // 3D looping source (Clip = anomalytheme_loop_mono.wav, Loop=ON)

    [Header("Clips")]
    public AudioClip spawnStinger;    // anomalySpawn_tight_mono.wav
    public AudioClip auraLoop;        // anomalytheme_loop_mono.wav (optional if already on auraSource)

    [Header("Mixer (optional)")]
    public AudioMixerGroup spawnMixerGroup;
    public AudioMixerGroup auraMixerGroup;

    [Header("Tuning")]
    [Range(0f, 0.2f)] public float pitchJitter = 0.03f;
    [Range(0f, 1f)] public float auraTargetVolume = 0.35f;
    [Min(0.05f)] public float fadeTime = 0.6f;

    [Header("Occlusion (optional)")]
    public AudioLowPassFilter auraLowPass;   // add component on same GO; drag here
    [Range(200, 22000)] public float occludedCutoff = 1000f; // muffled through walls
    [Range(200, 22000)] public float clearCutoff = 22000f;   // clear line-of-sight
    [Tooltip("0 = fully occluded, 1 = fully clear")]
    [Range(0f, 1f)] public float occlusionFactor = 1f;

    Coroutine auraFader;

    void Reset()
    {
        // Auto-find sources if missing
        var sources = GetComponents<AudioSource>();
        if (sources.Length > 0) auraSource = sources[0];
        if (sources.Length > 1) spawnSource = sources[1];
        auraLowPass = GetComponent<AudioLowPassFilter>();
    }

    void Awake()
    {
        if (spawnMixerGroup && spawnSource) spawnSource.outputAudioMixerGroup = spawnMixerGroup;
        if (auraMixerGroup && auraSource)  auraSource.outputAudioMixerGroup  = auraMixerGroup;

        if (auraSource)
        {
            if (auraLoop) auraSource.clip = auraLoop;
            auraSource.loop = true;
            auraSource.playOnAwake = false;
            auraSource.volume = 0f; // fade in when activated
        }
        ApplyOcclusionInstant(); // set starting LPF
    }

    // --- Public API ---

    /// Play the spawn sting immediately (one-shot).
    public void PlaySpawn()
    {
        if (!spawnSource || !spawnStinger) return;
        spawnSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        spawnSource.PlayOneShot(spawnStinger);
    }

    /// Toggle the looping aura with a smooth fade.
    public void SetActive(bool active)
    {
        if (!auraSource) return;
        if (auraFader != null) StopCoroutine(auraFader);
        float target = active ? auraTargetVolume : 0f;
        auraFader = StartCoroutine(FadeAura(target));
        if (active && !auraSource.isPlaying)
            auraSource.Play();
    }

    /// Set occlusion blend (0 = behind walls, 1 = clear LOS). Call from your LOS checker.
    public void SetOcclusion(float factor)
    {
        occlusionFactor = Mathf.Clamp01(factor);
        ApplyOcclusionInstant();
    }

    // --- Internals ---

    IEnumerator FadeAura(float target)
    {
        float start = auraSource.volume;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.05f, fadeTime);
            auraSource.volume = Mathf.Lerp(start, target, t);
            yield return null;
        }
        auraSource.volume = target;
        if (Mathf.Approximately(target, 0f)) auraSource.Stop();
    }

    void ApplyOcclusionInstant()
    {
        if (!auraLowPass) return;
        // Lerp cutoff between occluded and clear
        float cutoff = Mathf.Lerp(occludedCutoff, clearCutoff, occlusionFactor);
        auraLowPass.cutoffFrequency = cutoff;
    }
}
