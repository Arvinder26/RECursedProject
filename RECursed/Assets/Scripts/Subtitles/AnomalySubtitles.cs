using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Subtitles for anomaly lifecycle without showing the anomaly type.
/// Messages can use {room}.
/// </summary>
public class AnomalySubtitles : MonoBehaviour
{
    [Header("Enable / Disable")]
    [SerializeField] bool subtitlesEnabled = true;

    [Header("Messages (use {room})")]
    [SerializeField] string detectedMsg = "Anomaly detected in {room}";
    [SerializeField] string warningMsg  = "WARNING in {room}";
    [SerializeField] string criticalMsg = "CRITICAL in {room}";
    [SerializeField] string expiredMsg  = "Anomaly expired — battery lost";

    [Header("Durations (seconds)")]
    [SerializeField] float detectedSeconds = 1.3f;
    [SerializeField] float warningSeconds  = 1.2f;
    [SerializeField] float criticalSeconds = 1.2f;
    [SerializeField] float expiredSeconds  = 1.6f;

    [Header("Thresholds (seconds remaining)")]
    [SerializeField] float warningThreshold  = 10f;
    [SerializeField] float criticalThreshold = 5f;

    [Header("Formatting")]
    [SerializeField] bool includeRoomName = true;
    [Tooltip("Inserted when includeRoomName = true. {0} is the room name.")]
    [SerializeField] string roomFormat = "{0}";

    [Header("Discovery / Performance")]
    [SerializeField] bool rescanOnInterval = true;
    [SerializeField] float rescanSeconds = 3f;

    [Header("Debug")]
    [SerializeField] bool verbose = false;

    // Internals
    readonly List<MovedObject> moved = new();
    readonly List<DisappearedObject> disappeared = new();
    readonly List<ExtraObject> extra = new();
    readonly List<LightFlickerAnomaly> flicker = new();

    readonly HashSet<MonoBehaviour> prevActive = new();
    readonly HashSet<MonoBehaviour> warnedOnce = new();
    readonly HashSet<MonoBehaviour> criticalOnce = new();

    float nextRescanTime;

    void Start()
    {
        InitialScan();
    }

    void Update()
    {
        if (!subtitlesEnabled || SubtitleUI.Instance == null) return;

        if (rescanOnInterval && Time.time >= nextRescanTime)
        {
            RescanNewOnly();
            nextRescanTime = Time.time + Mathf.Max(0.5f, rescanSeconds);
        }

        var activeNow = new HashSet<MonoBehaviour>();
        var infos = new List<Info>(16);

        CollectActive(moved, infos, activeNow);
        CollectActive(disappeared, infos, activeNow);
        CollectActive(extra, infos, activeNow);
        CollectActive(flicker, infos, activeNow);

        // Detected (new this frame)
        foreach (var info in infos)
            if (!prevActive.Contains(info.anom))
                Show(detectedMsg, detectedSeconds, info.room);

        // Warning / Critical one-offs
        foreach (var info in infos)
        {
            if (info.t <= criticalThreshold)
            {
                if (criticalOnce.Add(info.anom))
                    Show(criticalMsg, criticalSeconds, info.room);
            }
            else if (info.t <= warningThreshold)
            {
                if (warnedOnce.Add(info.anom))
                    Show(warningMsg, warningSeconds, info.room);
            }
        }

        // Expired (was active, now not)
        foreach (var prev in prevActive)
        {
            if (prev != null && !activeNow.Contains(prev))
            {
                var room = GetRoom(prev);
                Show(expiredMsg, expiredSeconds, room);
            }
        }

        // Update sets
        prevActive.Clear();
        foreach (var a in activeNow) prevActive.Add(a);
        warnedOnce.RemoveWhere(a => a == null);
        criticalOnce.RemoveWhere(a => a == null);
    }

    // ---------- helpers ----------

    struct Info { public MonoBehaviour anom; public Room room; public float t; }

    void CollectActive<T>(List<T> list, List<Info> outInfos, HashSet<MonoBehaviour> activeSet) where T : MonoBehaviour
    {
        for (int i = 0; i < list.Count; i++)
        {
            var a = list[i];
            if (!a) continue;

            bool isActive;
            float tRemain;
            Room room;

            if (a is MovedObject m)                { isActive = m.IsActive; tRemain = m.GetTimeRemaining(); room = m.Room; }
            else if (a is DisappearedObject d)     { isActive = d.IsActive; tRemain = d.GetTimeRemaining(); room = d.Room; }
            else if (a is ExtraObject e)           { isActive = e.IsActive; tRemain = e.GetTimeRemaining(); room = e.Room; }
            else if (a is LightFlickerAnomaly f)   { isActive = f.IsActive; tRemain = f.GetTimeRemaining(); room = f.Room; }
            else continue;

            if (isActive && tRemain > 0f)
            {
                outInfos.Add(new Info { anom = a, room = room, t = tRemain });
                activeSet.Add(a);
                if (verbose) Debug.Log($"[AnomalySubtitles] Active: {a.name} ({room}) {tRemain:0.0}s");
            }
        }
    }

    void Show(string template, float seconds, Room room)
    {
        if (!subtitlesEnabled || SubtitleUI.Instance == null) return;

        string msg = BuildMessage(template, room);
        if (verbose) Debug.Log($"[AnomalySubtitles] SUBTITLE: {msg} ({seconds:0.00}s)");
        SubtitleUI.Instance.ShowForSeconds(msg, seconds);
    }

    string BuildMessage(string template, Room room)
    {
        // Build room text
        string roomText = includeRoomName ? string.Format(roomFormat, room.ToString()) : "";

        // If the template omitted {room} but rooms are enabled, append nicely
        if (!template.Contains("{room}"))
            template = includeRoomName ? $"{template} — {{room}}" : template;

        // In case any legacy template still has {type}, strip it out
        template = template.Replace("{type}", "");

        return template.Replace("{room}", roomText);
    }

    Room GetRoom(MonoBehaviour a)
    {
        if (a is MovedObject m)              return m.Room;
        if (a is DisappearedObject d)        return d.Room;
        if (a is ExtraObject e)              return e.Room;
        if (a is LightFlickerAnomaly f)      return f.Room;
        return default;
    }

    void InitialScan()
    {
        moved.Clear();        moved.AddRange(FindObjectsOfType<MovedObject>(true));
        disappeared.Clear();  disappeared.AddRange(FindObjectsOfType<DisappearedObject>(true));
        extra.Clear();        extra.AddRange(FindObjectsOfType<ExtraObject>(true));
        flicker.Clear();      flicker.AddRange(FindObjectsOfType<LightFlickerAnomaly>(true));
        nextRescanTime = Time.time + Mathf.Max(0.5f, rescanSeconds);
        if (verbose) Debug.Log($"[AnomalySubtitles] Scan totals: moved {moved.Count}, disappeared {disappeared.Count}, extra {extra.Count}, flicker {flicker.Count}");
    }

    void RescanNewOnly()
    {
        AddNew(moved);
        AddNew(disappeared);
        AddNew(extra);
        AddNew(flicker);
    }

    void AddNew<T>(List<T> list) where T : MonoBehaviour
    {
        var all = FindObjectsOfType<T>(true);
        foreach (var a in all)
            if (!list.Contains(a)) list.Add(a);
    }
}
