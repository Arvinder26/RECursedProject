using UnityEngine;

public class MovedObject : MonoBehaviour, IAnomaly
{
    [Header("Setup")]
    [SerializeField] private Room room = Room.Bedroom1;

    [Tooltip("Local position to move to when the anomaly triggers.")]
    public Vector3 newPosition;

    [Header("Debug/State")]
    public bool hasMovedAnomaly = false;

    private Vector3 originalLocalPosition;

    // IAnomaly
    public Room Room => room;
    public AnomalyType Type => AnomalyType.MovedObject;
    public bool IsActive => hasMovedAnomaly;

    void Start()
    {
        originalLocalPosition = transform.localPosition;
        
    }

    public void TriggerMovedAnomaly()
    {
        if (hasMovedAnomaly) return;

        transform.localPosition = newPosition;
        hasMovedAnomaly = true;
    }

    // IAnomaly
    public void Trigger() => TriggerMovedAnomaly();

    // IAnomaly
    public void Revert()
    {
        transform.localPosition = originalLocalPosition;
        hasMovedAnomaly = false;
    }
}
