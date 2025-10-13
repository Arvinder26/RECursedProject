using UnityEngine;

public class MovedObject : MonoBehaviour, IAnomaly
{
    [Header("Setup")]
    [SerializeField] private Room room = Room.Bedroom1;
    [Tooltip("Local position to move to when the anomaly triggers.")]
    public Vector3 newPosition;

    [Header("Timer Settings")]
    [Tooltip("How many seconds the player has to report this anomaly.")]
    [SerializeField] private float reportWindow = 30f;
    [SerializeField] private SegmentBattery battery;

    [Header("Debug/State")]
    public bool hasMovedAnomaly = false;

    private Vector3 originalLocalPosition;
    private float deadlineTimer = 0f;
    private bool isTimerActive = false;

    public Room Room => room;
    public AnomalyType Type => AnomalyType.MovedObject;
    public bool IsActive => hasMovedAnomaly;

    void Start()
    {
        originalLocalPosition = transform.localPosition;
        if (!battery) battery = FindObjectOfType<SegmentBattery>();
    }

    void Update()
    {
        if (isTimerActive && hasMovedAnomaly)
        {
            deadlineTimer -= Time.deltaTime;
            
            if (deadlineTimer <= 0f)
            {
                OnDeadlineExpired();
            }
        }
    }

    public void TriggerMovedAnomaly()
    {
        if (hasMovedAnomaly) return;

        transform.localPosition = newPosition;
        hasMovedAnomaly = true;
        
        isTimerActive = true;
        deadlineTimer = reportWindow;
        
        Debug.Log($"[MovedObject] {room} anomaly triggered! Player has {reportWindow}s to report.");
    }

    public void Trigger() => TriggerMovedAnomaly();

    public void Revert()
    {
        isTimerActive = false;
        deadlineTimer = 0f;
        
        transform.localPosition = originalLocalPosition;
        hasMovedAnomaly = false;
        
        Debug.Log($"[MovedObject] {room} anomaly reverted (reported successfully).");
    }

    private void OnDeadlineExpired()
    {
        Debug.LogWarning($"[MovedObject] DEADLINE EXPIRED! {room} - Battery drained.");
        
        isTimerActive = false;
        deadlineTimer = 0f;
        
        if (battery) battery.Consume(1);
        
        transform.localPosition = originalLocalPosition;
        hasMovedAnomaly = false;
    }

    public float GetTimeRemaining() => isTimerActive ? Mathf.Max(0f, deadlineTimer) : 0f;
}