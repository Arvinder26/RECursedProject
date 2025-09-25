using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SegmentBattery : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] int totalBars = 3;
    [SerializeField] Image[] barImages;   // assign Bar1, Bar2, Bar3...

    [Header("Events")]
    [SerializeField] UnityEvent onDepleted; // optional; keep if you wired it

    int barsRemaining;
    bool depletedInvoked;

    public int Current => barsRemaining;
    public int Total   => totalBars;

    void Awake()
    {
        barsRemaining   = Mathf.Max(0, totalBars);
        depletedInvoked = barsRemaining <= 0;
        RefreshUI();
    }

    public void Consume(int bars = 1)
    {
        if (barsRemaining <= 0) return;
        barsRemaining = Mathf.Max(0, barsRemaining - Mathf.Abs(bars));
        RefreshUI();

        if (barsRemaining == 0 && !depletedInvoked)
        {
            depletedInvoked = true;
            onDepleted?.Invoke(); // fine if nothing is hooked
        }
    }

    public void Refill(int bars)
    {
        int before = barsRemaining;
        barsRemaining = Mathf.Clamp(before + Mathf.Abs(bars), 0, totalBars);
        if (before == 0 && barsRemaining > 0) depletedInvoked = false;
        RefreshUI();
    }

    void RefreshUI()
    {
        if (barImages == null) return;
        for (int i = 0; i < barImages.Length; i++)
        {
            if (barImages[i]) barImages[i].enabled = i < barsRemaining;
        }
    }
}
