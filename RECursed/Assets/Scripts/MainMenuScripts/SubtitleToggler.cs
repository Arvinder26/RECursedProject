using UnityEngine;

public class SubtitleToggler : MonoBehaviour
{
    private const string PrefKey = "SubtitlesEnabled";
    public void ToggleSubtitles()
    {
        bool newState = !IsEnabled();
        PlayerPrefs.SetInt(PrefKey, newState ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"[{Time.time:F2}] Subtitles: {(newState ? "ON ✅" : "OFF ❌")}");
    }

    public bool IsEnabled()
    {
        return PlayerPrefs.GetInt(PrefKey, 1) == 1; // Default ON
    }
}
