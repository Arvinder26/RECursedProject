using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows subtitles when anomalies start, cross warning/critical thresholds,
/// and when they expire (battery loss / unresolved).
/// Attach this to the same GameObject as AnomalyTimerUI (or anywhere in scene).
/// Requires the global SubtitleUI in the scene.
/// </summary>
public class AnomalySubtitles : MonoBehaviour
{
    [Header("Enable / Disable")]
    public bool subtitlesEnabled = true;

    [Header("Messages")]
    [Tooltip("Shown when an anomaly first becomes active.")]
    public string detectedMsg = "[Anomaly detected{room}]";
    [Tooltip("Shown once when time remaining <= warningThreshold.")]
    public string warningMsg  = "[Anomaly warning{room}]";
    [Tooltip("Shown once when time remaining <= criticalThreshold.")]
    public string criticalMsg = "[ANOMALY CRITICAL{room}]";
    [Tooltip("Shown when an active anomaly disappears (battery lost or resolved).")]
    public string expiredMsg  = "[Battery lost — anomaly unresolved]";

    [Header("Durations (seconds)")]
    public float detectedSeconds = 1.3f;
    public float warningSeconds  = 1.2f;
    public float criticalSeconds = 1.2f;
    public float expiredSeconds  = 1.6f;

    [Header("Thresholds (seconds remaining)")]
    public float warningThreshold  = 10f;
    public float criticalThreshold = 5f;

    [Header("Formatting")]
    public bool includeRoomName = true;
    public string roomFormat = " — {0}";

    [Header("Discovery / Performance")]
    [Tooltip("Re-scan scene to catch newly spawned anomalies.")]
    public bool rescanOnInterval = true;
    [Tooltip("Seconds between rescans for new anomaly components.")]
    public float rescanSeconds = 3f;

    [Header("Debug")]
    public bool verbose = false;

    // ---- internal state ----
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
        {
            if (!prevActive.Contains(info.anom))
                Show(detectedMsg, detectedSeconds, info.room);
        }

        // Warning / Critical (one-offs per anomaly)
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
                Show(expiredMsg, expiredSeconds, null, isExpired:true);
        }

        prevActive.Clear();
        foreach (var a in activeNow) prevActive.Add(a);

        // Clean sets (avoid memory growth if anomalies are destroyed)
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

            // All anomaly classes expose IsActive + GetTimeRemaining + Room
            if (a is MovedObject m)          { isActive = m.IsActive;          tRemain = m.GetTimeRemaining(); }
            else if (a is DisappearedObject d){ isActive = d.IsActive;          tRemain = d.GetTimeRemaining(); }
            else if (a is ExtraObject e)     { isActive = e.IsActive;          tRemain = e.GetTimeRemaining(); }
            else if (a is LightFlickerAnomaly f){ isActive = f.IsActive;        tRemain = f.GetTimeRemaining(); }
            else continue;

            if (isActive && tRemain > 0f)
            {
                outInfos.Add(new Info { anom = a, room = GetRoom(a), t = tRemain });
                activeSet.Add(a);
                if (verbose) Debug.Log($"[AnomalySubtitles] Active: {a.name} ({GetRoom(a)}) {tRemain:0.0}s");
            }
        }
    }

    Room GetRoom(MonoBehaviour a)
    {
        if (a is MovedObject m) return m.Room;
        if (a is DisappearedObject d) return d.Room;
        if (a is ExtraObject e) return e.Room;
        if (a is LightFlickerAnomaly f) return f.Room;
        return default;
    }

    void Show(string template, float seconds, Room? room, bool isExpired = false)
    {
        if (!subtitlesEnabled || SubtitleUI.Instance == null) return;

        string roomSuffix = (includeRoomName && room.HasValue)
            ? string.Format(roomFormat, room.Value.ToString())
            : string.Empty;

        string msg = template.Replace("{room}", roomSuffix);

        // Strong preference: anomalies pre-empt footstep captions.
        // FootstepSubtitles already yields when another message is showing.

        if (verbose) Debug.Log($"[AnomalySubtitles] SUBTITLE: {msg} ({seconds:0.00}s)");
        SubtitleUI.Instance.ShowForSeconds(msg, seconds);
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
        // pick up any new instances since Start
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
