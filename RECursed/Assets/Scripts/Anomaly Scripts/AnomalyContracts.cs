using UnityEngine;

public enum Room
{
    Kitchen,
    Bedroom1,
    LivingRoom,
    EnsuiteBedroom,
    WalkinWardrobe,
    Bathroom,
    Garage
}

public enum AnomalyType
{
    MovedObject,
    ObjectDisappeared,
    ShadowEntity,
    LightFlicker,
    ExtraObject
}

public interface IAnomaly
{
    Room Room { get; }
    AnomalyType Type { get; }
    
    bool IsActive { get; }
    
    void Trigger();
    
    void Revert();
    
    float GetTimeRemaining();
}