using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mainMenuCanvas;
    public GameObject settingsCanvas;

    [Header("Settings References")]
    public GameObject visualSettings;
    public GameObject audioSettings;

    private void Start()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
    }

    public void loadNewGame(string sceneName)
    {
        Debug.Log("Loading gameplay scene");

        SceneManager.LoadScene(sceneName);
    }

    public void loadSettings()
    {
        Debug.Log("Opening settings");

        if (settingsCanvas != null) settingsCanvas.SetActive(true);
    }

    public void quitGame()
    {
        Debug.Log("Quitting game");

        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void showVisualSettings()
    {
        Debug.Log("Showing visual settings");

        if (audioSettings != null) audioSettings.SetActive(false);

        if (visualSettings != null) visualSettings.SetActive(true);
    }

    public void showAudioSettings()
    {
        Debug.Log("Showing audio settings");

        if (visualSettings != null) visualSettings.SetActive(false);

        if (audioSettings != null) audioSettings.SetActive(true);
    }

    public void returnToMenu()
    {
        Debug.Log("Returning back to main menu");

        if (settingsCanvas != null) settingsCanvas.SetActive(false);
    }


}
