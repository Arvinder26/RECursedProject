using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// My anomaly report UI controller:
/// - builds room/type button lists
/// - handles selection highlights
/// - runs the "Report" flow (talks to AnomalyManager)
/// - shows a modal overlay with text + optional SFX
/// - charges battery on wrong report and triggers loss when empty
/// - temporarily disables other UI (e.g., Close/Open menu button) while overlay is up
/// </summary>
public class ReportMenuController : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private Transform roomsParent;          // left column (buttons in enum order)
    [SerializeField] private Transform typesParent;          // right column (buttons in enum order)
    [SerializeField] private Button cancelButton;            // "Cancel" in the panel
    [SerializeField] private Button reportButton;            // "Report" in the panel
    [SerializeField] private Button closeMenuButton;         // "Close Anomaly Menu" (outside the panel)
    [SerializeField] private AnomalyManager anomalyManager;  // central logic that validates & resolves anomalies

    [Header("Selection visuals")]
    [SerializeField] private Color normalColor = new Color(1, 1, 1, 0.65f); // dim look for unselected buttons
    [SerializeField] private Color selectedColor = Color.white;             // pop color for selected button
    [SerializeField, Min(1f)] private float selectedScale = 1.05f;          // tiny scale bump for selection

    [Header("Feedback overlay")]
    [SerializeField] private CanvasGroup overlay;        // CanvasGroup on my ReportOverlay object
    [SerializeField] private TMP_Text overlayLabel;      // the TMP text child that shows the message
    [SerializeField, Min(0f)] private float overlaySeconds = 2f; // how long the overlay stays up
    [SerializeField] private string overlaySuccessText = "ANOMALY REPORTED";
    [SerializeField] private string overlayFailText    = "NO ANOMALY MATCH";

    [Header("Overlay SFX")]
    [SerializeField] private AudioSource sfxSource;          // UI AudioSource (2D) for one-shots
    [SerializeField] private AudioClip overlaySuccessSfx;    // plays on correct report
    [SerializeField] private AudioClip overlayFailSfx;       // plays on wrong report
    [SerializeField, Range(0f, 1f)] private float overlaySfxVolume = 1f;

    [Header("Battery / Loss")]
    [SerializeField] private SegmentBattery battery;     // my segmented battery (optional)
    [SerializeField, Min(1)] private int wrongReportCost = 1; // bars to spend on a wrong report
    [SerializeField] private LossScreen lossScreen;      // loss screen to show when battery hits 0 (optional but nice)

    // ---- runtime state ----
    private readonly List<Button> _roomButtons = new();   // cached room buttons (for quick highlight resets)
    private readonly List<Button> _typeButtons = new();   // cached type buttons
    private int _selectedRoom  = -1;                      // current room selection index (-1 = none)
    private int _selectedType  = -1;                      // current type selection index (-1 = none)
    private Coroutine _overlayCo;                        // running overlay hide coroutine (so I can cancel/restart)

    void Awake()
    {
        // Build selectable button lists and hook up click handlers.
        BuildButtons(roomsParent, _roomButtons, OnRoomClicked);
        BuildButtons(typesParent, _typeButtons, OnTypeClicked);

        // Hook my panel buttons.
        if (cancelButton)  cancelButton.onClick.AddListener(Cancel);
        if (reportButton)  reportButton.onClick.AddListener(Report);

        // Make sure visuals start in the "not selected" state.
        ResetButtonVisuals(_roomButtons);
        ResetButtonVisuals(_typeButtons);

        // Initialize overlay to hidden + non-interactive (but keep the object around).
        if (overlay)
        {
            if (!overlayLabel) overlayLabel = overlay.GetComponentInChildren<TMP_Text>(true);
            overlay.alpha = 0f;
            overlay.interactable   = false;
            overlay.blocksRaycasts = false;
            overlay.gameObject.SetActive(false);
        }
    }

    // Build a list of buttons from a parent and register a callback that receives the index I clicked.
    private void BuildButtons(Transform parent, List<Button> list, System.Action<int> onClick)
    {
        list.Clear();
        if (!parent) return;

        for (int i = 0; i < parent.childCount; i++)
        {
            var b = parent.GetChild(i).GetComponent<Button>();
            if (!b) continue;

            int idx = i; // capture
            b.onClick.AddListener(() => onClick(idx));
            list.Add(b);
        }
    }

    // Selection handlers just store the index and update the highlight visuals.
    private void OnRoomClicked(int idx) { _selectedRoom = idx; SetHighlight(_roomButtons, idx); }
    private void OnTypeClicked(int idx) { _selectedType = idx; SetHighlight(_typeButtons, idx); }

    // Quick highlight utility: color + tiny scale bump for the selected button.
    private void SetHighlight(List<Button> list, int index)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var g = list[i].targetGraphic;
            if (g) g.color = (i == index) ? selectedColor : normalColor;

            var rt = list[i].transform as RectTransform;
            if (rt) rt.localScale = (i == index) ? Vector3.one * selectedScale : Vector3.one;
        }
    }

    // Reset all buttons in a given list back to "normal" look.
    private void ResetButtonVisuals(List<Button> list)
    {
        foreach (var b in list)
        {
            if (b.targetGraphic) b.targetGraphic.color = normalColor;
            var rt = b.transform as RectTransform;
            if (rt) rt.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Clear both selections and put the visuals back to default.
    /// I call this after every report (success or fail).
    /// </summary>
    public void Cancel()
    {
        _selectedRoom = -1;
        _selectedType = -1;
        ResetButtonVisuals(_roomButtons);
        ResetButtonVisuals(_typeButtons);
    }

    // ------------------- Report flow -------------------
    public void Report()
    {
        // If the battery is already empty, ignore the report. For safety,
        // I also show the loss screen here in case the event wiring missed it.
        if (battery && battery.Current <= 0)
        {
            if (lossScreen) lossScreen.Show();
            return;
        }

        // Basic guardrails.
        if (!anomalyManager) return;
        if (_selectedRoom < 0 || _selectedType < 0) return;

        // Convert selection indices into the enums the manager expects.
        var room = (Room)_selectedRoom;
        var type = (AnomalyType)_selectedType;

        // Ask the manager if we got it right and let it resolve if so.
        bool ok = anomalyManager.ValidateAndResolve(room, type);

        if (ok)
        {
            // success → show the positive overlay
            ShowOverlay(overlaySuccessText, true);
        }
        else
        {
            // fail → show the negative overlay + spend battery
            ShowOverlay(overlayFailText, false);
            if (battery) battery.Consume(wrongReportCost);

            // If that was our last bar, trigger loss right away (extra safety).
            if (battery && battery.Current <= 0 && lossScreen) lossScreen.Show();
        }

        // After any report, clear the selections so the player starts fresh.
        Cancel();
    }

    // ------------------- Overlay -------------------
    // Pops the overlay text, plays the right SFX, and makes it modal for a few seconds.
    private void ShowOverlay(string text, bool success)
    {
        if (!overlay) return;

        // Make sure the overlay is drawn above everything else.
        overlay.transform.SetAsLastSibling();

        // If a previous overlay is still counting down, stop it and restart.
        if (_overlayCo != null) StopCoroutine(_overlayCo);

        // Update the label if I have one.
        if (overlayLabel) overlayLabel.text = text;

        // Play the "ding" for success/fail (if I hooked up clips + source).
        var clip = success ? overlaySuccessSfx : overlayFailSfx;
        if (sfxSource && clip) sfxSource.PlayOneShot(clip, overlaySfxVolume);

        // Lock inputs while the overlay is shown (prevents clicking through).
        SetButtonsInteractable(false);

        // Actually show the overlay and make it catch raycasts.
        overlay.gameObject.SetActive(true);
        overlay.alpha = 1f;
        overlay.interactable   = true;
        overlay.blocksRaycasts = true;

        // Start a timer to hide it again.
        _overlayCo = StartCoroutine(OverlayRoutine());
    }

    // Wait a real-time delay (works even if I pause time on loss), then hide and re-enable buttons.
    private IEnumerator OverlayRoutine()
    {
        yield return new WaitForSecondsRealtime(overlaySeconds);

        overlay.alpha = 0f;
        overlay.interactable   = false;
        overlay.blocksRaycasts = false;
        overlay.gameObject.SetActive(false);

        SetButtonsInteractable(true);
        _overlayCo = null;
    }

    // Toggle the interactivity of the few buttons I know about while overlay is up.
    private void SetButtonsInteractable(bool v)
    {
        if (reportButton)    reportButton.interactable = v;
        if (cancelButton)    cancelButton.interactable = v;
        if (closeMenuButton) closeMenuButton.interactable = v;
    }
}
