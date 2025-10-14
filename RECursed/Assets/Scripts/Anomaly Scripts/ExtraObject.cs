using UnityEngine;

public class ExtraObject : MonoBehaviour, IAnomaly
{
    [Header("Setup")]
    [SerializeField] private Room room = Room.Bedroom1;
    public GameObject extraObjectPrefab;
    public Vector3 spawnPosition;

    [Header("Timer Settings")]
    [SerializeField] private float reportWindow = 30f;
    [SerializeField] private SegmentBattery battery;

    [Header("Debug/State")]
    public bool hasExtraAnomaly = false;

    private GameObject spawnedInstance;
    private float deadlineTimer = 0f;
    private bool isTimerActive = false;

    public Room Room => room;
    public AnomalyType Type => AnomalyType.ExtraObject;
    public bool IsActive => hasExtraAnomaly;

    void Start()
    {
        if (!battery) battery = FindObjectOfType<SegmentBattery>();
    }

    void Update()
    {
        if (isTimerActive && hasExtraAnomaly)
        {
            deadlineTimer -= Time.deltaTime;
            
            if (deadlineTimer <= 0f)
            {
                OnDeadlineExpired();
            }
        }
    }

    public void TriggerExtraAnomaly()
    {
        if (hasExtraAnomaly || !extraObjectPrefab) return;

        Vector3 worldPos = transform.TransformPoint(spawnPosition);
        spawnedInstance = Instantiate(extraObjectPrefab, worldPos, transform.rotation);
        hasExtraAnomaly = true;
        
        isTimerActive = true;
        deadlineTimer = reportWindow;
        
        Debug.Log($"[ExtraObject] {room} anomaly triggered! Player has {reportWindow}s to report.");
    }

    public void Trigger() => TriggerExtraAnomaly();

    public void Revert()
    {
        isTimerActive = false;
        deadlineTimer = 0f;
        
        if (spawnedInstance)
            Destroy(spawnedInstance);

        spawnedInstance = null;
        hasExtraAnomaly = false;
        
        Debug.Log($"[ExtraObject] {room} anomaly reverted (reported successfully).");
    }

    private void OnDeadlineExpired()
    {
        Debug.LogWarning($"[ExtraObject] DEADLINE EXPIRED! {room} - Battery drained.");
        
        isTimerActive = false;
        deadlineTimer = 0f;
        
        if (battery) battery.Consume(1);
        
        if (spawnedInstance)
            Destroy(spawnedInstance);

        spawnedInstance = null;
        hasExtraAnomaly = false;
    }

    public float GetTimeRemaining() => isTimerActive ? Mathf.Max(0f, deadlineTimer) : 0f;
}