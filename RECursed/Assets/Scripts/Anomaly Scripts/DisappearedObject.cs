using UnityEngine;

// Makes this object vanish for a limited report window.
public class DisappearedObject : MonoBehaviour, IAnomaly
{
    [Header("Setup")]
    [SerializeField] private Room room = Room.Bedroom1; // Room this belongs to.
    [SerializeField] private Renderer[] renderersToToggle = new Renderer[0]; // Renderers to hide/show.
    [SerializeField] private Collider[] collidersToToggle = new Collider[0]; // Colliders to disable/enable.

    [Header("Timer Settings")]
    [SerializeField] private float reportWindow = 30f; // Seconds to report before penalty.
    [SerializeField] private SegmentBattery battery;   // Battery to drain on miss.

    [Header("Debug/State")]
    public bool hasDisappearedAnomaly = false; // True while anomaly is active.

    private float deadlineTimer = 0f; // Countdown timer.
    private bool isTimerActive = false; // Guards countdown updates.

    public Room Room => room;
    public AnomalyType Type => AnomalyType.ObjectDisappeared;
    public bool IsActive => hasDisappearedAnomaly;

    void Awake()
    {
        if (renderersToToggle == null || renderersToToggle.Length == 0)
            renderersToToggle = GetComponentsInChildren<Renderer>(true); // Auto-fill renderers

        if (collidersToToggle == null || collidersToToggle.Length == 0)
            collidersToToggle = GetComponentsInChildren<Collider>(true); // Auto-fill colliders.
        
        if (!battery) battery = FindObjectOfType<SegmentBattery>(); // Fallback battery.
    }

    void Update()
    {
        if (isTimerActive && hasDisappearedAnomaly)
        {
            deadlineTimer -= Time.deltaTime; // Tick down.
            
            if (deadlineTimer <= 0f)
            {
                OnDeadlineExpired(); // Apply penalty and reset.
            }
        }
    }

    public void TriggerDisappearedAnomaly()
    {
        if (hasDisappearedAnomaly) return; // Ignore if already active.

        SetVisible(false); // Hide object.
        hasDisappearedAnomaly = true; // Mark active.
        
        isTimerActive = true; // Start timer.
        deadlineTimer = reportWindow; // Reset window.
        
        Debug.Log($"[DisappearedObject] {room} anomaly triggered! Player has {reportWindow}s to report.");
    }

    public void Trigger() => TriggerDisappearedAnomaly(); // IAnomaly trigger

    public void Revert()
    {
        isTimerActive = false; // Stop timer.
        deadlineTimer = 0f;    // Clear countdown.
        
        SetVisible(true);  // Show object again.
        hasDisappearedAnomaly = false; // Clear active flag. 
        
        Debug.Log($"[DisappearedObject] {room} anomaly reverted (reported successfully).");
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderersToToggle)
            if (r) r.enabled = visible; // Toggle visuals.

        foreach (var c in collidersToToggle)
            if (c) c.enabled = visible; // Toggle collisions.
    }

    private void OnDeadlineExpired()
    {
        Debug.LogWarning($"[DisappearedObject] DEADLINE EXPIRED! {room} - Battery drained.");
        
        isTimerActive = false; // Stop timer.
        deadlineTimer = 0f; // Clear countdown.
        
        if (battery) battery.Consume(1); // Apply penalty.
        
        SetVisible(true); // Restore object.
        hasDisappearedAnomaly = false; // Reset state.
    }

    public float GetTimeRemaining() => isTimerActive ? Mathf.Max(0f, deadlineTimer) : 0f;
}