using System;
using UnityEngine;

public class SummaryManager
{
    private int hour;
    private int minute;

    private bool inMainMenu = false;
    private int currentRound = 1;

    public void SetTime(int h, int m)
    {
        // Check negative values
        if (h < 0 || m < 0)
        {
            throw new ArgumentOutOfRangeException("Hour or minute cannot be negative");
        }

        // Check valid ranges
        if (h > 23 || m > 59)
        {
            throw new ArgumentOutOfRangeException("Hour must be 0-23 and minute 0-59");
        }

        // Validate with RECursed's in-game time rules, allow only between 0:00 to 6:00AM
        if (h > 6 || (h == 6 && m > 0))
            throw new ArgumentOutOfRangeException("Time cannot be after 6AM");

        hour = h;
        minute = m;
    }

    public bool ShouldDisplaySummary()
    {
        return hour == 6 && minute == 0;
    }

    public void ContinueToNextRound()
    {
        if(!ShouldDisplaySummary()) return;

        currentRound++;
        ResetTime();
    }

    public void ReplayRound()
    {
        if (!ShouldDisplaySummary()) return;

        ResetTime();
    }

    public void ExitToMainMenu()
    {
        if (!ShouldDisplaySummary()) return;

        inMainMenu = true;
        ResetTime();
    }

    public bool IsInMainMenu()
    {
        return inMainMenu;
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public void SelectOption(String option)
    {
        if (!ShouldDisplaySummary()) return;

        switch (option.ToLower())
        {
            case "continue":
                ContinueToNextRound();
                break;
            case "replay":
                ReplayRound();
                break;
            case "exit":
                ExitToMainMenu();
                break;
            default:
                throw new ArgumentException("Invalid option");
        }
    }

    private void ResetTime()
    {
        hour = 0;
        minute = 0;
    }
}
