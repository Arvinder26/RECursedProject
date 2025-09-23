using UnityEngine;

public class AnomalyTrigger : MonoBehaviour
{
    // How many seconds after game starts should this anomaly trigger
    public float triggerTime = 60f;

    // Make sure we only trigger once
    private bool hasTriggered = false;

    void Update()
    {
        // Check if enough time has passed and we haven't triggered yet
        if (!hasTriggered && Time.time > triggerTime)
        {
            // Look for MovedObject script on this same object
            MovedObject movedScript = GetComponent<MovedObject>();
            if (movedScript != null)
            {
                movedScript.TriggerMovedAnomaly();
            }

            // Look for DisappearedObject script on this same object
            DisappearedObject disappearedScript = GetComponent<DisappearedObject>();
            if (disappearedScript != null)
            {
                disappearedScript.TriggerDisappearedAnomaly();
            }

            // Look for ExtraObject script on this same object
            ExtraObject extraScript = GetComponent<ExtraObject>();
            if (extraScript != null)
            {
                extraScript.TriggerExtraAnomaly();
            }

            // Mark as triggered so this doesn't keep running every frame
            hasTriggered = true;
            Debug.Log("Anomaly trigger completed - no more triggers will fire");
        }
    }
}