using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeSetting : MonoBehaviour
{
    public Slider masterSlider;
    private const string MasterKey = "MasterVolume";

    void Awake()
    {
        float savedVolume = PlayerPrefs.GetFloat(MasterKey, 1f);
        masterSlider.value = savedVolume;

        AudioListener.volume = savedVolume;
    }

    public void OnMasterSliderChanged(float value)
    {
        float clampedValue = Mathf.Clamp01(value); // ensure between 0 and 1
        AudioListener.volume = clampedValue;

        PlayerPrefs.SetFloat(MasterKey, clampedValue);
        PlayerPrefs.Save();

        Debug.Log("Master Volume Saved: " + clampedValue);
    }
}
