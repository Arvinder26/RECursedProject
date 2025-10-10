using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GameClockController : MonoBehaviour
{
    [Header("Clock UI")]
    [SerializeField] private TMP_Text clockText;     // drag your TextMeshPro label (the white 12:xx AM text)

    [Header("Time Control")]
    [Tooltip("In-game seconds added per real-time second. 60 = 1 real sec = 1 in-game minute.")]
    [Min(0.01f)] public float timeMultiplier = 60f;

    [Tooltip("Start time (24h). Example: 0 = 12:00 AM, 23 = 11:00 PM.")]
    [Range(0, 23)] public int startHour = 0;

    [Tooltip("Start minutes (0–59).")]
    [Range(0, 59)] public int startMinute = 0;

    [Header("Round End")]
    [Tooltip("When the clock reaches this time (24h), the round ends.")]
    [Range(0, 23)] public int endHour = 6;
    [Range(0, 59)] public int endMinute = 0;

    [Tooltip("Panel that appears when time reaches End (place this on a global overlay canvas).")]
    [SerializeField] private GameObject endPanel;

    [Tooltip("Pause the game (Time.timeScale = 0) when the round ends.")]
    [SerializeField] private bool pauseOnEnd = true;

    [Header("Disable While End")]
    [Tooltip("Drag components here to disable when the round ends (e.g., First Person Mover, MouseLook, TabletPanelController).")]
    [SerializeField] private Behaviour[] disableWhileEnd;

    [Header("Events")]
    [Tooltip("Raised once when the round ends.")]
    public UnityEvent onEnd;

    // -------- internal state --------
    private double elapsedGameSeconds;   // accumulates continuously
    private double startSecondsOfDay;    // converted start time to seconds-of-day
    private double endSecondsOfDay;      // converted end time to seconds-of-day
    private bool endTriggered;

    void Awake()
    {
        startSecondsOfDay = ToSecondsOfDay(startHour, startMinute);
        endSecondsOfDay   = ToSecondsOfDay(endHour,  endMinute);

        // Show initial time immediately
        UpdateClockUI();
    }

    void Update()
    {
        if (endTriggered) return;

        // Advance game time continuously, independent of UI
        elapsedGameSeconds += Time.deltaTime * timeMultiplier;

        // Update any visible clock label (safe even if the label is hidden)
        UpdateClockUI();

        // Check end condition
        if (GetCurrentSecondsOfDay() >= endSecondsOfDay)
        {
            TriggerEnd();
        }
    }

    // ---------------- helpers ----------------

    private double ToSecondsOfDay(int h, int m) => (h * 3600.0) + (m * 60.0);

    private double GetCurrentSecondsOfDay()
    {
        // Total since 00:00 of “day 0”
        double total = startSecondsOfDay + elapsedGameSeconds;
        // If you ever wanted to allow passing midnight, you could mod by 86400 here.
        return total;
    }

    private void UpdateClockUI()
    {
        if (!clockText) return;

        double current = GetCurrentSecondsOfDay();
        int totalSeconds = Mathf.FloorToInt((float)current);

        int hours =  totalSeconds / 3600;
        int mins  = (totalSeconds / 60) % 60;

        // Display as 12-hour clock with AM/PM
        string ampm = (hours % 24) < 12 ? "AM" : "PM";
        int displayHour = hours % 12;
        if (displayHour == 0) displayHour = 12;

        clockText.text = $"{displayHour:00}:{mins:00} {ampm}";
    }

    private void TriggerEnd()
    {
        if (endTriggered) return;
        endTriggered = true;

        // Disable any gameplay scripts you dragged into the list
        if (disableWhileEnd != null)
        {
            foreach (var b in disableWhileEnd)
            {
                if (b) b.enabled = false;
            }
        }

        // Show global end panel (works whether tablet is open or closed)
        if (endPanel) endPanel.SetActive(true);

        // Nice UX: unlock cursor so player can interact with UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseOnEnd) Time.timeScale = 0f;

        onEnd?.Invoke();
        Debug.Log("[GameClock] Round end reached — end panel shown.");
    }

    // ---------------- optional utilities ----------------

    /// <summary>Call this from a Restart/Continue button to hide the end panel and resume.</summary>
    public void ResumeFromEnd()
    {
        if (!endTriggered) return;

        if (pauseOnEnd) Time.timeScale = 1f;

        // Re-enable anything we disabled
        if (disableWhileEnd != null)
        {
            foreach (var b in disableWhileEnd)
            {
                if (b) b.enabled = true;
            }
        }

        if (endPanel) endPanel.SetActive(false);

        // Keep time running or reset as you prefer; here we just keep running.
        endTriggered = false;

        // Relock cursor if your game needs it
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    /// <summary>Set the current in-game time from code (24h).</summary>
    public void SetTime(int hour24, int minute)
    {
        startHour = hour24;
        startMinute = minute;
        startSecondsOfDay = ToSecondsOfDay(startHour, startMinute);
        elapsedGameSeconds = 0;
        UpdateClockUI();
    }
}
