using UnityEngine;
using UnityEngine.UI;

public class BrightnessManager : MonoBehaviour
{
    public static BrightnessManager Instance;

    [Header("Brightness Overlay")]
    [SerializeField] private Image brightnessOverlay;

    private float brightness = 1f;

    void Awake()
    {
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

    public void SetBrightness(float value)
    {
        brightness = Mathf.Clamp01(value);
        UpdateBrightness();
    }

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

    public float GetBrightness() => brightness;
}
