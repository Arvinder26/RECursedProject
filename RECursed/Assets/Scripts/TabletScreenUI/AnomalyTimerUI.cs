using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Displays all active anomalies with countdown timers.
/// DEBUG VERSION with extensive logging.
/// </summary>
public class AnomalyTimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Display Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;

    [Header("Battery Loss Notification")]
    [SerializeField] private string batteryLossMessage = "⚡ BATTERY LOST!";
    [SerializeField] private float notificationDuration = 2f;
    [SerializeField] private Color notificationColor = Color.red;

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

    void Awake()
    {
        Debug.Log("[AnomalyTimerUI] ===== AWAKE START =====");
        
        movedObjects.AddRange(FindObjectsOfType<MovedObject>(true));
        disappearedObjects.AddRange(FindObjectsOfType<DisappearedObject>(true));
        extraObjects.AddRange(FindObjectsOfType<ExtraObject>(true));
        lightFlickerObjects.AddRange(FindObjectsOfType<LightFlickerAnomaly>(true));

        int totalFound = movedObjects.Count + disappearedObjects.Count + extraObjects.Count + lightFlickerObjects.Count;
        Debug.Log($"[AnomalyTimerUI] Found {totalFound} anomalies ({movedObjects.Count} moved, {disappearedObjects.Count} disappeared, {extraObjects.Count} extra, {lightFlickerObjects.Count} light flicker)");

        if (!timerText)
            timerText = GetComponentInChildren<TMP_Text>();
        
        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        Debug.Log($"[AnomalyTimerUI] Setup - TimerText: {timerText != null}, CanvasGroup: {canvasGroup != null}");
        Debug.Log("[AnomalyTimerUI] ===== AWAKE END =====");
    }

    void Update()
    {
        updateCount++;
        
        // Only log every 60 frames to avoid spam
        if (verboseDebug && updateCount % 60 == 0)
        {
            Debug.Log($"[AnomalyTimerUI] Update() is running (frame {updateCount})");
        }
        
        // Update notification timer
        if (showingNotification)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f)
            {
                showingNotification = false;
            }
        }
        
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (!timerText || !canvasGroup)
        {
            if (verboseDebug && updateCount % 300 == 0)
            {
                Debug.LogWarning($"[AnomalyTimerUI] Missing references - TimerText: {timerText != null}, CanvasGroup: {canvasGroup != null}");
            }
            return;
        }

        var activeTimers = new List<AnomalyTimerInfo>();
        var currentActiveAnomalies = new HashSet<MonoBehaviour>();

        // Check MovedObjects
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

        // Check DisappearedObjects
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

        // Check ExtraObjects
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

        // Check LightFlickerAnomalies
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
                // An anomaly just expired! Show notification
                TriggerBatteryLossNotification();
                Debug.Log("[AnomalyTimerUI] Battery loss detected - showing notification!");
                break; // Only trigger once per frame
            }
        }

        // Update the previous active list
        previousActiveAnomalies = currentActiveAnomalies;

        // Log active timer count
        if (verboseDebug && (activeTimers.Count > 0 || updateCount % 300 == 0))
        {
            Debug.Log($"[AnomalyTimerUI] Active timers detected: {activeTimers.Count}");
        }

        // If no active timers, fade out
        if (activeTimers.Count == 0)
        {
            timerText.text = "";
            
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * 5f);
            if (canvasGroup.alpha < 0.01f)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            
            warnedAnomalies.Clear();
            criticalAnomalies.Clear();
            return;
        }

        // Show the panel by fading in
        Debug.Log($"[AnomalyTimerUI] === PANEL SHOULD BE VISIBLE! {activeTimers.Count} active timers ===");
        
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * 10f);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        Debug.Log($"[AnomalyTimerUI] CanvasGroup alpha: {canvasGroup.alpha}");

        activeTimers.Sort((a, b) => a.timeRemaining.CompareTo(b.timeRemaining));

        // Build display text with optional notification
        string notificationText = "";
        if (showingNotification)
        {
            string notifColorHex = ColorUtility.ToHtmlStringRGB(notificationColor);
            notificationText = $" <color=#{notifColorHex}>{batteryLossMessage}</color>";
        }

        string displayText = $"<b>ACTIVE ANOMALIES:</b>{notificationText}\n";

        foreach (var info in activeTimers)
        {
            string roomName = info.room.ToString();
            int seconds = Mathf.CeilToInt(info.timeRemaining);
            
            string colorHex;
            if (info.timeRemaining <= 5f)
                colorHex = ColorUtility.ToHtmlStringRGB(criticalColor);
            else if (info.timeRemaining <= 10f)
                colorHex = ColorUtility.ToHtmlStringRGB(warningColor);
            else
                colorHex = ColorUtility.ToHtmlStringRGB(normalColor);

            string warningIcon = info.timeRemaining <= 5f ? " [!]" : "";
            
            displayText += $"<color=#{colorHex}>• {roomName} - {seconds}s remaining{warningIcon}</color>\n";
        }

        timerText.text = displayText;
        Debug.Log($"[AnomalyTimerUI] Text set to: {displayText}");

        warnedAnomalies.RemoveWhere(a => a == null);
        criticalAnomalies.RemoveWhere(a => a == null);
    }

    private void TriggerBatteryLossNotification()
    {
        showingNotification = true;
        notificationTimer = notificationDuration;
    }

    private class AnomalyTimerInfo
    {
        public Room room;
        public float timeRemaining;
        public MonoBehaviour anomalyObject;
    }
}