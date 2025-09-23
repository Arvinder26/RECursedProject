using UnityEngine;

public class MovedObject : MonoBehaviour
{
    // Set the local coordinates relative to the parent object
    public Vector3 newPosition;

    // This gets checked by the reporting system to see if anomaly happened
    public bool hasMovedAnomaly = false;

    // We need to remember where the object started locally
    private Vector3 originalLocalPosition;

    void Start()
    {
        // Save the starting local position relative to parent
        originalLocalPosition = transform.localPosition;
        Debug.Log("Bear starting local position: " + originalLocalPosition);
        Debug.Log("Bear target local position: " + newPosition);
    }

    public void TriggerMovedAnomaly()
    {
        // Only move once, don't want it moving multiple times
        if (!hasMovedAnomaly)
        {
            Debug.Log("TRIGGERING ANOMALY - Moving bear from " + transform.localPosition + " to " + newPosition);
            // Move using local position relative to parent
            transform.localPosition = newPosition;
            hasMovedAnomaly = true;
            Debug.Log("Bear moved! New local position: " + transform.localPosition);
        }
    }
}