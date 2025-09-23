using UnityEngine;

public class ExtraObject : MonoBehaviour
{
    // Drag the prefab you want to spawn here in the inspector
    public GameObject extraObjectPrefab;

    // Where relative to this object should the extra one appear (local coordinates)
    public Vector3 spawnPosition;

    // For the reporting system to check
    public bool hasExtraAnomaly = false;

    public void TriggerExtraAnomaly()
    {
        Debug.Log("TriggerExtraAnomaly called. hasExtraAnomaly = " + hasExtraAnomaly);

        // Only spawn once and make sure we actually have a prefab to spawn
        if (!hasExtraAnomaly && extraObjectPrefab != null)
        {
            Debug.Log("Spawning extra object...");
            // Calculate world position from local position
            Vector3 worldSpawnPos = transform.TransformPoint(spawnPosition);
            // Create the duplicate at the calculated world position
            Instantiate(extraObjectPrefab, worldSpawnPos, transform.rotation);
            hasExtraAnomaly = true;
            Debug.Log("Extra object spawned! hasExtraAnomaly now = " + hasExtraAnomaly);
        }
        else
        {
            Debug.Log("Not spawning - either already spawned or no prefab assigned");
        }
    }
}