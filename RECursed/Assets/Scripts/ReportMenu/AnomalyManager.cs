using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central registry for anomalies.
/// - Validates if player reports are correct
/// - Reverts anomalies when reported correctly
/// - NO LONGER re-triggers anomalies (RoundManager handles that now!)
/// </summary>
public class AnomalyManager : MonoBehaviour
{
    [Header("Note: Respawn is now controlled by RoundManager")]
    [Tooltip("This delay is no longer used - kept for backward compatibility")]
    public float respawnDelay = 60f;

    private readonly List<IAnomaly> _anomalies = new List<IAnomaly>();

    void Awake()
    {
        // Find ANY MonoBehaviour in the scene (including inactive),
        // then pick the ones that implement IAnomaly interface.
        var allBehaviours = FindObjectsOfType<MonoBehaviour>(true);

        _anomalies.Clear();
        foreach (var mb in allBehaviours)
        {
            if (mb is IAnomaly a)
                _anomalies.Add(a);
        }
        
        Debug.Log($"[AnomalyManager] Found {_anomalies.Count} anomalies in scene.");
    }

    /// <summary>
    /// Given a room+type pair from the report UI, decide if it's correct.
    /// If there's an ACTIVE anomaly with the same room & type:
    ///   - Revert it immediately
    ///   - Let RoundManager handle re-triggering (we don't do it anymore!)
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
            // Turn it off immediately
            match.Revert();
            
            Debug.Log($"[AnomalyManager] Correct report! Reverted {reportedRoom} - {reportedType}");
            
            // RoundManager will re-trigger it later based on round settings
            // We don't need to do anything else here!
            
            return true;
        }

        Debug.Log($"[AnomalyManager] Wrong report: {reportedRoom} - {reportedType} (no active anomaly found)");
        return false;
    }
}