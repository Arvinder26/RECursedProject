using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Central registry for anomalies.

public class AnomalyManager : MonoBehaviour
{
    [Header("Note: Respawn is now controlled by RoundManager")]
    [Tooltip("This delay is no longer used - kept for backward compatibility")]
    public float respawnDelay = 60f;

    // Live list of all anomalies found in the scene at startup.
    private readonly List<IAnomaly> _anomalies = new List<IAnomaly>();

    void Awake()
    {
	// Grab every MonoBehaviour (even on inactive objects), then filter by IAnomaly.
        var allBehaviours = FindObjectsOfType<MonoBehaviour>(true);

        _anomalies.Clear();
        foreach (var mb in allBehaviours)
        {
            if (mb is IAnomaly a)
                _anomalies.Add(a);
        }
        
        Debug.Log($"[AnomalyManager] Found {_anomalies.Count} anomalies in scene.");
    }

    // Given the player's selected room and anomaly type, check if an ACTIVE anomaly matches.
    public bool ValidateAndResolve(Room reportedRoom, AnomalyType reportedType)
    {
        // Look for the first ACTIVE anomaly that matches both room and type.
        var match = _anomalies.FirstOrDefault(a =>
            a.Room == reportedRoom &&
            a.Type == reportedType &&
            a.IsActive);

        if (match != null)
        {
            // Turn it off now.
            match.Revert();
            
            Debug.Log($"[AnomalyManager] Correct report! Reverted {reportedRoom} - {reportedType}");
                   
            return true;
        }

        Debug.Log($"[AnomalyManager] Wrong report: {reportedRoom} - {reportedType} (no active anomaly found)");
        return false;
    }
}