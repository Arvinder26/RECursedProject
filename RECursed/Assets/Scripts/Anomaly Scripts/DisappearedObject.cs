using System.Collections.Generic;
using UnityEngine;

public class DisappearedObject : MonoBehaviour, IAnomaly
{
    [Header("Setup")]
    [SerializeField] private Room room = Room.Bedroom1;

    [Tooltip("Optional: specify which components to toggle. If empty, all Renderers & Colliders in children are used.")]
    [SerializeField] private Renderer[] renderersToToggle = new Renderer[0];
    [SerializeField] private Collider[] collidersToToggle = new Collider[0];

    [Header("Debug/State")]
    public bool hasDisappearedAnomaly = false;

    // IAnomaly
    public Room Room => room;
    public AnomalyType Type => AnomalyType.ObjectDisappeared;
    public bool IsActive => hasDisappearedAnomaly;

    void Awake()
    {
        if (renderersToToggle == null || renderersToToggle.Length == 0)
            renderersToToggle = GetComponentsInChildren<Renderer>(true);

        if (collidersToToggle == null || collidersToToggle.Length == 0)
            collidersToToggle = GetComponentsInChildren<Collider>(true);
    }

    public void TriggerDisappearedAnomaly()
    {
        if (hasDisappearedAnomaly) return;

        SetVisible(false);
        hasDisappearedAnomaly = true;
    }

    // IAnomaly
    public void Trigger() => TriggerDisappearedAnomaly();

    // IAnomaly
    public void Revert()
    {
        SetVisible(true);
        hasDisappearedAnomaly = false;
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderersToToggle)
            if (r) r.enabled = visible;

        foreach (var c in collidersToToggle)
            if (c) c.enabled = visible;
    }
}
