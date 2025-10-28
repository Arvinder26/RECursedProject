using UnityEngine;
using UnityEngine.UI;

public class DefaultSettingsButton : MonoBehaviour
{
    // UI References for settings
    public Slider fovSlider;       
    public Slider brightnessSlider;
    public Toggle subtitlesToggle;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    // Player prefs to save settings
    private const string FOVKey = "PlayerFOV";
    private const string BrightnessKey = "Brightness";
    private const string SubtitlesKey = "SubtitlesEnabled";
    private const string MasterKey = "MasterVolume";
    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";

    // Called when the player clicks "Reset to Default"
    public void ResetDefaults()
    {
        ResetFOV();
        ResetBrightness();
        ResetSubtitles();
        ResetMasterVolume();
        ResetMusicVolume();
        ResetSFXVolume();

        Debug.Log("Settings reset to defaults ✅");
    }

    // Resets to default FOV
    private void ResetFOV()
    {
        float defaultFOV = 60f;

        if (fovSlider != null)
            fovSlider.value = defaultFOV;

        PlayerPrefs.SetFloat(FOVKey, defaultFOV);
        PlayerPrefs.Save();

        Debug.Log("FOV reset to default: " + defaultFOV);
    }

    // Resets to default brightness level
    private void ResetBrightness()
    {
        float defaultBrightness = 1f;

        if (brightnessSlider != null)
            brightnessSlider.value = defaultBrightness;

        PlayerPrefs.SetFloat(BrightnessKey, defaultBrightness);
        PlayerPrefs.Save();

        Debug.Log("Brightness reset to default: " + defaultBrightness);
    }

    // Resets subtitles to ON
    private void ResetSubtitles()
    {
        int defaultSubtitles = 1;

        if (subtitlesToggle != null)
            subtitlesToggle.isOn = true;

        PlayerPrefs.SetInt(SubtitlesKey, defaultSubtitles);
        PlayerPrefs.Save();

        Debug.Log("Subtitles reset to default: ON ✅");
    }

    // Reset master volume
    private void ResetMasterVolume()
    {
        float defaultVolume = 0.5f;
        if (masterSlider != null)
            masterSlider.value = defaultVolume;

        PlayerPrefs.SetFloat(MasterKey, defaultVolume);
        PlayerPrefs.Save();

        Debug.Log("Master volume reset to default: " + defaultVolume);
    }

    // Reset music volume
    private void ResetMusicVolume()
    {
        float defaultVolume = 0.5f;
        if (musicSlider != null)
            musicSlider.value = defaultVolume;

        PlayerPrefs.SetFloat(MusicKey, defaultVolume);
        PlayerPrefs.Save();

        Debug.Log("Music volume reset to default: " + defaultVolume);
    }

    // Reset SFX volume
    private void ResetSFXVolume()
    {
        float defaultVolume = 0.5f;
        if (sfxSlider != null)
            sfxSlider.value = defaultVolume;

        PlayerPrefs.SetFloat(SFXKey, defaultVolume);
        PlayerPrefs.Save();

        Debug.Log("SFX volume reset to default: " + defaultVolume);
    }
}
