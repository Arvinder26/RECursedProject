using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalyTrigger : MonoBehaviour
{
    [Tooltip("Auto trigger this many seconds after Start. Set <= 0 to not auto-trigger.")]
    public float triggerTime = 60f;

    [Tooltip("Explicit anomalies to trigger. If empty, IAnomaly components on this GameObject will be used.")]
    public List<MonoBehaviour> explicitAnomalies = new(); 

    private List<IAnomaly> _targets = new();
    private bool _hasTriggered;

    void Awake()
    {
        _targets.Clear();

        if (explicitAnomalies != null && explicitAnomalies.Count > 0)
        {
            foreach (var mb in explicitAnomalies)
                if (mb is IAnomaly a) _targets.Add(a);
        }
        else
        {
            GetComponents<IAnomaly>(_targets);
        }
    }

    void Start()
    {
        if (triggerTime > 0f)
            StartCoroutine(AutoTriggerAfter(triggerTime));
    }

    public void TriggerNow()
    {
        if (_hasTriggered) return;
        foreach (var a in _targets) a.Trigger();
        _hasTriggered = true;
    }

    IEnumerator AutoTriggerAfter(float t)
    {
        yield return new WaitForSeconds(t);
        TriggerNow();
    }
}
