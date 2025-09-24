using UnityEngine;

public class ExtraObject : MonoBehaviour, IAnomaly
{
    [Header("Setup")]
    [SerializeField] private Room room = Room.Bedroom1;
    [Tooltip("Prefab to spawn when the anomaly triggers.")]
    public GameObject extraObjectPrefab;

    [Tooltip("Local position offset (relative to this object) to spawn at.")]
    public Vector3 spawnPosition;

    [Header("Debug/State")]
    public bool hasExtraAnomaly = false;

    private GameObject spawnedInstance;

    // IAnomaly
    public Room Room => room;
    public AnomalyType Type => AnomalyType.ExtraObject;
    public bool IsActive => hasExtraAnomaly;

    public void TriggerExtraAnomaly()
    {
        if (hasExtraAnomaly || !extraObjectPrefab) return;

        Vector3 worldPos = transform.TransformPoint(spawnPosition);
        spawnedInstance = Instantiate(extraObjectPrefab, worldPos, transform.rotation);
        hasExtraAnomaly = true;
    }

    // IAnomaly
    public void Trigger() => TriggerExtraAnomaly();

    // IAnomaly
    public void Revert()
    {
        if (spawnedInstance)
            Destroy(spawnedInstance);

        spawnedInstance = null;
        hasExtraAnomaly = false;
    }
}
