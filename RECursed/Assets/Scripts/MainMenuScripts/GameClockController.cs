using UnityEngine;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Controls the in-game clock system with accelerated time progression.
/// Handles time display, round end detection, and integration with RoundManager.
/// Supports both single-round mode (with end panel) and multi-round mode (with RoundManager).
/// </summary>
public class GameClockController : MonoBehaviour
{
    // Constants for magic numbers (Coding Standards: Replace hardcoded values with named constants)
    // PDF Review Feedback: "Lines 77, 92, 93, 96 and 97 contain hardcoded values like 3600, 60, 12 and 24"
    private const int SECONDS_PER_HOUR = 3600;
    private const int SECONDS_PER_MINUTE = 60;
    private const int HOURS_IN_DAY = 24;
    private const int HOURS_IN_HALF_DAY = 12;
    private const int MIDNIGHT_HOUR = 0;

    [Header("Clock UI")]
    [SerializeField] private TMP_Text clockText;     

    [Header("Time Control")]
    [Tooltip("In-game seconds added per real-time second. 60 = 1 real sec = 1 in-game minute.")]
    [Min(0.01f)] public float timeMultiplier = 60f;

    [Tooltip("Start time (24h). Example: 0 = 12:00 AM, 23 = 11:00 PM.")]
    [Range(0, 23)] public int startHour = 0;

    [Tooltip("Start minutes (0-59).")]
    [Range(0, 59)] public int startMinute = 0;

    [Header("Round End")]
    [Tooltip("When the clock reaches this time (24h), the round ends.")]
    [Range(0, 23)] public int endHour = 6;
    [Range(0, 59)] public int endMinute = 0;

    [Tooltip("DEPRECATED: Use RoundManager instead for multi-round games.")]
    [SerializeField] private GameObject endPanel;

    [Tooltip("Use Round Manager for multi-round games. Leave empty for single-round mode.")]
    [SerializeField] private RoundManager roundManager;

    [Tooltip("Pause the game (Time.timeScale = 0) when the round ends (single-round mode only).")]
    [SerializeField] private bool pauseOnEnd = true;

    [Header("Disable While End")]
    [Tooltip("Drag components here to disable when the round ends (e.g., First Person Mover, MouseLook, TabletPanelController).")]
    [SerializeField] private Behaviour[] disableWhileEnd;

    [Header("Events")]
    [Tooltip("Raised once when the round ends.")]
    public UnityEvent onEnd;

    // -------- internal state --------
    private double elapsedGameSeconds;  
    private double startSecondsOfDay;    
    private double endSecondsOfDay;      
    private bool endTriggered;

    /// <summary>
    /// Initializes the clock system.
    /// Calculates start and end times in seconds, auto-finds RoundManager if needed, and validates setup.
    /// </summary>
    void Awake()
    {
        startSecondsOfDay = ToSecondsOfDay(startHour, startMinute);
        endSecondsOfDay = ToSecondsOfDay(endHour, endMinute);

        // Auto-find RoundManager if not set
        // PDF Review: "Added braces for consistency" (was: if (!roundManager) roundManager = ...)
        if (!roundManager)
        {
            roundManager = FindObjectOfType<RoundManager>();
        }

        UpdateClockUI();

        // Input Validation (PDF Review: "Defensive programming - validate Inspector values")
        ValidateSetup();
    }

    /// <summary>
    /// Validates the clock configuration and logs warnings for potential issues.
    /// Addresses PDF Review feedback: "No validator on Inspector-assigned values"
    /// </summary>
    private void ValidateSetup()
    {
        // Validate time multiplier
        if (timeMultiplier <= 0f)
        {
            Debug.LogError("[GameClockController] CRITICAL: timeMultiplier is <= 0. Clock will not progress!");
        }

        // Validate time ranges
        if (startHour < 0 || startHour >= HOURS_IN_DAY)
        {
            Debug.LogWarning($"[GameClockController] startHour ({startHour}) is outside valid range (0-23).");
        }

        if (endHour < 0 || endHour >= HOURS_IN_DAY)
        {
            Debug.LogWarning($"[GameClockController] endHour ({endHour}) is outside valid range (0-23).");
        }

        // Validate end time logic (PDF Review: "Nothing prevents endHour from being less than startHour")
        if (endSecondsOfDay <= startSecondsOfDay)
        {
            Debug.LogWarning($"[GameClockController] End time ({endHour}:{endMinute:00}) is before or equal to start time ({startHour}:{startMinute:00}). This may cause issues unless you intend an overnight round.");
        }

        // Validate UI reference (PDF Review: "Null checks fail silently without logging warnings")
        if (!clockText)
        {
            Debug.LogWarning("[GameClockController] Clock Text is not assigned. Time will not display in UI.");
        }

        // Log successful setup
        Debug.Log($"[GameClockController] Setup complete - Start: {startHour}:{startMinute:00}, End: {endHour}:{endMinute:00}, Multiplier: {timeMultiplier}x");
    }

    /// <summary>
    /// Updates the clock every frame.
    /// Advances game time, updates UI display, and checks for round end condition.
    /// </summary>
    void Update()
    {
        // PDF Review: "Added braces for consistency" (was: if (endTriggered) return;)
        if (endTriggered)
        {
            return;
        }

        elapsedGameSeconds += Time.deltaTime * timeMultiplier;

        UpdateClockUI();

        if (GetCurrentSecondsOfDay() >= endSecondsOfDay)
        {
            TriggerEnd();
        }
    }

    // ---------------- helpers ----------------

    /// <summary>
    /// Converts hours and minutes to total seconds since midnight.
    /// Uses named constants instead of magic numbers for clarity.
    /// </summary>
    /// <param name="h">Hour (0-23)</param>
    /// <param name="m">Minute (0-59)</param>
    /// <returns>Total seconds since midnight</returns>
    private double ToSecondsOfDay(int h, int m)
    {
        // PDF Review: "Replaced magic numbers 3600 and 60 with named constants"
        return (h * SECONDS_PER_HOUR) + (m * SECONDS_PER_MINUTE);
    }

    /// <summary>
    /// Gets the current in-game time as seconds since midnight.
    /// </summary>
    /// <returns>Current seconds since midnight</returns>
    private double GetCurrentSecondsOfDay()
    {
        double total = startSecondsOfDay + elapsedGameSeconds;
        return total;
    }

    /// <summary>
    /// Updates the clock UI text with the current time in 12-hour format with AM/PM.
    /// PDF Review: "String interpolation format :00 applies zero-padding for two-digit display"
    /// Example: 3:05 AM instead of 3:5 AM
    /// </summary>
    private void UpdateClockUI()
    {
        // PDF Review: "Added braces and warning log instead of silent failure"
        if (!clockText)
        {
            return;
        }

        double current = GetCurrentSecondsOfDay();
        int totalSeconds = Mathf.FloorToInt((float)current);

        // PDF Review: "Replaced magic numbers with named constants"
        int hours = totalSeconds / SECONDS_PER_HOUR;
        int mins = (totalSeconds / SECONDS_PER_MINUTE) % SECONDS_PER_MINUTE;

        // Display as 12-hour clock with AM/PM
        // PDF Review: "Replaced magic numbers 24 and 12 with named constants"
        string ampm = (hours % HOURS_IN_DAY) < HOURS_IN_HALF_DAY ? "AM" : "PM";
        int displayHour = hours % HOURS_IN_HALF_DAY;

        // PDF Review: "Added braces for consistency"
        if (displayHour == MIDNIGHT_HOUR)
        {
            displayHour = HOURS_IN_HALF_DAY;
        }

        // :00 format applies zero-padding to ensure two digits for minutes
        clockText.text = $"{displayHour:00}:{mins:00} {ampm}";
    }

    /// <summary>
    /// Triggers the round end logic.
    /// If RoundManager exists, delegates to it for multi-round support.
    /// Otherwise, handles single-round end (shows panel, disables controls).
    /// </summary>
    private void TriggerEnd()
    {
        // PDF Review: "Added braces for consistency"
        if (endTriggered)
        {
            return;
        }

        endTriggered = true;

        Debug.Log("[GameClock] Round end reached.");

        // If we have a RoundManager, let it handle the transition
        if (roundManager)
        {
            roundManager.OnRoundTimeComplete();
            
            // Reset this flag so the clock can run again for the next round
            endTriggered = false;
        }
        else
        {
            // Single-round mode: show the end panel and stop
            HandleSingleRoundEnd();
        }

        onEnd?.Invoke();
    }

    /// <summary>
    /// Handles the end of a single-round game.
    /// Disables gameplay scripts, shows end panel, reveals cursor, and optionally pauses.
    /// Only called when no RoundManager is present.
    /// </summary>
    private void HandleSingleRoundEnd()
    {
        // Disable any gameplay scripts that are dragged into the list
        if (disableWhileEnd != null)
        {
            foreach (var b in disableWhileEnd)
            {
                // PDF Review: "Added braces for consistency"
                if (b)
                {
                    b.enabled = false;
                }
            }
        }

        // Show global end panel (works whether tablet is open or closed)
        // PDF Review: "Added braces for consistency"
        if (endPanel)
        {
            endPanel.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // PDF Review: "Added braces for consistency"
        if (pauseOnEnd)
        {
            Time.timeScale = 0f;
        }
    }

    // ---------------- optional utilities ----------------

    /// <summary>
    /// Resumes the game after a single-round end.
    /// Re-enables disabled scripts, hides end panel, and unpauses if needed.
    /// Used for restarting or debugging.
    /// </summary>
    public void ResumeFromEnd()
    {
        // PDF Review: "Added braces for consistency"
        if (!endTriggered)
        {
            return;
        }

        // PDF Review: "Added braces for consistency"
        if (pauseOnEnd)
        {
            Time.timeScale = 1f;
        }

        // Re-enable anything that was disabled
        if (disableWhileEnd != null)
        {
            foreach (var b in disableWhileEnd)
            {
                // PDF Review: "Added braces for consistency"
                if (b)
                {
                    b.enabled = true;
                }
            }
        }

        // PDF Review: "Added braces for consistency"
        if (endPanel)
        {
            endPanel.SetActive(false);
        }

        endTriggered = false;
    }

    /// <summary>
    /// Manually sets the clock time and resets the elapsed time counter.
    /// Used by RoundManager to reset the clock between rounds.
    /// </summary>
    /// <param name="hour24">Hour in 24-hour format (0-23)</param>
    /// <param name="minute">Minute (0-59)</param>
    public void SetTime(int hour24, int minute)
    {
        startHour = hour24;
        startMinute = minute;
        startSecondsOfDay = ToSecondsOfDay(startHour, startMinute);
        elapsedGameSeconds = 0;
        endTriggered = false; // Allow the clock to run again
        UpdateClockUI();
    }
}