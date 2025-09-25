using UnityEngine;
using UnityEngine.UI;

public class BrightnessSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;

    void Start()
    {
        //Remove if player brightness should be saved and kept for next game launch
        PlayerPrefs.DeleteKey("Brightness");

        float saved;
        if (PlayerPrefs.HasKey("Brightness"))
        {
            saved = PlayerPrefs.GetFloat("Brightness");
        }
        else
        {
            saved = 1f;
            PlayerPrefs.SetFloat("Brightness", saved);
        }

        slider.value = saved;
        BrightnessManager.Instance.SetBrightness(saved);
        slider.onValueChanged.AddListener(OnSliderChanged);
    }



    void OnSliderChanged(float value)
    {
        BrightnessManager.Instance.SetBrightness(value);
        PlayerPrefs.SetFloat("Brightness", value);
    }
}
