using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class SummaryReportViewing
{
    [Test]
    public void AllCountsZero()
    {
        var summaryManager = new SummaryManager();
        var report = summaryManager.GetAnomalyReportPlaceholder();

        Assert.AreEqual(0, report.ReportedCorrectly);
        Assert.AreEqual(0, report.Missed);
        Assert.AreEqual(0, report.Misreported);
    }

    [Test]
    public void CorrectDisplayCounts()
    {
        var summaryManager = new SummaryManager();
        summaryManager.SetPlaceholderData(("correct", 3), ("missed", 2), ("misreported", 1));

        var report = summaryManager.GetAnomalyReportPlaceholder();

        Assert.AreEqual(3, report.ReportedCorrectly);
        Assert.AreEqual(2, report.Missed);
        Assert.AreEqual(1, report.Misreported);
    }

    [Test]
    public void IgnoreUnknownCategory()
    {
        var summaryManager = new SummaryManager();
        summaryManager.SetPlaceholderData(("correct", 2), ("unknown", 5), ("incorrect", 1));

        var report = summaryManager.GetAnomalyReportPlaceholder();

        Assert.AreEqual(2, report.ReportedCorrectly);
        Assert.AreEqual(0, report.Missed);
        Assert.AreEqual(0, report.Misreported);
    }

    [Test]
    public void NegativeCounts()
    {
        var summaryManager = new SummaryManager();
        summaryManager.SetPlaceholderData(("correct", -3), ("missed", 2));

        var report = summaryManager.GetAnomalyReportPlaceholder();

        Assert.AreEqual(0, report.ReportedCorrectly);
        Assert.AreEqual(2, report.Missed);
        Assert.AreEqual(0, report.Misreported);
    }

    [Test]
    public void MultipleEntries()
    {
        var summaryManager = new SummaryManager();
        summaryManager.SetPlaceholderData(("correct", 1), ("correct", 4), ("missed", 2));

        var report = summaryManager.GetAnomalyReportPlaceholder();

        Assert.AreEqual(5, report.ReportedCorrectly);
        Assert.AreEqual(2, report.Missed);
        Assert.AreEqual(0, report.Misreported);
    }
}
