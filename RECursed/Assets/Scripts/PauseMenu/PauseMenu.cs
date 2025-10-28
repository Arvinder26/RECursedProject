using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject PauseMenuUI;
    public GameObject PauseInfo;
    public GameObject TabletInfo;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (GameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // Resumes game loop when called
    public void ResumeGame()
    {
        PauseMenuUI.SetActive(false);
        PauseInfo.SetActive(true);
        TabletInfo.SetActive(true);

        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Opens pause menu when called
    void PauseGame()
    {
        PauseMenuUI.SetActive(true);
        PauseInfo.SetActive(false);
        TabletInfo.SetActive(false);

        Time.timeScale = 0f;
        GameIsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Returns to main menu while in pause menu
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenu");
    }
}
