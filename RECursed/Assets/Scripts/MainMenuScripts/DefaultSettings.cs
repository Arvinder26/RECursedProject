using UnityEngine;
using UnityEngine.UI;

public class DefaultSettingsButton : MonoBehaviour
{
    public Slider fovSlider;       
    public Slider brightnessSlider;
    public Toggle subtitlesToggle;

    private const string FOVKey = "PlayerFOV";
    private const string BrightnessKey = "Brightness";
    private const string SubtitlesKey = "SubtitlesEnabled";

    // Called when the player clicks "Reset to Default"
    public void ResetDefaults()
    {
        ResetFOV();
        ResetBrightness();
        ResetSubtitles();
        Debug.Log("Settings reset to defaults ✅");
    }

    private void ResetFOV()
    {
        float defaultFOV = 60f;

        if (fovSlider != null)
            fovSlider.value = defaultFOV;

        PlayerPrefs.SetFloat(FOVKey, defaultFOV);
        PlayerPrefs.Save();

        Debug.Log("FOV reset to default: " + defaultFOV);
    }

    private void ResetBrightness()
    {
        float defaultBrightness = 1f;

        if (brightnessSlider != null)
            brightnessSlider.value = defaultBrightness;

        PlayerPrefs.SetFloat(BrightnessKey, defaultBrightness);
        PlayerPrefs.Save();

        Debug.Log("Brightness reset to default: " + defaultBrightness);
    }

    private void ResetSubtitles()
    {
        int defaultSubtitles = 1;

        if (subtitlesToggle != null)
            subtitlesToggle.isOn = true;

        PlayerPrefs.SetInt(SubtitlesKey, defaultSubtitles);
        PlayerPrefs.Save();

        Debug.Log("Subtitles reset to default: ON ✅");
    }

}
