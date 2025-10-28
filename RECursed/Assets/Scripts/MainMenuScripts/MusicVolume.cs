using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSetting : MonoBehaviour
{
    public Slider musicSlider;
    private const string MusicKey = "MusicVolume";

    void Start()
    {
        // Load saved value
        float savedVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
        musicSlider.value = savedVolume;
        
        // Apply volume to all music sources
        ApplyVolume(savedVolume);
    }

    // Called if music slider changes
    public void OnMusicSliderChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(MusicKey, value);
        PlayerPrefs.Save();
    }

    // Update volume for AudioSources tagged with "Music"
    void ApplyVolume(float value)
    {
        var musicSources = GameObject.FindGameObjectsWithTag("Music");
        if (musicSources.Length == 0)
        {
            Debug.Log("No music audio sources found in the scene.");
        }

        // Apply volume for each found source
        foreach (var go in musicSources)
        {
            var audio = go.GetComponent<AudioSource>();
            if (audio != null) {
                    audio.volume = value;
                    Debug.Log($"[MusicVolumeSetting] Found and adjusted music audio on: {go.name}");
            }
            else
            {
                Debug.Log("GameObject tagged 'Music' has no AudioSource: " + go.name);
            }

        }
    }
}
