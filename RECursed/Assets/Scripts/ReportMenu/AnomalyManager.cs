using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central registry/authority for anomalies.
/// - On Awake, I scan the scene for anything that implements IAnomaly
/// - When the player reports, I check if there's an active anomaly that matches
/// - If correct, I revert it and schedule it to re-arm later
/// </summary>
public class AnomalyManager : MonoBehaviour
{
    [Tooltip("How long after a correct report before the same anomaly will trigger again.")]
    public float respawnDelay = 60f;

    // I keep all discovered anomalies here so lookups are fast.
    private readonly List<IAnomaly> _anomalies = new List<IAnomaly>();

    void Awake()
    {
        // Find ANY MonoBehaviour in the scene (including inactive),
        // then pick the ones that implement my IAnomaly interface.
        var allBehaviours = FindObjectsOfType<MonoBehaviour>(true);

        _anomalies.Clear();
        foreach (var mb in allBehaviours)
        {
            if (mb is IAnomaly a)
                _anomalies.Add(a);
        }
    }

    /// <summary>
    /// Given a room+type pair from the report UI, decide if it's correct.
    /// If there's an ACTIVE anomaly with the same room & type:
    ///   - Revert it now
    ///   - Schedule it to re-arm after a delay (so it can show up again later)
    /// Returns true on correct report; false on wrong/no match.
    /// </summary>
    public bool ValidateAndResolve(Room reportedRoom, AnomalyType reportedType)
    {
        // Look for the first active anomaly that matches both the room and type.
        var match = _anomalies.FirstOrDefault(a =>
            a.Room == reportedRoom &&
            a.Type == reportedType &&
            a.IsActive);

        if (match != null)
        {
            // Turn it off immediately…
            match.Revert();

            // …and let it come back after the cooldown.
            StartCoroutine(ReArmAfterDelay(match, respawnDelay));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Wait for a bit, then tell that anomaly to trigger itself again.
    /// Keeping this as a coroutine keeps things simple and scene-local.
    /// </summary>
    private IEnumerator ReArmAfterDelay(IAnomaly anomaly, float delay)
    {
        yield return new WaitForSeconds(delay);
        anomaly.Trigger();
    }
}
