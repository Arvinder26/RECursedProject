using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Triggers one or more anomalies now or after a delay.
public class AnomalyTrigger : MonoBehaviour
{
    [Tooltip("Auto trigger this many seconds after Start. Set <= 0 to not auto-trigger.")]
    public float triggerTime = 60f; // Delay before auto-triggering.

    [Tooltip("Explicit anomalies to trigger. If empty, IAnomaly components on this GameObject will be used.")]
    public List<MonoBehaviour> explicitAnomalies = new(); // Optional explicit targets.

    private List<IAnomaly> _targets = new(); // Cached anomalies to trigger.
    private bool _hasTriggered;		     // Guard against double trigger.

    void Awake()
    {
        _targets.Clear(); // Reset target list.

        if (explicitAnomalies != null && explicitAnomalies.Count > 0)
        {
            foreach (var mb in explicitAnomalies)
                if (mb is IAnomaly a) _targets.Add(a); // Use provided list.
        }
        else
        {
            GetComponents<IAnomaly>(_targets); // Auto-find on this GameObject.
        }
    }

    void Start()
    {
        if (triggerTime > 0f)
            StartCoroutine(AutoTriggerAfter(triggerTime)); // Schedule auto trigger.
    }

    public void TriggerNow()
    {
        if (_hasTriggered) return; // Only fire once.
        foreach (var a in _targets) a.Trigger(); // Activate each target.
        _hasTriggered = true; // Mark as done.
    }

    IEnumerator AutoTriggerAfter(float t)
    {
        yield return new WaitForSeconds(t); // Simple delay.
        TriggerNow(); // Fire after wait.
    }
}
