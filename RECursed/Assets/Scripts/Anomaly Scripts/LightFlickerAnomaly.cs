using UnityEngine;

// Randomly flickers a light for a limited time windo
public class LightFlickerAnomaly : MonoBehaviour, IAnomaly
{
    [Header("Anomaly Info")]
    [SerializeField] private Room room = Room.Bathroom; // Room this belongs to.
    [SerializeField] private float deadline = 60f; // Seconds to report before auto-revert.

    [Header("Light Settings")]
    [SerializeField] private Light targetLight; // Light to flicker.

    [Header("Flicker Settings")]
    [SerializeField] private float minFlickerInterval = 0.05f;
    [SerializeField] private float maxFlickerInterval = 0.3f;
    [SerializeField] private float minIntensity = 0.0f;
    [SerializeField] private float maxIntensity = 1.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private bool playAudio = true; // Toggle audio.

    [Header("Battery Penalty")]
    [SerializeField] private SegmentBattery battery;
    [SerializeField] private int batteryPenalty = 1;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    // IAnomaly interface implementation
    public Room Room => room;
    public AnomalyType Type => AnomalyType.LightFlicker;
    public bool IsActive { get; private set; }

    private float originalIntensity;
    private float nextFlickerTime;
    private float activationTime;

    private void Awake()
    {
        // Get components if not assigned
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (!battery)
        {
            battery = FindObjectOfType<SegmentBattery>();
        }

        // Store original intensity
        if (targetLight != null)
        {
            originalIntensity = targetLight.intensity;
            maxIntensity = originalIntensity;
        }

        // Make sure it starts inactive
        IsActive = false;
    }

    private void Update()
    {
        if (!IsActive) return;

        // Check if deadline has passed
        if (Time.time >= activationTime + deadline)
        {
            if (debugMode) Debug.Log($"[LightFlicker] DEADLINE EXPIRED! Battery penalty applied.");
            
            // Apply battery penalty
            if (battery)
            {
                battery.Consume(batteryPenalty);
            }

            // Auto-revert after deadline
            Revert();
            return;
        }

        // Handle flickering
        if (targetLight != null && Time.time >= nextFlickerTime)
        {
            // Randomly change light intensity
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);

            // Set next flicker time
            nextFlickerTime = Time.time + Random.Range(minFlickerInterval, maxFlickerInterval);
        }
    }

    // IAnomaly interface methods
    public void Trigger()
    {
        if (IsActive)
        {
            if (debugMode) Debug.LogWarning($"[LightFlicker] {room} already active!");
            return;
        }

        IsActive = true;
        activationTime = Time.time;
        nextFlickerTime = Time.time;

        if (debugMode) Debug.Log($"[LightFlicker] TRIGGERED in {room}. Deadline: {deadline}s");

        // Start flickering
        if (targetLight != null)
        {
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);
        }

        // Start audio if enabled
        if (playAudio && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void Revert()
    {
        if (!IsActive)
        {
            if (debugMode) Debug.LogWarning($"[LightFlicker] {room} not active, cannot revert!");
            return;
        }

        IsActive = false;

        if (debugMode) Debug.Log($"[LightFlicker] REVERTED in {room}");

        // Restore original intensity
        if (targetLight != null)
        {
            targetLight.intensity = originalIntensity;
        }

        // Stop audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public float GetTimeRemaining()
    {
        if (!IsActive) return 0f;

        float elapsed = Time.time - activationTime;
        float remaining = deadline - elapsed;
        return Mathf.Max(0f, remaining);
    }

    // Manual control methods (for testing)
    [ContextMenu("Test Trigger")]
    private void TestTrigger()
    {
        Trigger();
    }

    [ContextMenu("Test Revert")]
    private void TestRevert()
    {
        Revert();
    }
}