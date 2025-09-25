using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Tiny helper for a feedback overlay: shows a line of text for a short time,
/// blocks clicks behind it while visible, then hides itself. I keep this active
/// in the scene so Awake runs and the CanvasGroup is ready.
/// </summary>
public class ReportFeedbackOverlay : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] CanvasGroup group;  // CanvasGroup I fade + use to block raycasts
    [SerializeField] TMP_Text label;     // the TMP text element inside the overlay

    [Header("Timing")]
    [SerializeField] float showSeconds = 1.25f; // how long to keep it visible

    Coroutine hideRoutine; // so I can cancel/restart if Show() is called again fast

    void Awake()
    {
        // Auto-wire references if I forgot in the inspector.
        if (!group) group = GetComponent<CanvasGroup>();
        if (!label) label = GetComponentInChildren<TMP_Text>(true);

        // Start hidden but keep the GameObject active so this Awake ran already.
        SetVisible(false);
    }

    /// <summary>
    /// Central place to toggle visibility + input blocking.
    /// </summary>
    void SetVisible(bool v)
    {
        group.alpha = v ? 1f : 0f;
        group.interactable = v;       // keyboard/gamepad focus (not essential but nice)
        group.blocksRaycasts = v;     // critical: prevents clicking the UI behind this
    }

    /// <summary>
    /// Pop the overlay with a message for a given duration (or my default).
    /// </summary>
    public void Show(string message, float? duration = null)
    {
        label.text = message;
        SetVisible(true);

        // If a previous timer was still running, stop it and restart.
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfter(duration ?? showSeconds));
    }

    /// <summary>
    /// Wait a realtime delay (so it still hides even if Time.timeScale == 0),
    /// then make the overlay go away and release input.
    /// </summary>
    IEnumerator HideAfter(float t)
    {
        yield return new WaitForSecondsRealtime(t);
        SetVisible(false);
    }
}
