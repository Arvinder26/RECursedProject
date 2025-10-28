using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSetting : MonoBehaviour
{
    public Slider musicSlider;
    private const string MusicKey = "MusicVolume";

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
        musicSlider.value = savedVolume;
        ApplyVolume(savedVolume);
    }

    public void OnMusicSliderChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(MusicKey, value);
        PlayerPrefs.Save();
    }

    void ApplyVolume(float value)
    {
        var musicSources = GameObject.FindGameObjectsWithTag("Music");
        if (musicSources.Length == 0)
        {
            Debug.Log("No music audio sources found in the scene.");
        }

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
