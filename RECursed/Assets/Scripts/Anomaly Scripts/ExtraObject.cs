using UnityEngine;

// Spawns an extra copy/prefab for a limited time window.
public class ExtraObject : MonoBehaviour, IAnomaly
{
    [Header("Setup")]
    [SerializeField] private Room room = Room.Bedroom1; // Room this belongs to.
    
    [Header("Extra Object Settings")]
    [Tooltip("The prefab that will be spawned as the extra object (auto-created).")]
    public GameObject extraObjectPrefab; // Prefab to instantiate.
    [Tooltip("Local position where the extra object will spawn.")]
    public Vector3 spawnPosition; // Local spawn position.
    [Tooltip("Local rotation (Euler angles) for the spawned object.")]
    public Vector3 spawnRotation; // Local spawn rotation.

    [Header("Timer Settings")]
    [SerializeField] private float reportWindow = 30f; // Seconds to report before penalty.
    [SerializeField] private SegmentBattery battery; // Battery to drain on miss.

    [Header("Debug/State")]
    public bool hasExtraAnomaly = false; // True while anomaly is active.

    private GameObject spawnedInstance; // Active spawned object.
    private float deadlineTimer = 0f; // Countdown timer.
    private bool isTimerActive = false; // Guards countdown.

    // Stored original transform for easy setup workflow
    private Vector3 savedOriginalPosition;
    private Vector3 savedOriginalRotation;
    private bool hasStoredOriginal = false;

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
            deadlineTimer -= Time.deltaTime; // Tick down.
            
            if (deadlineTimer <= 0f)
            {
                OnDeadlineExpired(); // Apply penalty and reset.
            }
        }
    }

    public void TriggerExtraAnomaly()
    {
        Debug.Log($"[ExtraObject] === TRIGGER CALLED ===");
        Debug.Log($"[ExtraObject] hasExtraAnomaly: {hasExtraAnomaly}");
        Debug.Log($"[ExtraObject] extraObjectPrefab: {(extraObjectPrefab ? extraObjectPrefab.name : "NULL")}");
        
        if (hasExtraAnomaly)
        {
            Debug.LogWarning("[ExtraObject] Already active! Aborting.");
            return;
        }
        
        if (!extraObjectPrefab)
        {
            Debug.LogError("[ExtraObject] No prefab assigned! Aborting.");
            return;
        }

        // Spawn at the exact position/rotation that was saved during STEP 2
        Vector3 worldPos;
        Quaternion worldRot;
        
        if (transform.parent)
        {
            // Convert local position relative to parent to world position
            worldPos = transform.parent.TransformPoint(spawnPosition);
            worldRot = transform.parent.rotation * Quaternion.Euler(spawnRotation);
        }
        else
        {
            // No parent, spawn position is already in world space
            worldPos = spawnPosition;
            worldRot = Quaternion.Euler(spawnRotation);
        }
        
        Debug.Log($"[ExtraObject] Spawning at WORLD position: {worldPos}");
        Debug.Log($"[ExtraObject] Prefab state: active={extraObjectPrefab.activeSelf}, scene={extraObjectPrefab.scene.name}");
        
        try
        {
            spawnedInstance = Instantiate(extraObjectPrefab, worldPos, worldRot);
            
            if (spawnedInstance)
            {
                spawnedInstance.SetActive(true); // Force active
                Debug.Log($"[ExtraObject] ✓ SUCCESS! Spawned: {spawnedInstance.name}, active={spawnedInstance.activeSelf}");
            }
            else
            {
                Debug.LogError("[ExtraObject] ✗ FAILED! Instantiate returned null!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ExtraObject] ✗ EXCEPTION during Instantiate: {e.Message}");
        }
        
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
        {
            Debug.Log($"[ExtraObject] Destroying spawned instance: {spawnedInstance.name}");
            Destroy(spawnedInstance);
        }

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

    // SUPER EASY SETUP WORKFLOW 

    [ContextMenu("STEP 1: Save Original Transform (Starting Position)")]
    private void SaveOriginalTransform()
    {
        savedOriginalPosition = transform.localPosition;
        savedOriginalRotation = transform.localEulerAngles;
        hasStoredOriginal = true;
        
        Debug.Log($"[ExtraObject] ✓ SAVED Original Transform! Position: {savedOriginalPosition}, Rotation: {savedOriginalRotation}");
        Debug.Log("[ExtraObject] Now move THIS object to where you want the EXTRA object to spawn, then use STEP 2.");
    }

    [ContextMenu("STEP 2: Copy Transform & Create Prefab")]
    private void CopyTransformAndCreatePrefab()
    {
        // Save the spawn position/rotation in LOCAL space relative to parent (or world if no parent)
        if (transform.parent)
        {
            spawnPosition = transform.localPosition;
            spawnRotation = transform.localEulerAngles;
        }
        else
        {
            spawnPosition = transform.position;
            spawnRotation = transform.eulerAngles;
        }
        
        Debug.Log($"[ExtraObject] ✓ COPIED Spawn Transform! Position: {spawnPosition}, Rotation: {spawnRotation}");
        
        // Create the prefab if it doesn't exist
        if (!extraObjectPrefab)
        {
            // Create a duplicate
            GameObject duplicate = Instantiate(gameObject, transform.position, transform.rotation);
            duplicate.name = gameObject.name + "_ExtraPrefab";
            
            // Set it as a child of this object's parent (or root if no parent)
            if (transform.parent)
                duplicate.transform.SetParent(transform.parent);
            
            // Remove the ExtraObject script from the duplicate (it's just a visual prop)
            ExtraObject duplicateScript = duplicate.GetComponent<ExtraObject>();
            if (duplicateScript)
            {
                #if UNITY_EDITOR
                DestroyImmediate(duplicateScript);
                #else
                Destroy(duplicateScript);
                #endif
            }
            
            // Hide it immediately
            duplicate.SetActive(false);
            
            // Assign as prefab
            extraObjectPrefab = duplicate;
            
            Debug.Log($"[ExtraObject] ✓ AUTO-CREATED Prefab! '{duplicate.name}' created and hidden.");
        }
        else
        {
            Debug.Log("[ExtraObject] Prefab already exists, just updated spawn position/rotation.");
        }
        
        Debug.Log("[ExtraObject] Now use STEP 3 to return to original position.");
    }

    [ContextMenu("STEP 3: Return to Original Transform")]
    private void ReturnToOriginalTransform()
    {
        if (!hasStoredOriginal)
        {
            Debug.LogWarning("[ExtraObject] ⚠ No original transform saved! Use STEP 1 first.");
            return;
        }

        transform.localPosition = savedOriginalPosition;
        transform.localRotation = Quaternion.Euler(savedOriginalRotation);
        
        Debug.Log($"[ExtraObject] ✓ RETURNED to Original! Position: {savedOriginalPosition}, Rotation: {savedOriginalRotation}");
        Debug.Log("[ExtraObject] Setup complete! Press Play and use 'Test Trigger' to test the extra object spawning.");
    }

    [ContextMenu("--- TESTING ---")]
    private void Separator1() { }

    [ContextMenu("Test Trigger (Spawn Extra Object)")]
    private void TestTrigger()
    {
        Trigger();
    }

    [ContextMenu("Test Revert (Remove Extra Object)")]
    private void TestRevert()
    {
        Revert();
    }

    [ContextMenu("--- ADVANCED ---")]
    private void Separator2() { }

    [ContextMenu("Preview Extra Object (Show Prefab)")]
    private void PreviewExtraObject()
    {
        if (!extraObjectPrefab)
        {
            Debug.LogError("[ExtraObject] ⚠ No prefab created yet! Use STEP 2 first.");
            return;
        }

        Vector3 worldPos;
        Quaternion worldRot;
        
        if (transform.parent)
        {
            // Convert local position relative to parent to world position
            worldPos = transform.parent.TransformPoint(spawnPosition);
            worldRot = transform.parent.rotation * Quaternion.Euler(spawnRotation);
        }
        else
        {
            // No parent, spawn position is already in world space
            worldPos = spawnPosition;
            worldRot = Quaternion.Euler(spawnRotation);
        }
        
        extraObjectPrefab.transform.position = worldPos;
        extraObjectPrefab.transform.rotation = worldRot;
        extraObjectPrefab.SetActive(true);
        
        Debug.Log($"[ExtraObject] Preview: Extra object visible at spawn position {worldPos}.");
    }

    [ContextMenu("Hide Extra Object (Hide Prefab)")]
    private void HideExtraObject()
    {
        if (!extraObjectPrefab)
        {
            Debug.LogError("[ExtraObject] ⚠ No prefab exists!");
            return;
        }

        extraObjectPrefab.SetActive(false);
        Debug.Log($"[ExtraObject] Extra object hidden.");
    }

    [ContextMenu("Delete Prefab (Clean Up)")]
    private void DeletePrefab()
    {
        if (!extraObjectPrefab)
        {
            Debug.LogError("[ExtraObject] ⚠ No prefab to delete!");
            return;
        }

        string prefabName = extraObjectPrefab.name;
        
        #if UNITY_EDITOR
        DestroyImmediate(extraObjectPrefab);
        #else
        Destroy(extraObjectPrefab);
        #endif
        
        extraObjectPrefab = null;
        
        Debug.LogWarning($"[ExtraObject] ⚠ Deleted prefab '{prefabName}'. Run STEP 1 again to create a new setup.");
    }

    [ContextMenu("Clear Stored Original")]
    private void ClearStoredOriginal()
    {
        hasStoredOriginal = false;
        savedOriginalPosition = Vector3.zero;
        savedOriginalRotation = Vector3.zero;
        Debug.Log("[ExtraObject] Cleared stored original transform.");
    }
}