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
    [SerializeField] private GameObject panel;          // loss panel GameObject
    [SerializeField] private bool pauseOnShow = true;   // Freeze time when showing

    [Header("Hide these while loss is shown")]
    [Tooltip("Drag the AnomalyMenu root, Tablet UI, etc., here so they get hidden during the loss screen.")]
    [SerializeField] private GameObject[] hideWhileShown;

    // Optional: if the panel has a CanvasGroup, we'll use it to block clicks
    private CanvasGroup _group;

    /// <summary>
    /// Initializes the loss screen system.
    /// Finds the CanvasGroup component and hides the panel by default.
    /// Validates that required references are assigned.
    /// </summary>
    void Awake()
    {
        // Coding Standards: Always use braces for if statements
        if (panel)
        {
            _group = panel.GetComponent<CanvasGroup>();
            panel.SetActive(false);
        }

        // Input Validation (Coding Standards: Validate Inspector-assigned values)
        ValidateSetup();
    }

    /// <summary>
    /// Validates the component setup and logs errors for missing critical references.
    /// Ensures the loss screen can function properly before gameplay begins.
    /// </summary>
    private void ValidateSetup()
    {
        // Validate critical panel reference
        if (!panel)
        {
            Debug.LogError("[LossScreen] VALIDATION FAILED: Panel GameObject is not assigned! Loss screen will not function.");
        }

        // Validate hideWhileShown array
        if (hideWhileShown == null || hideWhileShown.Length == 0)
        {
            Debug.LogWarning("[LossScreen] VALIDATION WARNING: No UI elements assigned to hide during loss screen. This may cause UI conflicts.");
        }
        else
        {
            // Check for null entries in the array
            int nullCount = 0;
            foreach (var go in hideWhileShown)
            {
                if (go == null)
                {
                    nullCount++;
                }
            }

            if (nullCount > 0)
            {
                Debug.LogWarning($"[LossScreen] VALIDATION WARNING: {nullCount} null entries found in hideWhileShown array.");
            }
        }

        // Log successful setup
        if (panel && _group)
        {
            Debug.Log("[LossScreen] Setup complete with CanvasGroup for click blocking.");
        }
        else if (panel && !_group)
        {
            Debug.LogWarning("[LossScreen] Panel exists but no CanvasGroup found. Clicks may leak through to underlying UI.");
        }
    }

    /// <summary>
    /// Shows the loss UI, blocks input behind it, and pauses the game if requested.
    /// Hides competing UI elements and ensures the loss panel is on top visually.
    /// Also unlocks the cursor so the player can interact with buttons.
    /// </summary>
    public void Show()
    {
        // Coding Standards: Always use braces for if statements
        if (!panel)
        {
            Debug.LogError("[LossScreen] Cannot show loss screen - panel is null!");
            return;
        }

        // Put panel visually on top of its siblings (within the same Canvas)
        panel.transform.SetAsLastSibling();

        // Ensure clicks don't leak through if you added a CanvasGroup
        if (_group)
        {
            _group.alpha = 1f;
            _group.interactable = true;
            _group.blocksRaycasts = true;
        }

        // Hide competing UI while this is up (e.g., Anomaly menu, Tablet)
        if (hideWhileShown != null)
        {
            foreach (var go in hideWhileShown)
            {
                if (go)
                {
                    go.SetActive(false);
                }
            }
        }

        // Activate the loss panel
        panel.SetActive(true);

        // Pause the game if requested
        if (pauseOnShow)
        {
            Time.timeScale = 0f;
        }

        // Free the cursor so the player can read the screen and click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[LossScreen] Loss screen is now visible.");
    }

    /// <summary>
    /// Hides the loss UI and restores anything we hid during Show().
    /// Unpauses the game and restores competing UI elements.
    /// </summary>
    public void Hide()
    {
        // Coding Standards: Always use braces for if statements
        if (!panel)
        {
            Debug.LogError("[LossScreen] Cannot hide loss screen - panel is null!");
            return;
        }

        // Hide the panel using CanvasGroup if available
        if (_group)
        {
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        // Deactivate the panel GameObject
        panel.SetActive(false);

        // Restore the UI elements we hid
        if (hideWhileShown != null)
        {
            foreach (var go in hideWhileShown)
            {
                if (go)
                {
                    go.SetActive(true);
                }
            }
        }

        // Unpause the game
        Time.timeScale = 1f;

        Debug.Log("[LossScreen] Loss screen is now hidden.");
    }
}