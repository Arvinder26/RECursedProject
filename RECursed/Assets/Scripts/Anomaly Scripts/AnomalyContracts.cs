using UnityEngine;

// Rooms you can report anomalies in.
public enum Room
{
    Kitchen,
    Bedroom1,
    LivingRoom,
    EnsuiteBedroom,
    WalkinWardrobe,
    Bathroom,
    Garage,
    Office,        // Round 5 room.
    DiningRoom     // Round 5 room.
}

// All supported anomaly categories.
public enum AnomalyType
{
    MovedObject,
    ObjectDisappeared,
    ShadowEntity,
    LightFlicker,
    ExtraObject
}

// Minimal contract every anomaly implements.
public interface IAnomaly
{
    Room Room { get; }		// Which room this anomaly belongs to.
    AnomalyType Type { get; }   // What kind of anomaly this is.
    
    bool IsActive { get; }	// True while the anomaly is live.
    
    void Trigger();		// Turn the anomaly on.
    
    void Revert();		// Turn the anomaly off.
    
    float GetTimeRemaining();   // Seconds left to report, or 0 if inactive.
}