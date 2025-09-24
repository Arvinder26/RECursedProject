using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    [Tooltip("How long after a correct report before the same anomaly will trigger again.")]
    public float respawnDelay = 60f;

    private readonly List<IAnomaly> _anomalies = new List<IAnomaly>();

    void Awake()
    {
        var allBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        _anomalies.Clear();
        foreach (var mb in allBehaviours)
        {
            if (mb is IAnomaly a)
                _anomalies.Add(a);
        }
    }

    public bool ValidateAndResolve(Room reportedRoom, AnomalyType reportedType)
    {
        var match = _anomalies.FirstOrDefault(a =>
            a.Room == reportedRoom &&
            a.Type == reportedType &&
            a.IsActive);

        if (match != null)
        {
            match.Revert();
            StartCoroutine(ReArmAfterDelay(match, respawnDelay));
            return true;
        }

        return false;
    }

    private IEnumerator ReArmAfterDelay(IAnomaly anomaly, float delay)
    {
        yield return new WaitForSeconds(delay);
        anomaly.Trigger();
    }
}
