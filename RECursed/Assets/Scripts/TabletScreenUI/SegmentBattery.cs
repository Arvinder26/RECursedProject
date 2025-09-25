using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Super lightweight segmented battery UI + event when empty.
/// I only toggle the individual bar Images; I never disable this parent
/// so the OnDepleted UnityEvent always has a living component to invoke.
/// </summary>
public class SegmentBattery : MonoBehaviour
{
    [Header("Config")]
    // How many total bars I want to display.
    [SerializeField] int totalBars = 3;

    // The bar Images in visual order (e.g., left→right). I just enable/disable these.
    [SerializeField] Image[] barImages;   // assign Bar1, Bar2, Bar3...

    [Header("Events")]
    // Optional event I can wire up in the Inspector (e.g., LossScreen.Show()).
    // Fires immediately when I drop to 0, and only once until I refill.
    [SerializeField] UnityEvent onDepleted; // optional; keep if you wired it

    // Runtime state: how many bars are still lit.
    int barsRemaining;

    // Guard so I don't spam the depletion event.
    bool depletedInvoked;

    /// <summary>How many bars I currently have lit.</summary>
    public int Current => barsRemaining;

    /// <summary>Total capacity (mostly useful for debug/UI).</summary>
    public int Total   => totalBars;

    void Awake()
    {
        // Clamp to something sane and sync the UI.
        barsRemaining   = Mathf.Max(0, totalBars);
        depletedInvoked = barsRemaining <= 0;
        RefreshUI();
    }

    /// <summary>
    /// Spend some bars (defaults to 1). If this takes me to zero,
    /// I fire onDepleted exactly once.
    /// </summary>
    public void Consume(int bars = 1)
    {
        if (barsRemaining <= 0) return; // already dead; nothing to do

        barsRemaining = Mathf.Max(0, barsRemaining - Mathf.Abs(bars));
        RefreshUI();

        if (barsRemaining == 0 && !depletedInvoked)
        {
            depletedInvoked = true;
            onDepleted?.Invoke(); // fine if nothing is hooked
        }
    }

    /// <summary>
    /// Give back some bars (handy for debugging or future pickups).
    /// If I come back from 0, I allow onDepleted to fire again next time.
    /// </summary>
    public void Refill(int bars)
    {
        int before = barsRemaining;
        barsRemaining = Mathf.Clamp(before + Mathf.Abs(bars), 0, totalBars);
        if (before == 0 && barsRemaining > 0) depletedInvoked = false;
        RefreshUI();
    }

    /// <summary>
    /// Turn each bar image on/off based on barsRemaining.
    /// I intentionally don't disable this parent object here.
    /// </summary>
    void RefreshUI()
    {
        if (barImages == null) return;

        for (int i = 0; i < barImages.Length; i++)
        {
            if (barImages[i])
                barImages[i].enabled = i < barsRemaining;
        }
    }
}
