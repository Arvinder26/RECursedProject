using UnityEngine;
using TMPro;

// Tiny results tracker for the current session/round
public class SummaryReportManager : MonoBehaviour
{
    [SerializeField] private TMP_Text summaryText; // Assigned in Inspector (auto-finds if missing)

    // Counters for each result type
    private int correctCount = 0;
    private int missedCount = 0;
    private int misreportedCount = 0;

    private void Awake()
    {
        if (!summaryText)
            summaryText = GetComponentInChildren<TMP_Text>(); // Fallback for convenience

        RefreshText(); // Ensure UI isn't blank on load
    }

    // Called when player correctly reports an anomaly
    public void ShowSuccess()
    {
        correctCount++;
        RefreshText();
    }

    // Called when player misses to report an anomaly
    public void ShowMissed()
    {
        missedCount++;
        RefreshText();
    }

    // Called when player reports a false anomaly
    public void ShowMisreport()
    {
        misreportedCount++;
        RefreshText();
    }

    // Resets all counts to zero
    public void ResetCounts()
    {
        correctCount = 0;
        missedCount = 0;
        misreportedCount = 0;
        RefreshText();
    }

    // Update UI with current results
    private void RefreshText()
    {
        summaryText.text =
            $"<color=green>Correctly reported - {correctCount}</color>\n\n" +
            $"<color=yellow>Missed - {missedCount}</color>\n\n" +
            $"<color=red>Misreported - {misreportedCount}</color>";
    }
}
