using UnityEngine;

public class MovedObject : MonoBehaviour, IAnomaly
{
    [Header("Setup")]
    [SerializeField] private Room room = Room.Bedroom1;
    
    [Header("New Transform")]
    [Tooltip("Local position to move to when the anomaly triggers.")]
    public Vector3 newPosition;
    [Tooltip("Local rotation (Euler angles) when the anomaly triggers.")]
    public Vector3 newRotation;

    [Header("Timer Settings")]
    [Tooltip("How many seconds the player has to report this anomaly.")]
    [SerializeField] private float reportWindow = 30f;
    [SerializeField] private SegmentBattery battery;

    [Header("Debug/State")]
    public bool hasMovedAnomaly = false;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private float deadlineTimer = 0f;
    private bool isTimerActive = false;

    // Stored original transform for easy setup workflow
    private Vector3 savedOriginalPosition;
    private Vector3 savedOriginalRotation;
    private bool hasStoredOriginal = false;

    public Room Room => room;
    public AnomalyType Type => AnomalyType.MovedObject;
    public bool IsActive => hasMovedAnomaly;

    void Start()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        
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

        // Apply new position and rotation
        transform.localPosition = newPosition;
        transform.localRotation = Quaternion.Euler(newRotation);
        
        hasMovedAnomaly = true;
        isTimerActive = true;
        deadlineTimer = reportWindow;
        
        Debug.Log($"[MovedObject] {room} anomaly triggered! Player has {reportWindow}s to report. New position: {newPosition}, rotation: {newRotation}");
    }

    public void Trigger() => TriggerMovedAnomaly();

    public void Revert()
    {
        isTimerActive = false;
        deadlineTimer = 0f;
        
        // Restore original position and rotation
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        
        hasMovedAnomaly = false;
        
        Debug.Log($"[MovedObject] {room} anomaly reverted (reported successfully).");
    }

    private void OnDeadlineExpired()
    {
        Debug.LogWarning($"[MovedObject] DEADLINE EXPIRED! {room} - Battery drained.");
        
        isTimerActive = false;
        deadlineTimer = 0f;
        
        if (battery) battery.Consume(1);
        
        // Restore original position and rotation
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        
        hasMovedAnomaly = false;
    }

    public float GetTimeRemaining() => isTimerActive ? Mathf.Max(0f, deadlineTimer) : 0f;

    // ========== EASY SETUP WORKFLOW ==========

    [ContextMenu("STEP 1: Save Original Transform (Starting Position)")]
    private void SaveOriginalTransform()
    {
        savedOriginalPosition = transform.localPosition;
        savedOriginalRotation = transform.localEulerAngles;
        hasStoredOriginal = true;
        
        Debug.Log($"[MovedObject] ✓ SAVED Original Transform! Position: {savedOriginalPosition}, Rotation: {savedOriginalRotation}");
        Debug.Log("[MovedObject] Now move the object to its anomaly position, then use STEP 2.");
    }

    [ContextMenu("STEP 2: Copy Current to New Position & Rotation")]
    private void CopyCurrentToNew()
    {
        newPosition = transform.localPosition;
        newRotation = transform.localEulerAngles;
        
        Debug.Log($"[MovedObject] ✓ COPIED Anomaly Transform! Position: {newPosition}, Rotation: {newRotation}");
        Debug.Log("[MovedObject] Now use STEP 3 to return to original position.");
    }

    [ContextMenu("STEP 3: Return to Original Transform")]
    private void ReturnToOriginal()
    {
        if (!hasStoredOriginal)
        {
            Debug.LogWarning("[MovedObject] ⚠ No original transform saved! Use STEP 1 first.");
            return;
        }

        transform.localPosition = savedOriginalPosition;
        transform.localRotation = Quaternion.Euler(savedOriginalRotation);
        
        Debug.Log($"[MovedObject] ✓ RETURNED to Original! Position: {savedOriginalPosition}, Rotation: {savedOriginalRotation}");
        Debug.Log("[MovedObject] Setup complete! Press Play and use 'Test Trigger' to test.");
    }

    [ContextMenu("--- TESTING ---")]
    private void Separator1() { }

    [ContextMenu("Test Trigger (Move Object)")]
    private void TestTrigger()
    {
        Trigger();
    }

    [ContextMenu("Test Revert (Return to Original)")]
    private void TestRevert()
    {
        Revert();
    }

    [ContextMenu("--- ADVANCED ---")]
    private void Separator2() { }

    [ContextMenu("Copy Current Position Only")]
    private void CopyCurrentPosition()
    {
        newPosition = transform.localPosition;
        Debug.Log($"[MovedObject] ✓ Copied position: {newPosition}");
    }

    [ContextMenu("Copy Current Rotation Only")]
    private void CopyCurrentRotation()
    {
        newRotation = transform.localEulerAngles;
        Debug.Log($"[MovedObject] ✓ Copied rotation: {newRotation}");
    }

    [ContextMenu("Preview Anomaly Transform")]
    private void PreviewNewTransform()
    {
        transform.localPosition = newPosition;
        transform.localRotation = Quaternion.Euler(newRotation);
        Debug.Log($"[MovedObject] Preview: Position {newPosition}, Rotation {newRotation}");
    }

    [ContextMenu("Preview Original Transform")]
    private void PreviewOriginalTransform()
    {
        if (!hasStoredOriginal)
        {
            Debug.LogWarning("[MovedObject] ⚠ No original transform saved! Use STEP 1 first.");
            return;
        }

        transform.localPosition = savedOriginalPosition;
        transform.localRotation = Quaternion.Euler(savedOriginalRotation);
        Debug.Log($"[MovedObject] Preview: Original Position {savedOriginalPosition}, Rotation {savedOriginalRotation}");
    }

    [ContextMenu("Swap Original and Moved Positions")]
    private void SwapPositions()
    {
        Vector3 tempPos = savedOriginalPosition;
        Vector3 tempRot = savedOriginalRotation;
        
        savedOriginalPosition = newPosition;
        savedOriginalRotation = newRotation;
        
        newPosition = tempPos;
        newRotation = tempRot;
        
        Debug.Log($"[MovedObject] ✓ Swapped positions! Original and Moved positions have been exchanged.");
    }

    [ContextMenu("Clear Stored Original")]
    private void ClearStoredOriginal()
    {
        hasStoredOriginal = false;
        savedOriginalPosition = Vector3.zero;
        savedOriginalRotation = Vector3.zero;
        Debug.Log("[MovedObject] Cleared stored original transform.");
    }
}