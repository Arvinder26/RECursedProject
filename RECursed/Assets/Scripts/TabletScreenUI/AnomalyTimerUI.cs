using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Displays all active anomalies with countdown timers.
/// DEBUG VERSION with extensive logging.
/// </summary>
public class AnomalyTimerUI : MonoBehaviour
{
    // Constants for magic numbers (Coding Standards: Replace hardcoded values with named constants)
    private const int FRAMES_PER_LOG = 60;
    private const int LONG_INTERVAL_FRAMES = 300;
    private const float CRITICAL_TIME_THRESHOLD = 5f;
    private const float WARNING_TIME_THRESHOLD = 10f;
    private const float FADE_IN_SPEED = 5f;
    private const float FAST_FADE_IN_SPEED = 10f;

    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private SummaryReportManager summaryManager;


    [Header("Display Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;

    [Header("Battery Loss Notification")]
    [SerializeField] private string batteryLossMessage = "⚡ BATTERY LOST!";
    [SerializeField] private float notificationDuration = 2f;
    [SerializeField] private Color notificationColor = Color.red;

    [Header("Battery Loss Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip batteryLossSound;
    [Range(0f, 1f)]
    [SerializeField] private float audioVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool verboseDebug = true;

    private HashSet<MonoBehaviour> warnedAnomalies = new HashSet<MonoBehaviour>();
    private HashSet<MonoBehaviour> criticalAnomalies = new HashSet<MonoBehaviour>();
    private HashSet<MonoBehaviour> previousActiveAnomalies = new HashSet<MonoBehaviour>();

    private List<MovedObject> movedObjects = new List<MovedObject>();
    private List<DisappearedObject> disappearedObjects = new List<DisappearedObject>();
    private List<ExtraObject> extraObjects = new List<ExtraObject>();
    private List<LightFlickerAnomaly> lightFlickerObjects = new List<LightFlickerAnomaly>();
    
    private int updateCount = 0;
    
    // Battery loss notification state
    private float notificationTimer = 0f;
    private bool showingNotification = false;

    /// <summary>
    /// Initializes the anomaly timer UI system.
    /// Finds all anomaly objects in the scene and sets up UI references.
    /// Auto-finds missing components and validates setup.
    /// </summary>
    void Awake()
    {
        Debug.Log("[AnomalyTimerUI] ===== AWAKE START =====");
        
        // Find all anomaly objects in the scene (including inactive ones)
        movedObjects.AddRange(FindObjectsOfType<MovedObject>(true));
        disappearedObjects.AddRange(FindObjectsOfType<DisappearedObject>(true));
        extraObjects.AddRange(FindObjectsOfType<ExtraObject>(true));
        lightFlickerObjects.AddRange(FindObjectsOfType<LightFlickerAnomaly>(true));

        int totalFound = movedObjects.Count + disappearedObjects.Count + extraObjects.Count + lightFlickerObjects.Count;
        Debug.Log($"[AnomalyTimerUI] Found {totalFound} anomalies ({movedObjects.Count} moved, {disappearedObjects.Count} disappeared, {extraObjects.Count} extra, {lightFlickerObjects.Count} light flicker)");

        // Auto-find TMP_Text if not assigned in Inspector
        if (!timerText)
        {
            timerText = GetComponentInChildren<TMP_Text>();
        }
        
        // Auto-find CanvasGroup if not assigned in Inspector
        if (!canvasGroup)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        // Configure canvas group for proper UI display
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // Set initial text state
        if (timerText)
        {
            timerText.text = "Currently no anomalies...";
        }

        // Auto-find AudioSource if not assigned
        if (!audioSource)
        {
            audioSource = GetComponent<AudioSource>();
            
            // If still not found, create one dynamically
            if (!audioSource)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D sound
            }
        }

        // Input Validation (Coding Standards: Validate Inspector-assigned values)
        ValidateSetup();
        
        Debug.Log($"[AnomalyTimerUI] Setup - TimerText: {timerText != null}, CanvasGroup: {canvasGroup != null}, AudioSource: {audioSource != null}");
        Debug.Log("[AnomalyTimerUI] ===== AWAKE END =====");
    }

    /// <summary>
    /// Validates the component setup and logs warnings for missing references.
    /// Ensures the UI can function properly before gameplay begins.
    /// </summary>
    private void ValidateSetup()
    {
        // Validate critical UI components
        if (!timerText)
        {
            Debug.LogError("[AnomalyTimerUI] VALIDATION FAILED: TimerText is missing! UI will not display properly.");
        }

        if (!canvasGroup)
        {
            Debug.LogWarning("[AnomalyTimerUI] VALIDATION WARNING: CanvasGroup is missing. UI fading will not work.");
        }

        if (!audioSource)
        {
            Debug.LogWarning("[AnomalyTimerUI] VALIDATION WARNING: AudioSource is missing. Battery loss sound will not play.");
        }

        if (!batteryLossSound)
        {
            Debug.LogWarning("[AnomalyTimerUI] VALIDATION WARNING: Battery loss sound clip is not assigned.");
        }

        // Validate notification settings
        if (notificationDuration <= 0f)
        {
            Debug.LogWarning($"[AnomalyTimerUI] VALIDATION WARNING: Notification duration is {notificationDuration}. Should be > 0. Using default 2s.");
            notificationDuration = 2f;
        }

        if (audioVolume < 0f || audioVolume > 1f)
        {
            Debug.LogWarning($"[AnomalyTimerUI] VALIDATION WARNING: Audio volume is {audioVolume}. Clamping to 0-1 range.");
            audioVolume = Mathf.Clamp01(audioVolume);
        }
    }

    /// <summary>
    /// Updates the timer display every frame.
    /// Handles notification timing and triggers the main display update logic.
    /// </summary>
    void Update()
    {
        updateCount++;
        
        // Only log every FRAMES_PER_LOG frames to avoid console spam
        if (verboseDebug && updateCount % FRAMES_PER_LOG == 0)
        {
            Debug.Log($"[AnomalyTimerUI] Update() is running (frame {updateCount})");
        }
        
        // Update notification timer countdown
        if (showingNotification)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f)
            {
                showingNotification = false;
            }
        }
        
        // Update the main timer display
        UpdateTimerDisplay();
    }

    /// <summary>
    /// Updates the timer display with all active anomaly countdowns.
    /// Collects active anomalies, detects expirations, and formats the UI text.
    /// Handles color coding based on time remaining and shows battery loss notifications.
    /// </summary>
    private void UpdateTimerDisplay()
    {
        // Coding Standards: Always use braces for if statements
        if (!timerText || !canvasGroup)
        {
            if (verboseDebug && updateCount % LONG_INTERVAL_FRAMES == 0)
            {
                Debug.LogWarning($"[AnomalyTimerUI] Missing references - TimerText: {timerText != null}, CanvasGroup: {canvasGroup != null}");
            }
            return;
        }

        var activeTimers = new List<AnomalyTimerInfo>();
        var currentActiveAnomalies = new HashSet<MonoBehaviour>();

        // Check MovedObjects for active anomalies
        foreach (var obj in movedObjects)
        {
            if (obj)
            {
                bool isActive = obj.IsActive;
                float timeRemaining = obj.GetTimeRemaining();
                
                if (verboseDebug && isActive)
                {
                    Debug.Log($"[AnomalyTimerUI] MovedObject {obj.Room} - IsActive: {isActive}, Time: {timeRemaining}s");
                }
                
                if (isActive && timeRemaining > 0)
                {
                    activeTimers.Add(new AnomalyTimerInfo
                    {
                        room = obj.Room,
                        timeRemaining = timeRemaining,
                        anomalyObject = obj
                    });
                    currentActiveAnomalies.Add(obj);
                }
            }
        }

        // Check DisappearedObjects for active anomalies
        foreach (var obj in disappearedObjects)
        {
            if (obj)
            {
                bool isActive = obj.IsActive;
                float timeRemaining = obj.GetTimeRemaining();
                
                if (verboseDebug && isActive)
                {
                    Debug.Log($"[AnomalyTimerUI] DisappearedObject {obj.Room} - IsActive: {isActive}, Time: {timeRemaining}s");
                }
                
                if (isActive && timeRemaining > 0)
                {
                    activeTimers.Add(new AnomalyTimerInfo
                    {
                        room = obj.Room,
                        timeRemaining = timeRemaining,
                        anomalyObject = obj
                    });
                    currentActiveAnomalies.Add(obj);
                }
            }
        }

        // Check ExtraObjects for active anomalies
        foreach (var obj in extraObjects)
        {
            if (obj)
            {
                bool isActive = obj.IsActive;
                float timeRemaining = obj.GetTimeRemaining();
                
                if (verboseDebug && isActive)
                {
                    Debug.Log($"[AnomalyTimerUI] ExtraObject {obj.Room} - IsActive: {isActive}, Time: {timeRemaining}s");
                }
                
                if (isActive && timeRemaining > 0)
                {
                    activeTimers.Add(new AnomalyTimerInfo
                    {
                        room = obj.Room,
                        timeRemaining = timeRemaining,
                        anomalyObject = obj
                    });
                    currentActiveAnomalies.Add(obj);
                }
            }
        }

        // Check LightFlickerAnomalies for active anomalies
        foreach (var obj in lightFlickerObjects)
        {
            if (obj)
            {
                bool isActive = obj.IsActive;
                float timeRemaining = obj.GetTimeRemaining();
                
                if (verboseDebug && isActive)
                {
                    Debug.Log($"[AnomalyTimerUI] LightFlicker {obj.Room} - IsActive: {isActive}, Time: {timeRemaining}s");
                }
                
                if (isActive && timeRemaining > 0)
                {
                    activeTimers.Add(new AnomalyTimerInfo
                    {
                        room = obj.Room,
                        timeRemaining = timeRemaining,
                        anomalyObject = obj
                    });
                    currentActiveAnomalies.Add(obj);
                }
            }
        }

        // Detect if an anomaly expired (was active before, now not active)
        foreach (var prevAnomaly in previousActiveAnomalies)
        {
            if (prevAnomaly != null && !currentActiveAnomalies.Contains(prevAnomaly))
            {
                // An anomaly just expired! Show notification and play sound
                TriggerBatteryLossNotification();
                Debug.Log("[AnomalyTimerUI] Battery loss detected - showing notification and playing sound!");
                break; // Only trigger once per frame
            }
        }

        // Update the previous active list for next frame comparison
        previousActiveAnomalies = currentActiveAnomalies;

        // Log active timer count periodically
        if (verboseDebug && (activeTimers.Count > 0 || updateCount % LONG_INTERVAL_FRAMES == 0))
        {
            Debug.Log($"[AnomalyTimerUI] Active timers detected: {activeTimers.Count}");
        }

        // Handle case when no anomalies are active
        if (activeTimers.Count == 0)
        {
            // Show "no anomalies" message
            timerText.text = "<b>Currently no anomalies...</b>";

            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * FADE_IN_SPEED);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            warnedAnomalies.Clear();
            criticalAnomalies.Clear();
            return;
        }


        // Show the panel by fading in
        Debug.Log($"[AnomalyTimerUI] === PANEL SHOULD BE VISIBLE! {activeTimers.Count} active timers ===");
        
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * FAST_FADE_IN_SPEED);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        Debug.Log($"[AnomalyTimerUI] CanvasGroup alpha: {canvasGroup.alpha}");

        // Sort timers by time remaining (most urgent first)
        activeTimers.Sort((a, b) => a.timeRemaining.CompareTo(b.timeRemaining));

        // Build display text with optional notification
        string notificationText = "";
        if (showingNotification)
        {
            string notifColorHex = ColorUtility.ToHtmlStringRGB(notificationColor);
            notificationText = $" <color=#{notifColorHex}>{batteryLossMessage}</color>";
        }

        string displayText = $"<b>ACTIVE ANOMALIES:</b>{notificationText}\n";

        // Build the timer list with color coding based on urgency
        foreach (var info in activeTimers)
        {
            string roomName = info.room.ToString();
            int seconds = Mathf.CeilToInt(info.timeRemaining);
            
            // Determine color based on time remaining (Coding Standards: Named constants for thresholds)
            string colorHex;
            if (info.timeRemaining <= CRITICAL_TIME_THRESHOLD)
            {
                colorHex = ColorUtility.ToHtmlStringRGB(criticalColor);
            }
            else if (info.timeRemaining <= WARNING_TIME_THRESHOLD)
            {
                colorHex = ColorUtility.ToHtmlStringRGB(warningColor);
            }
            else
            {
                colorHex = ColorUtility.ToHtmlStringRGB(normalColor);
            }

            string warningIcon = info.timeRemaining <= CRITICAL_TIME_THRESHOLD ? " [!]" : "";
            
            displayText += $"<color=#{colorHex}>• {roomName} - {seconds}s remaining{warningIcon}</color>\n";
        }

        timerText.text = displayText;
        Debug.Log($"[AnomalyTimerUI] Text set to: {displayText}");

        // Clean up null references from tracking sets
        warnedAnomalies.RemoveWhere(a => a == null);
        criticalAnomalies.RemoveWhere(a => a == null);
    }

    /// <summary>
    /// Triggers the battery loss notification and plays the associated sound effect.
    /// Shows a visual notification for the configured duration and updates the summary manager.
    /// </summary>
    private void TriggerBatteryLossNotification()
    {
        showingNotification = true;
        notificationTimer = notificationDuration;

        // Play the battery loss sound effect
        if (audioSource && batteryLossSound)
        {
            audioSource.PlayOneShot(batteryLossSound, audioVolume);
            Debug.Log("[AnomalyTimerUI] Playing battery loss sound!");
        }
        else if (!batteryLossSound)
        {
            Debug.LogWarning("[AnomalyTimerUI] Battery loss sound not assigned!");
        }

        // Update summary manager with missed anomaly
        if (summaryManager)
        {
            summaryManager.ShowMissed();
        }
    }

    /// <summary>
    /// Data structure to hold information about an active anomaly timer.
    /// Used internally for sorting and displaying anomaly countdowns.
    /// </summary>
    private class AnomalyTimerInfo
    {
        public Room room;
        public float timeRemaining;
        public MonoBehaviour anomalyObject;
    }
}