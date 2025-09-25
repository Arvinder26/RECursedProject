using UnityEngine;

public class LossScreen : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private bool pauseOnShow = true;

    void Awake()
    {
        if (panel) panel.SetActive(false);
    }

    public void Show()
    {
        if (panel) panel.SetActive(true);

        // UI best practices when showing a fail/game-over screen:
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseOnShow) Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
