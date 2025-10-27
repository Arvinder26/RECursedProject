using UnityEngine;
using UnityEngine.UI;

public class FOVSetting : MonoBehaviour
{
    private const string FOVKey = "PlayerFOV";
    public Slider fovSlider;

    [Header("Safe FOV range")]
    public float minFOV = 30f;
    public float maxFOV = 110f;

    void Start()
    {
        fovSlider.minValue = minFOV;
        fovSlider.maxValue = maxFOV;

        float savedFOV = PlayerPrefs.GetFloat(FOVKey, 60f);
        savedFOV = Mathf.Clamp(savedFOV, minFOV, maxFOV);
        fovSlider.value = savedFOV;
    }

    public void OnFOVSliderChanged(float value)
    {
        float clampedValue = Mathf.Clamp(value, minFOV, maxFOV);

        PlayerPrefs.SetFloat(FOVKey, clampedValue);
        PlayerPrefs.Save();

        Debug.Log("FOV Saved: " + clampedValue);
    }
}
