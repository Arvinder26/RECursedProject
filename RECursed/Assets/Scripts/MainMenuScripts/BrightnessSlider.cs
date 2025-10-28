using UnityEngine;
using UnityEngine.UI;

public class BrightnessSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;

    void Start()
    {
        float saved;

        // Load saved brightness
        if (PlayerPrefs.HasKey("Brightness"))
        {
            saved = PlayerPrefs.GetFloat("Brightness");
        }
        else
        {
            saved = 1f;
            PlayerPrefs.SetFloat("Brightness", saved);
        }

        // Sync slider with the current brightness
        slider.value = saved;

        // Apply brightness to screen
        BrightnessManager.Instance.SetBrightness(saved);
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    // Called when brightness slider is moved
    void OnSliderChanged(float value)
    {
        BrightnessManager.Instance.SetBrightness(value);
        PlayerPrefs.SetFloat("Brightness", value);
    }
}
