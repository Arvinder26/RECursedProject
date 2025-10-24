using UnityEngine;
using TMPro;

public class SummaryReportManager : MonoBehaviour
{
    [SerializeField] private TMP_Text summaryText;

    private int correctCount = 0;
    private int missedCount = 0;
    private int misreportedCount = 0;

    private void Awake()
    {
        if (!summaryText)
            summaryText = GetComponentInChildren<TMP_Text>();

        RefreshText();
    }

    public void ShowSuccess()
    {
        correctCount++;
        RefreshText();
    }

    public void ShowMissed()
    {
        missedCount++;
        RefreshText();
    }

    public void ShowMisreport()
    {
        misreportedCount++;
        RefreshText();
    }
    public void ResetCounts()
    {
        correctCount = 0;
        missedCount = 0;
        misreportedCount = 0;
        RefreshText();
    }

    private void RefreshText()
    {
        summaryText.text =
            $"<color=green>Correctly reported - {correctCount}</color>\n\n" +
            $"<color=yellow>Missed - {missedCount}</color>\n\n" +
            $"<color=red>Misreported - {misreportedCount}</color>";
    }
}
