using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class SummaryReportTest
{
    [Test]
    public void SummaryReportAppears()
    {
        var summaryManager = new SummaryManager();

        summaryManager.SetTime(6, 0);
        Assert.IsTrue(summaryManager.ShouldDisplaySummary(), "Summary report should appear at 6:00 AM");
    }

    [Test]
    public void IfTimeBefore12AM_ThrowException()
    {
        var summaryManager = new SummaryManager();

        Assert.Throws<ArgumentOutOfRangeException>(() => summaryManager.SetTime(23, 59), "Time cannot be before 12AM");
        Assert.Throws<ArgumentOutOfRangeException>(() => summaryManager.SetTime(22, 45), "Time cannot be before 12AM");
        Assert.Throws<ArgumentOutOfRangeException>(() => summaryManager.SetTime(14, 00), "Time cannot be before 12AM");
    }

    [Test]
    public void IfTimeAfter6AM_ThrowException()
    {
        var summaryManager = new SummaryManager();

        Assert.Throws<ArgumentOutOfRangeException>(() => summaryManager.SetTime(6, 1), "Time cannot be after 6AM");
        Assert.Throws<ArgumentOutOfRangeException>(() => summaryManager.SetTime(7, 35), "Time cannot be after 6AM");
        Assert.Throws<ArgumentOutOfRangeException>(() => summaryManager.SetTime(12, 21), "Time cannot be after 6AM");
    }

    [Test]
    public void IfTimeIsNegative_ThrowException()
    {
        var summaryManager = new SummaryManager();

        Assert.Throws<ArgumentOutOfRangeException>(() => summaryManager.SetTime(-1, 0), "Negative hour should throw exception");
        Assert.Throws<ArgumentOutOfRangeException>(() => summaryManager.SetTime(0, -1), "Negative minute should throw exception");
        Assert.Throws<ArgumentOutOfRangeException>(() => summaryManager.SetTime(-5, -10), "Negative hour and minute should throw exception");
    }
}
