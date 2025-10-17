using UnityEngine;

public class LightFlickerAnomaly : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light targetLight;

    [Header("Flicker Settings")]
    [SerializeField] private float minFlickerInterval = 0.05f;
    [SerializeField] private float maxFlickerInterval = 0.3f;
    [SerializeField] private float minIntensity = 0.0f;
    [SerializeField] private float maxIntensity = 1.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool playAudio = true;

    [Header("Anomaly Control")]
    [SerializeField] private bool isFlickering = false;

    private float originalIntensity;
    private float nextFlickerTime;

    private void Start()
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

        // Store original intensity
        if (targetLight != null)
        {
            originalIntensity = targetLight.intensity;
            maxIntensity = originalIntensity;
        }

        // Start flickering if enabled
        if (isFlickering)
        {
            StartFlickering();
        }
    }

    private void Update()
    {
        if (isFlickering && targetLight != null)
        {
            if (Time.time >= nextFlickerTime)
            {
                // Randomly change light intensity
                targetLight.intensity = Random.Range(minIntensity, maxIntensity);

                // Set next flicker time
                nextFlickerTime = Time.time + Random.Range(minFlickerInterval, maxFlickerInterval);
            }
        }
    }

    // Call this method to start the anomaly
    public void StartFlickering()
    {
        isFlickering = true;
        nextFlickerTime = Time.time;

        // Start audio if enabled
        if (playAudio && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // Call this method to stop the anomaly
    public void StopFlickering()
    {
        isFlickering = false;

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

    // Toggle flickering on/off
    public void ToggleFlickering()
    {
        if (isFlickering)
        {
            StopFlickering();
        }
        else
        {
            StartFlickering();
        }
    }
}