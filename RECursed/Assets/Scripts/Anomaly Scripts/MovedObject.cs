using UnityEngine;

// Moves this object to a new local transform for a limited time window.
public class MovedObject : MonoBehaviour, IAnomaly
{
    [Header("Setup")]
    [SerializeField] private Room room = Room.Bedroom1; // Room this belongs to.
    
    [Header("New Transform")]
    [Tooltip("Local position to move to when the anomaly triggers.")]
    public Vector3 newPosition; // Target local position.
    [Tooltip("Local rotation (Euler angles) when the anomaly triggers.")]
    public Vector3 newRotation; // Target local rotation.

    [Header("Timer Settings")]
    [Tooltip("How many seconds the player has to report this anomaly.")]
    [SerializeField] private float reportWindow = 30f; // Seconds to report.
    [SerializeField] private SegmentBattery battery; // Battery to drain on miss.

    [Header("Debug/State")]
    public bool hasMovedAnomaly = false; // True while anomaly is active.

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private float deadlineTimer = 0f; // Countdown timer.
    private bool isTimerActive = false; // Guards countdown.

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
            deadlineTimer -= Time.deltaTime; // Tick down.
            
            if (deadlineTimer <= 0f)
            {
                OnDeadlineExpired(); // Apply penalty and reset.
            }
        }
    }

    public void TriggerMovedAnomaly()
    {
        if (hasMovedAnomaly) return; // Ignore if already active.

        // Apply new position and rotation
        transform.localPosition = newPosition;
        transform.localRotation = Quaternion.Euler(newRotation);
        
        hasMovedAnomaly = true; // Mark active.
        isTimerActive = true; // Start timer.
        deadlineTimer = reportWindow; // Reset window.
        
        Debug.Log($"[MovedObject] {room} anomaly triggered! Player has {reportWindow}s to report. New position: {newPosition}, rotation: {newRotation}");
    }

    public void Trigger() => TriggerMovedAnomaly();

    public void Revert()
    {
        isTimerActive = false; // Stop timer.
        deadlineTimer = 0f; // Clear countdown.
        
        // Restore original position and rotation
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        
        hasMovedAnomaly = false;
        
        Debug.Log($"[MovedObject] {room} anomaly reverted (reported successfully).");
    }

    private void OnDeadlineExpired()
    {
        Debug.LogWarning($"[MovedObject] DEADLINE EXPIRED! {room} - Battery drained.");
        
        isTimerActive = false; // Stop timer.
        deadlineTimer = 0f; // Clear countdown.
        
        if (battery) battery.Consume(1); // Apply penalty.
        
        // Restore original position and rotation
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        
        hasMovedAnomaly = false;
    }

    public float GetTimeRemaining() => isTimerActive ? Mathf.Max(0f, deadlineTimer) : 0f;

    // EASY SETUP WORKFLOW

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
    private void Separator1() { } // Context menu spacer.

    [ContextMenu("Test Trigger (Move Object)")]
    private void TestTrigger()
    {
        Trigger(); // Manual trigger.
    } 

    [ContextMenu("Test Revert (Return to Original)")]
    private void TestRevert()
    {
        Revert(); // Manual revert.
    }

    [ContextMenu("--- ADVANCED ---")]
    private void Separator2() { } // Context menu spacer.

    [ContextMenu("Copy Current Position Only")]
    private void CopyCurrentPosition() 
    {
        newPosition = transform.localPosition; // Record only pos.
        Debug.Log($"[MovedObject] ✓ Copied position: {newPosition}");
    }

    [ContextMenu("Copy Current Rotation Only")]
    private void CopyCurrentRotation()
    {
        newRotation = transform.localEulerAngles; // Record only rot.
        Debug.Log($"[MovedObject] ✓ Copied rotation: {newRotation}");
    }

    [ContextMenu("Preview Anomaly Transform")]
    private void PreviewNewTransform()
    {
        transform.localPosition = newPosition; // Preview pos. 
        transform.localRotation = Quaternion.Euler(newRotation); // Preview rot.
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
        Vector3 tempPos = savedOriginalPosition; // Temp store pos.
        Vector3 tempRot = savedOriginalRotation; // Temp store rot.
        
        savedOriginalPosition = newPosition; // Swap pos.
        savedOriginalRotation = newRotation; // Swap rot.
        
        newPosition = tempPos; // Complete swap.
        newRotation = tempRot; // Complete swap.
        
        Debug.Log($"[MovedObject] ✓ Swapped positions! Original and Moved positions have been exchanged.");
    }

    [ContextMenu("Clear Stored Original")]
    private void ClearStoredOriginal()
    {
        hasStoredOriginal = false; // Forget snapshot.
        savedOriginalPosition = Vector3.zero; // Reset pos.
        savedOriginalRotation = Vector3.zero; // Reset rot.
        Debug.Log("[MovedObject] Cleared stored original transform.");
    }
}