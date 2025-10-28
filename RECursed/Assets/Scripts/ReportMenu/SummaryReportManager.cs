using UnityEngine;
using TMPro;

/// <summary>
/// Manages the summary report display showing player performance statistics.
/// Tracks correctly reported, missed, and misreported anomalies throughout rounds.
/// Updates UI text automatically when counts change.
/// </summary>
public class SummaryReportManager : MonoBehaviour
{
    [SerializeField] private TMP_Text summaryText;

    private int correctCount = 0;
    private int missedCount = 0;
    private int misreportedCount = 0;

    /// <summary>
    /// Initializes the summary report manager.
    /// Auto-finds the summary text component if not assigned and displays initial counts.
    /// </summary>
    private void Awake()
    {
        // Coding Standards: Always use braces for if statements
        if (!summaryText)
        {
            summaryText = GetComponentInChildren<TMP_Text>();
        }

        RefreshText();

        // Input Validation (Coding Standards: Validate Inspector-assigned values)
        ValidateSetup();
    }

    /// <summary>
    /// Validates the summary report configuration and logs warnings for missing references.
    /// </summary>
    private void ValidateSetup()
    {
        if (!summaryText)
        {
            Debug.LogError("[SummaryReportManager] VALIDATION FAILED: Summary Text is not assigned! Summary report will not display.");
        }
        else
        {
            Debug.Log("[SummaryReportManager] Setup complete - Ready to track anomaly reports.");
        }
    }

    /// <summary>
    /// Increments the correct report count and updates the display.
    /// Called when the player successfully reports an anomaly.
    /// </summary>
    public void ShowSuccess()
    {
        correctCount++;
        RefreshText();
    }

    /// <summary>
    /// Increments the missed anomaly count and updates the display.
    /// Called when an anomaly expires without being reported.
    /// </summary>
    public void ShowMissed()
    {
        missedCount++;
        RefreshText();
    }

    /// <summary>
    /// Increments the misreported anomaly count and updates the display.
    /// Called when the player reports a non-existent anomaly (false positive).
    /// </summary>
    public void ShowMisreport()
    {
        misreportedCount++;
        RefreshText();
    }

    /// <summary>
    /// Resets all counts to zero and updates the display.
    /// Called at the start of a new round to clear previous round statistics.
    /// </summary>
    public void ResetCounts()
    {
        correctCount = 0;
        missedCount = 0;
        misreportedCount = 0;
        RefreshText();
    }

    /// <summary>
    /// Updates the summary text display with current counts.
    /// Uses color-coded text: green for correct, yellow for missed, red for misreported.
    /// </summary>
    private void RefreshText()
    {
        // Coding Standards: Always use braces for if statements
        if (!summaryText)
        {
            return;
        }

        summaryText.text =
            $"<color=green>Correctly reported - {correctCount}</color>\n\n" +
            $"<color=yellow>Missed - {missedCount}</color>\n\n" +
            $"<color=red>Misreported - {misreportedCount}</color>";
    }
}