using UnityEngine;
using UnityEngine.UI;

public class SFXVolumeSetting : MonoBehaviour
{
    public Slider sfxSlider;
    private const string SFXKey = "SFXVolume";

    void Start()
    {
        // Load saved volume (default: full volume)
        float savedVolume = PlayerPrefs.GetFloat(SFXKey, 1f);

        // Sync slider with saved value
        if (sfxSlider != null)
            sfxSlider.value = savedVolume;

        // Apply volume to all SFX sources right away
        ApplyVolume(savedVolume);
    }

    // Called when slider value changes
    public void OnSFXSliderChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(SFXKey, value);
        PlayerPrefs.Save();
    }

    // Update all AudioSources tagged "SFX"
    private void ApplyVolume(float value)
    {
        var sfxSources = GameObject.FindGameObjectsWithTag("SFX");

        if (sfxSources.Length == 0)
        {
            Debug.Log($"No SFX sources found in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        }

        // Apply volume for each found source
        foreach (var go in sfxSources)
        {
            var audio = go.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.volume = value;
                Debug.Log($"[SFXVolumeSetting] Adjusted SFX on: {go.name}");
            }
            else
            {
                Debug.Log($"GameObject tagged 'SFX' has no AudioSource: {go.name}");
            }
        }
    }
}
