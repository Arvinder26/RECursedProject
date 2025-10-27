using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SFXVolumeSetting : MonoBehaviour
{
    public Slider sfxSlider;
    private const string SFXKey = "SFXVolume";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(SFXKey, 1f);
        if (sfxSlider != null)
            sfxSlider.value = savedVolume;

        // Apply immediately, even on first scene load
        StartCoroutine(ApplyVolumeNextFrame(savedVolume));
    }

    public void OnSFXSliderChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(SFXKey, value);
        PlayerPrefs.Save();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        float savedVolume = PlayerPrefs.GetFloat(SFXKey, 1f);
        StartCoroutine(ApplyVolumeNextFrame(savedVolume));
    }

    private IEnumerator ApplyVolumeNextFrame(float value)
    {
        yield return null; // wait one frame for all objects to exist
        ApplyVolume(value);
    }

    private void ApplyVolume(float value)
    {
        // Find all AudioSources, including inactive ones
        AudioSource[] allSources = Resources.FindObjectsOfTypeAll<AudioSource>();

        bool foundAny = false;
        foreach (var audio in allSources)
        {
            if (audio.gameObject.CompareTag("SFX"))
            {
                audio.volume = value;
                foundAny = true;
                Debug.Log($"[SFXVolumeSetting] Adjusted SFX on: {audio.gameObject.name}");
            }
        }

        if (!foundAny)
        {
            Debug.Log("[SFXVolumeSetting] No SFX audio sources found in scene: " + SceneManager.GetActiveScene().name);
        }
    }
}
