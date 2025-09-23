using UnityEngine;
using TMPro;

public class GameClockController : MonoBehaviour
{
    [Header("Clock UI")]
    [SerializeField] private TMP_Text clockText;

    [Header("Time Control")]
    [Tooltip("Multiplier for how fast time passes. 1 = normal speed")]

    [Header("End Panel")]
    [SerializeField] private GameObject endPanel;

    private float elapsedTime;
    public float timeMultiplier = 60f;
    private bool endTriggered = false;

    private void Update()
    {
        Debug.Log("Update running!");

        if (!endTriggered)
        {
            elapsedTime += Time.deltaTime * timeMultiplier;
        }
        updateClockUI();
    }

    void updateClockUI()
    {
        int totalSeconds = Mathf.FloorToInt(elapsedTime);

        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        int displayHours = (hours + 12) % 12;
        if (displayHours == 0) displayHours = 12;

        string clockString = string.Format("{0:00}:{1:00} AM", displayHours, minutes);
        clockText.text = clockString;

        if (!endTriggered && displayHours == 6 && minutes == 0)
        {
            endTriggered = true;
            if (endPanel != null)
            {
                endPanel.SetActive(true);
            }
            Debug.Log("It's 6:00 AM! End panel shown.");
        }
    }
}
