using UnityEngine;

public class SubtitleToggler : MonoBehaviour
{
    // Player prefs to save subtitle setting
    private const string PrefKey = "SubtitlesEnabled";

    // Called when subtitles button is pressed
    public void ToggleSubtitles()
    {
        bool newState = !IsEnabled();
        PlayerPrefs.SetInt(PrefKey, newState ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"[{Time.time:F2}] Subtitles: {(newState ? "ON ✅" : "OFF ❌")}");
    }

    // Return true if subtitles are enabled
    public bool IsEnabled()
    {
        return PlayerPrefs.GetInt(PrefKey, 1) == 1; // Default ON
    }
}
