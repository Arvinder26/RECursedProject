using UnityEngine;

/// <summary>
/// Simple "You Lost" screen controller.
/// - Shows a panel (on top of everything) and optionally pauses the game
/// - Hides any conflicting UI (e.g., Anomaly Menu) while visible
/// - Restores state when hidden
/// </summary>
public class LossScreen : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;          // Your loss panel GameObject
    [SerializeField] private bool pauseOnShow = true;   // Freeze time when showing

    [Header("Hide these while loss is shown")]
    [Tooltip("Drag the AnomalyMenu root, Tablet UI, etc., here so they get hidden during the loss screen.")]
    [SerializeField] private GameObject[] hideWhileShown;

    // optional: if the panel has a CanvasGroup, we’ll use it to block clicks
    CanvasGroup _group;

    void Awake()
    {
        if (panel)
        {
            _group = panel.GetComponent<CanvasGroup>();
            panel.SetActive(false);
        }
    }

    /// <summary>Show the loss UI, block input behind it, and pause if requested.</summary>
    public void Show()
    {
        if (!panel) return;

        // Put panel visually on top of its siblings (same Canvas).
        panel.transform.SetAsLastSibling();

        // Ensure clicks don't leak through if you added a CanvasGroup.
        if (_group)
        {
            _group.alpha = 1f;
            _group.interactable = true;
            _group.blocksRaycasts = true;
        }

        // Hide competing UI while this is up (e.g., Anomaly menu).
        if (hideWhileShown != null)
        {
            foreach (var go in hideWhileShown)
                if (go) go.SetActive(false);
        }

        panel.SetActive(true);

        // Pause + free cursor so the player can read the screen.
        if (pauseOnShow) Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Hide the loss UI and restore anything we hid.</summary>
    public void Hide()
    {
        if (!panel) return;

        if (_group)
        {
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        panel.SetActive(false);

        // Restore the UI we hid.
        if (hideWhileShown != null)
        {
            foreach (var go in hideWhileShown)
                if (go) go.SetActive(true);
        }

        Time.timeScale = 1f;
    }
}
