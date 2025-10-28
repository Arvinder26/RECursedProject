using UnityEngine;
using UnityEngine.UI;

public class BrightnessManager : MonoBehaviour
{
    // Singleton instance for global access across scenes
    public static BrightnessManager Instance;

    // Image used a dark overlay to simulate brightness
    [Header("Brightness Overlay")]
    [SerializeField] private Image brightnessOverlay;

    private float brightness = 1f;

    void Awake()
    {
        // Setup instance so is isn't destroyed across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Update brightness and clamp slider to only valid range
    public void SetBrightness(float value)
    {
        brightness = Mathf.Clamp01(value);
        UpdateBrightness();
    }

    // Apply brightness to the overlay transparency
    private void UpdateBrightness()
    {
        if (brightnessOverlay != null)
        {
            Color c = brightnessOverlay.color;
            c.a = 1f - brightness;
            brightnessOverlay.color = c;
        }
        else
        {
        }
    }
}
