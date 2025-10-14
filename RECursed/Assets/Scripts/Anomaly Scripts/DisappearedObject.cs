using UnityEngine;

public class DisappearedObject : MonoBehaviour, IAnomaly
{
    [Header("Setup")]
    [SerializeField] private Room room = Room.Bedroom1;
    [SerializeField] private Renderer[] renderersToToggle = new Renderer[0];
    [SerializeField] private Collider[] collidersToToggle = new Collider[0];

    [Header("Timer Settings")]
    [SerializeField] private float reportWindow = 30f;
    [SerializeField] private SegmentBattery battery;

    [Header("Debug/State")]
    public bool hasDisappearedAnomaly = false;

    private float deadlineTimer = 0f;
    private bool isTimerActive = false;

    public Room Room => room;
    public AnomalyType Type => AnomalyType.ObjectDisappeared;
    public bool IsActive => hasDisappearedAnomaly;

    void Awake()
    {
        if (renderersToToggle == null || renderersToToggle.Length == 0)
            renderersToToggle = GetComponentsInChildren<Renderer>(true);

        if (collidersToToggle == null || collidersToToggle.Length == 0)
            collidersToToggle = GetComponentsInChildren<Collider>(true);
        
        if (!battery) battery = FindObjectOfType<SegmentBattery>();
    }

    void Update()
    {
        if (isTimerActive && hasDisappearedAnomaly)
        {
            deadlineTimer -= Time.deltaTime;
            
            if (deadlineTimer <= 0f)
            {
                OnDeadlineExpired();
            }
        }
    }

    public void TriggerDisappearedAnomaly()
    {
        if (hasDisappearedAnomaly) return;

        SetVisible(false);
        hasDisappearedAnomaly = true;
        
        isTimerActive = true;
        deadlineTimer = reportWindow;
        
        Debug.Log($"[DisappearedObject] {room} anomaly triggered! Player has {reportWindow}s to report.");
    }

    public void Trigger() => TriggerDisappearedAnomaly();

    public void Revert()
    {
        isTimerActive = false;
        deadlineTimer = 0f;
        
        SetVisible(true);
        hasDisappearedAnomaly = false;
        
        Debug.Log($"[DisappearedObject] {room} anomaly reverted (reported successfully).");
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderersToToggle)
            if (r) r.enabled = visible;

        foreach (var c in collidersToToggle)
            if (c) c.enabled = visible;
    }

    private void OnDeadlineExpired()
    {
        Debug.LogWarning($"[DisappearedObject] DEADLINE EXPIRED! {room} - Battery drained.");
        
        isTimerActive = false;
        deadlineTimer = 0f;
        
        if (battery) battery.Consume(1);
        
        SetVisible(true);
        hasDisappearedAnomaly = false;
    }

    public float GetTimeRemaining() => isTimerActive ? Mathf.Max(0f, deadlineTimer) : 0f;
}