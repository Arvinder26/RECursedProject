using UnityEngine;

/// <summary>
/// Dumb/simple "You Lost" screen controller.
/// I keep the actual UI panel assigned so I can enable/disable it here,
/// and I optionally pause the game when it shows.
/// </summary>
public class LossScreen : MonoBehaviour
{
    // The GameObject that contains my "You Lost" UI.
    [SerializeField] private GameObject panel;

    // If this is on, I freeze time when the screen shows.
    [SerializeField] private bool pauseOnShow = true;

    void Awake()
    {
        // Start hidden. I don't want this to flash on scene load.
        if (panel) panel.SetActive(false);
    }

    /// <summary>
    /// Show the loss panel and set up the input state so the player isn't stuck.
    /// </summary>
    public void Show()
    {
        if (panel) panel.SetActive(true);

        // UI best practices: unlock and show the cursor for menu interaction.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseOnShow) Time.timeScale = 0f;
    }

    /// <summary>
    /// Hide the loss panel and unpause (used when restarting/continuing).
    /// </summary>
    public void Hide()
    {
        if (panel) panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
