using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GameClockController : MonoBehaviour
{
    [Header("Clock UI")]
    [SerializeField] private TMP_Text clockText;     

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
    private double elapsedGameSeconds;  
    private double startSecondsOfDay;    
    private double endSecondsOfDay;      
    private bool endTriggered;

    void Awake()
    {
        startSecondsOfDay = ToSecondsOfDay(startHour, startMinute);
        endSecondsOfDay   = ToSecondsOfDay(endHour,  endMinute);

        
        UpdateClockUI();
    }

    void Update()
    {
        if (endTriggered) return;

       
        elapsedGameSeconds += Time.deltaTime * timeMultiplier;

        
        UpdateClockUI();

        
        if (GetCurrentSecondsOfDay() >= endSecondsOfDay)
        {
            TriggerEnd();
        }
    }

    // ---------------- helpers ----------------

    private double ToSecondsOfDay(int h, int m) => (h * 3600.0) + (m * 60.0);

    private double GetCurrentSecondsOfDay()
    {
        
        double total = startSecondsOfDay + elapsedGameSeconds;
        
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

        // Disable any gameplay scripts that is dragged into the list
        if (disableWhileEnd != null)
        {
            foreach (var b in disableWhileEnd)
            {
                if (b) b.enabled = false;
            }
        }

        // Show global end panel (works whether tablet is open or closed)
        if (endPanel) endPanel.SetActive(true);

        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseOnEnd) Time.timeScale = 0f;

        onEnd?.Invoke();
        Debug.Log("[GameClock] Round end reached — end panel shown.");
    }

    // ---------------- optional utilities ----------------

    
    public void ResumeFromEnd()
    {
        if (!endTriggered) return;

        if (pauseOnEnd) Time.timeScale = 1f;

        // Re-enable anything that is disabled
        if (disableWhileEnd != null)
        {
            foreach (var b in disableWhileEnd)
            {
                if (b) b.enabled = true;
            }
        }

        if (endPanel) endPanel.SetActive(false);

        
        endTriggered = false;

        // Relock cursor if your game needs it
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    
    public void SetTime(int hour24, int minute)
    {
        startHour = hour24;
        startMinute = minute;
        startSecondsOfDay = ToSecondsOfDay(startHour, startMinute);
        elapsedGameSeconds = 0;
        UpdateClockUI();
    }
}
