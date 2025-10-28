using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Super lightweight segmented battery UI + event when empty.
/// Only toggles the individual bar Images; never disables this parent component
/// so the OnDepleted UnityEvent always has a living component to invoke.
/// </summary>
public class SegmentBattery : MonoBehaviour
{
    // Constants for validation (Coding Standards: Named constants for clarity)
    private const int MIN_BATTERY_BARS = 1;
    private const int MAX_BATTERY_BARS = 10;

    [Header("Config")]
    // How many total bars to display
    [SerializeField] private int totalBars = 3;

    // The bar Images in visual order (e.g., left→right). Only enable/disable these.
    [SerializeField] private Image[] barImages;   // assign Bar1, Bar2, Bar3...

    [Header("Events")]
    // Optional event that can be wired up in the Inspector (e.g., LossScreen.Show())
    // Fires immediately when battery drops to 0, and only once until refilled
    [SerializeField] private UnityEvent onDepleted;

    // Runtime state: how many bars are still lit
    private int barsRemaining;

    // Guard so we don't spam the depletion event
    private bool depletedInvoked;

    /// <summary>How many bars are currently lit.</summary>
    public int Current => barsRemaining;

    /// <summary>Total capacity (mostly useful for debug/UI).</summary>
    public int Total => totalBars;

    /// <summary>
    /// Initializes the battery system.
    /// Clamps totalBars to a sane range, syncs the UI, and validates the setup.
    /// </summary>
    void Awake()
    {
        // Input Validation (Coding Standards: Validate Inspector-assigned values)
        ValidateSetup();

        // Clamp to something sane and sync the UI
        barsRemaining = Mathf.Max(0, totalBars);
        depletedInvoked = barsRemaining <= 0;
        RefreshUI();
    }

    /// <summary>
    /// Validates the battery configuration and logs errors for invalid setups.
    /// Ensures totalBars is within valid range and barImages array is properly configured.
    /// </summary>
    private void ValidateSetup()
    {
        // Validate totalBars range
        if (totalBars < MIN_BATTERY_BARS)
        {
            Debug.LogError($"[SegmentBattery] VALIDATION FAILED: totalBars ({totalBars}) is less than minimum ({MIN_BATTERY_BARS}). Setting to minimum.");
            totalBars = MIN_BATTERY_BARS;
        }

        if (totalBars > MAX_BATTERY_BARS)
        {
            Debug.LogWarning($"[SegmentBattery] VALIDATION WARNING: totalBars ({totalBars}) exceeds recommended maximum ({MAX_BATTERY_BARS}). This may cause performance issues.");
        }

        // Validate barImages array
        if (barImages == null || barImages.Length == 0)
        {
            Debug.LogError("[SegmentBattery] VALIDATION FAILED: barImages array is null or empty! UI will not display properly.");
        }
        else
        {
            // Check if array size matches totalBars
            if (barImages.Length < totalBars)
            {
                Debug.LogWarning($"[SegmentBattery] VALIDATION WARNING: barImages array length ({barImages.Length}) is less than totalBars ({totalBars}). Some bars won't display.");
            }
            else if (barImages.Length > totalBars)
            {
                Debug.LogWarning($"[SegmentBattery] VALIDATION WARNING: barImages array length ({barImages.Length}) is greater than totalBars ({totalBars}). Extra images will be unused.");
            }

            // Check for null entries in the array
            int nullCount = 0;
            for (int i = 0; i < barImages.Length; i++)
            {
                if (barImages[i] == null)
                {
                    nullCount++;
                }
            }

            if (nullCount > 0)
            {
                Debug.LogError($"[SegmentBattery] VALIDATION FAILED: {nullCount} null Image reference(s) found in barImages array!");
            }
        }

        // Validate onDepleted event
        if (onDepleted == null)
        {
            Debug.LogWarning("[SegmentBattery] VALIDATION WARNING: onDepleted event is null. No action will be taken when battery depletes.");
        }
        else if (onDepleted.GetPersistentEventCount() == 0)
        {
            Debug.LogWarning("[SegmentBattery] VALIDATION WARNING: onDepleted event has no listeners. Consider adding a LossScreen.Show() or similar action.");
        }

        Debug.Log($"[SegmentBattery] Setup complete - {totalBars} bars configured.");
    }

    /// <summary>
    /// Spends battery bars (defaults to 1).
    /// If this takes the battery to zero, fires onDepleted event exactly once.
    /// Will not consume bars if battery is already depleted.
    /// </summary>
    /// <param name="bars">Number of bars to consume (will be converted to positive value)</param>
    public void Consume(int bars = 1)
    {
        // Coding Standards: Always use braces for if statements
        if (barsRemaining <= 0)
        {
            return; // Already depleted; nothing to do
        }

        // Reduce battery by the specified amount (ensure positive value)
        barsRemaining = Mathf.Max(0, barsRemaining - Mathf.Abs(bars));
        RefreshUI();

        // Fire depletion event if we just hit zero
        if (barsRemaining == 0 && !depletedInvoked)
        {
            depletedInvoked = true;
            onDepleted?.Invoke(); // Safe even if nothing is hooked up
            Debug.Log("[SegmentBattery] Battery depleted! Firing onDepleted event.");
        }

        Debug.Log($"[SegmentBattery] Consumed {bars} bar(s). Remaining: {barsRemaining}/{totalBars}");
    }

    /// <summary>
    /// Restores battery bars (handy for debugging or future power-up pickups).
    /// If battery comes back from 0, allows onDepleted to fire again next time.
    /// </summary>
    /// <param name="bars">Number of bars to restore (will be converted to positive value)</param>
    public void Refill(int bars)
    {
        int before = barsRemaining;
        barsRemaining = Mathf.Clamp(before + Mathf.Abs(bars), 0, totalBars);

        // Reset depletion flag if we come back from zero
        if (before == 0 && barsRemaining > 0)
        {
            depletedInvoked = false;
        }

        RefreshUI();
        Debug.Log($"[SegmentBattery] Refilled {bars} bar(s). Current: {barsRemaining}/{totalBars}");
    }

    /// <summary>
    /// Updates the UI to reflect the current battery state.
    /// Enables/disables individual bar images based on barsRemaining.
    /// Intentionally doesn't disable this parent object so events can still fire.
    /// </summary>
    private void RefreshUI()
    {
        // Coding Standards: Always use braces for if statements
        if (barImages == null)
        {
            return;
        }

        // Enable/disable each bar image based on current battery level
        for (int i = 0; i < barImages.Length; i++)
        {
            if (barImages[i])
            {
                barImages[i].enabled = i < barsRemaining;
            }
        }
    }
}