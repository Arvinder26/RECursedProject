using UnityEngine;

public class DisappearedObject : MonoBehaviour
{
    // Your friend can check this to see if the object disappeared
    public bool hasDisappearedAnomaly = false;

    public void TriggerDisappearedAnomaly()
    {
        // Make sure we only disappear once
        if (!hasDisappearedAnomaly)
        {
            // Just turn off the entire game object - poof, it's gone!
            gameObject.SetActive(false);
            hasDisappearedAnomaly = true;
        }
    }
}
