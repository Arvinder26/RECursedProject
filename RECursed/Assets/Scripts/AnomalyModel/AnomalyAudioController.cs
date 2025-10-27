using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

[DisallowMultipleComponent]
public class AnomalyAudioController : MonoBehaviour
{
    // --- Assign on the prefab ---
    [Header("Sources")]
    [Tooltip("3D one-shot for the spawn sting (leave Clip empty).")]
    public AudioSource spawnSource;

    [Tooltip("3D looping source for the aura/theme (Clip = boosted loop, Loop=ON).")]
    public AudioSource auraSource;

    [Header("Clips")]
    [Tooltip("Use the processed mono spawn (e.g., anomalySpawn_4s_mono.wav).")]
    public AudioClip spawnStinger;

    [Tooltip("Use the boosted seamless mono loop (e.g., anomalytheme_loop_boost_mono.wav).")]
    public AudioClip auraLoop;

    [Header("Mixer (optional)")]
    public AudioMixerGroup spawnMixerGroup;
    public AudioMixerGroup auraMixerGroup;

    [Header("Tuning")]
    [Range(0f, 0.2f)] public float pitchJitter = 0.03f;
    [Range(0f, 1f)]  public float auraTargetVolume = 0.65f; // louder by default
    [Min(0.05f)]     public float fadeTime = 0.6f;

    [Header("Distance Loudness")]
    [Tooltip("Drag your Player (or Main Camera) here.")]
    public Transform player;

    [Tooltip("Match these with auraSource Min/Max Distance.")]
    public float minDistance = 3f;
    public float maxDistance = 30f;

    [Tooltip("0 = no distance scaling, 1 = full scaling by curve.")]
    [Range(0f, 1f)] public float distanceVolStrength = 1f;

    [Tooltip("x-axis: near(0)→far(1), y-axis: volume scale (1 near, 0 far).")]
    public AnimationCurve distanceToVol = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Occlusion (optional)")]
    [Tooltip("Add an AudioLowPassFilter to the same GameObject and assign here.")]
    public AudioLowPassFilter auraLowPass;

    [Range(200, 22000)] public float occludedCutoff = 1000f; // muffled
    [Range(200, 22000)] public float clearCutoff    = 22000f; // clear
    [Tooltip("0 = fully occluded, 1 = clear LOS")]
    [Range(0f, 1f)] public float occlusionFactor = 1f;

    // --- Internals ---
    Coroutine auraFader;
    bool auraRequestedActive;

    void Reset()
    {
        // Try to auto-wire sources and LPF if present
        var sources = GetComponents<AudioSource>();
        if (sources.Length > 0) auraSource = sources[0];
        if (sources.Length > 1) spawnSource = sources[1];
        auraLowPass = GetComponent<AudioLowPassFilter>();
    }

    void Awake()
    {
        if (spawnSource && spawnMixerGroup) spawnSource.outputAudioMixerGroup = spawnMixerGroup;
        if (auraSource  && auraMixerGroup)  auraSource.outputAudioMixerGroup  = auraMixerGroup;

        if (auraSource)
        {
            if (auraLoop) auraSource.clip = auraLoop;
            auraSource.loop = true;
            auraSource.playOnAwake = false;
            auraSource.volume = 0f; // will fade in on SetActive(true)
            auraSource.spatialBlend = 1f;
        }

        if (spawnSource)
        {
            spawnSource.playOnAwake = false;
            spawnSource.spatialBlend = 1f;
        }

        ApplyOcclusionInstant();
    }

    void Update()
    {
        // Distance-based volume scaling for the aura while it's active
        if (!auraSource || !player) return;

        // Only update while aura is meant to be active and the source is playing
        if (!auraRequestedActive || !auraSource.isPlaying) return;

        float d = Vector3.Distance(player.position, auraSource.transform.position);
        float t = Mathf.InverseLerp(minDistance, maxDistance, d); // 0 near, 1 far
        float scale = distanceToVol.Evaluate(t);                  // 1 near, 0 far

        // Blend between base volume and distance-scaled volume
        float baseVol = auraTargetVolume;
        float targetVol = Mathf.Lerp(baseVol, baseVol * scale, distanceVolStrength);

        // Clamp to a safe range
        auraSource.volume = Mathf.Clamp01(targetVol);
    }

    // --- Public API ---

    /// <summary>Play the spawn sting immediately (random slight pitch).</summary>
    public void PlaySpawn()
    {
        if (!spawnSource || !spawnStinger) return;

        spawnSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        // Replace any residual playback to avoid overlap smearing
        spawnSource.Stop();
        spawnSource.clip = spawnStinger;
        spawnSource.Play();
    }

    /// <summary>Toggle the looping aura with a smooth fade.</summary>
    public void SetActive(bool active)
    {
        auraRequestedActive = active;
        if (!auraSource) return;

        if (auraLoop && auraSource.clip != auraLoop)
            auraSource.clip = auraLoop;

        if (auraFader != null) StopCoroutine(auraFader);
        float target = active ? auraTargetVolume : 0f;
        auraFader = StartCoroutine(FadeAura(target));

        if (active && !auraSource.isPlaying)
            auraSource.Play();
    }

    /// <summary>0 = occluded, 1 = clear. Call from a line-of-sight checker if you have one.</summary>
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
        float dur = Mathf.Max(0.05f, fadeTime);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;

            // While fading, still apply distance scaling so volume feels consistent
            float d = (player && auraSource) ? Vector3.Distance(player.position, auraSource.transform.position) : minDistance;
            float tt = Mathf.InverseLerp(minDistance, maxDistance, d);
            float scale = distanceToVol.Evaluate(tt);
            float baseVol = Mathf.Lerp(start, target, t);
            float distVol = Mathf.Lerp(baseVol, baseVol * scale, distanceVolStrength);

            auraSource.volume = Mathf.Clamp01(distVol);
            yield return null;
        }

        auraSource.volume = Mathf.Clamp01(target);
        if (Mathf.Approximately(target, 0f))
            auraSource.Stop();

        auraFader = null;
    }

    void ApplyOcclusionInstant()
    {
        if (!auraLowPass) return;
        auraLowPass.cutoffFrequency = Mathf.Lerp(occludedCutoff, clearCutoff, occlusionFactor);
    }
}
