using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SegmentBattery : MonoBehaviour
{
    [Header("Cells")]
    [SerializeField] Transform cellsParent;              // BatteryPanel
    [SerializeField] Color filledColor = new Color(0.42f, 1f, 0.38f, 1f);
    [SerializeField] Color emptyColor  = new Color(0.25f, 0.25f, 0.25f, 0.8f);

    [Header("Values")]
    [SerializeField] int maxSegments = 3;
    [SerializeField] int startSegments = 3;

    [Header("On Depleted")]
    public UnityEvent onDepleted;                        // Hook your loss UI here

    readonly List<Image> _cells = new();
    int _current;

    public int Current => _current;
    public int Max => maxSegments;

    void Awake()
    {
        if (!cellsParent) cellsParent = transform;
        _cells.Clear();

        foreach (Transform t in cellsParent)
        {
            var img = t.GetComponent<Image>();
            if (img) _cells.Add(img);
        }

        // If you built N children, use that as max
        if (_cells.Count > 0) maxSegments = _cells.Count;

        _current = Mathf.Clamp(startSegments, 0, maxSegments);
        Refresh();
    }

    void Refresh()
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            if (!_cells[i]) continue;
            _cells[i].color = (i < _current) ? filledColor : emptyColor;
        }
    }

    public void ApplyPenalty(int amount = 1)
    {
        if (_current <= 0) return;
        _current = Mathf.Max(0, _current - Mathf.Abs(amount));
        Refresh();
        if (_current == 0) onDepleted?.Invoke();
    }

    public void Recharge(int amount = 1)
    {
        _current = Mathf.Min(maxSegments, _current + Mathf.Abs(amount));
        Refresh();
    }

    public void SetValue(int value)
    {
        _current = Mathf.Clamp(value, 0, maxSegments);
        Refresh();
        if (_current == 0) onDepleted?.Invoke();
    }

    public void ResetBattery() { SetValue(startSegments); }
}
