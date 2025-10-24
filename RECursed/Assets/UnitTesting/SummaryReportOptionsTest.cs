using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class SummaryReportOptionsTest
{
    [Test]
    public void PlayerChoosesContinue()
    {
        var summaryManager = new SummaryManager();
        summaryManager.SetTime(6, 0);
        Assert.IsTrue(summaryManager.ShouldDisplaySummary());

        summaryManager.SelectOption("Continue");

        Assert.AreEqual(2, summaryManager.GetCurrentRound());
        Assert.IsFalse(summaryManager.ShouldDisplaySummary());
    }

    [Test]
    public void PlayerChoosesReplay()
    {
        var summaryManager = new SummaryManager();
        summaryManager.SetTime(6, 0);
        Assert.IsTrue(summaryManager.ShouldDisplaySummary());

        int initialRound = summaryManager.GetCurrentRound();

        summaryManager.SelectOption("Replay");

        Assert.AreEqual(initialRound, summaryManager.GetCurrentRound(), "Replaying should be same round number.");
        Assert.IsFalse(summaryManager.ShouldDisplaySummary());
    }

    [Test]
    public void PlayerChoosesExitMain()
    {
        var summaryManager = new SummaryManager();
        summaryManager.SetTime(6, 0);
        Assert.IsTrue(summaryManager.ShouldDisplaySummary());

        summaryManager.SelectOption("Exit");

        Assert.IsTrue(summaryManager.IsInMainMenu());
        Assert.IsFalse(summaryManager.ShouldDisplaySummary());
    }

    [Test]
    public void InvalidChoice_ThrowException()
    {
        var summaryManager = new SummaryManager();
        summaryManager.SetTime(6, 0);

        Assert.Throws<ArgumentException>(() => summaryManager.SelectOption("Invalid"));

    }
}
