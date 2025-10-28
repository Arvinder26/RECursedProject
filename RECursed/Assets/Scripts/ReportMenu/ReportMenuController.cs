using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReportMenuController : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private Transform roomsParent;     // Left column
    [SerializeField] private Transform typesParent;     // Right column
    [SerializeField] private Button cancelButton;       // "Cancel" in the panel
    [SerializeField] private Button reportButton;       // "Report" in the panel
    [SerializeField] private Button closeMenuButton;    // "Close Anomaly Menu"
    [SerializeField] private AnomalyManager anomalyManager;
    [SerializeField] private SummaryReportManager summaryManager;


    [Header("Selection visuals")]
    [SerializeField] private Color normalColor = new Color(1, 1, 1, 0.65f);
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField, Min(1f)] private float selectedScale = 1.05f;

    [Header("Feedback overlay")]
    [SerializeField] private CanvasGroup overlay;     // CanvasGroup on the ReportOverlay object.
    [SerializeField] private TMP_Text overlayLabel;   // TMP child of the overlay.
    [SerializeField, Min(0f)] private float overlaySeconds = 2f;
    [SerializeField] private string overlaySuccessText = "ANOMALY REPORTED";
    [SerializeField] private string overlayFailText = "NO ANOMALY MATCH";

    [Header("Overlay SFX")]
    [SerializeField] private AudioSource sfxSource;       
    [SerializeField] private AudioClip overlaySuccessSfx; 
    [SerializeField] private AudioClip overlayFailSfx;    
    [SerializeField, Range(0f, 1f)] private float overlaySfxVolume = 1f;

    [Header("Battery / Loss")]
    [SerializeField] private SegmentBattery battery; 
    [SerializeField, Min(1)] private int wrongReportCost = 1;
    [SerializeField] private LossScreen lossScreen;   

    // Internals: the two flat lists of buttons and the selected indices (-1 = none).
    private readonly List<Button> _roomButtons = new();
    private readonly List<Button> _typeButtons = new();
    private int _selectedRoom = -1;
    private int _selectedType = -1;
    private Coroutine _overlayCo;

    void Awake()
    {
	// Build click handlers from each column's children.
        BuildButtons(roomsParent, _roomButtons, OnRoomClicked);
        BuildButtons(typesParent, _typeButtons, OnTypeClicked);

	// Wire top-level actions.
        if (cancelButton)  cancelButton.onClick.AddListener(Cancel);
        if (reportButton)  reportButton.onClick.AddListener(Report);

	// Ensure a clean visual state at start.
        ResetButtonVisuals(_roomButtons);
        ResetButtonVisuals(_typeButtons);

	// Prepare the overlay if present.
        if (overlay)
        {
            if (!overlayLabel) overlayLabel = overlay.GetComponentInChildren<TMP_Text>(true);
            overlay.alpha = 0f;
            overlay.interactable   = false;
            overlay.blocksRaycasts = false;
            overlay.gameObject.SetActive(false);
        }
    }

    // UI building / selection.
    private void BuildButtons(Transform parent, List<Button> list, System.Action<int> onClick)
    {
        list.Clear();
        if (!parent) return;
        for (int i = 0; i < parent.childCount; i++)
        {
            var b = parent.GetChild(i).GetComponent<Button>();
            if (!b) continue;
            int idx = i; // Capture for closure
            b.onClick.AddListener(() => onClick(idx));
            list.Add(b);
        }
    }

    private void OnRoomClicked(int idx) { _selectedRoom = idx; SetHighlight(_roomButtons, idx); }
    private void OnTypeClicked(int idx) { _selectedType = idx; SetHighlight(_typeButtons, idx); }

    // Apply selection color + scale to the chosen index and reset others.
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

    // Reset a column back to its “unselected” visuals.
    private void ResetButtonVisuals(List<Button> list)
    {
        foreach (var b in list)
        {
            if (b.targetGraphic) b.targetGraphic.color = normalColor;
            var rt = b.transform as RectTransform;
            if (rt) rt.localScale = Vector3.one;
        }
    }

    // Clear both selections and restore default visuals.
    public void Cancel()
    {
        _selectedRoom = -1;
        _selectedType = -1;
        ResetButtonVisuals(_roomButtons);
        ResetButtonVisuals(_typeButtons);
    }

    // Report flow.
    public void Report()
    {
        // Ignore reports if battery is dead, also show loss if wired
        if (battery && battery.Current <= 0)
        {
            if (lossScreen) lossScreen.Show();
            return;
        }

        if (!anomalyManager) return;
        if (_selectedRoom < 0 || _selectedType < 0) return;

        var room = (Room)_selectedRoom;
        var type = (AnomalyType)_selectedType;

        bool ok = anomalyManager.ValidateAndResolve(room, type);

        if (ok)
        {
            ShowOverlay(overlaySuccessText, true);

            if (summaryManager)
                summaryManager.ShowSuccess();

        }
        else
        {
            ShowOverlay(overlayFailText, false);

            if (summaryManager)
                summaryManager.ShowMisreport();


            if (battery) battery.Consume(wrongReportCost);
            if (battery && battery.Current <= 0 && lossScreen) lossScreen.Show();
        }

        // Always clear selections after any report (success or fail)
        Cancel();
    }

    // Overlay.
    // Pops the overlay to the front, locks UI briefly, and auto-hides after a delay.
    private void ShowOverlay(string text, bool success)
    {
        if (!overlay) return;

        overlay.transform.SetAsLastSibling(); // Render above other UI

        if (_overlayCo != null) StopCoroutine(_overlayCo);
        if (overlayLabel) overlayLabel.text = text;

        // play SFX
        var clip = success ? overlaySuccessSfx : overlayFailSfx;
        if (sfxSource && clip) sfxSource.PlayOneShot(clip, overlaySfxVolume);

        // Lock inputs while overlay is shown
        SetButtonsInteractable(false);

        overlay.gameObject.SetActive(true);
        overlay.alpha = 1f;
        overlay.interactable   = true;
        overlay.blocksRaycasts = true;

        _overlayCo = StartCoroutine(OverlayRoutine());
    }

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

    // Enable/disable the key buttons as a group.
    private void SetButtonsInteractable(bool v)
    {
        if (reportButton)    reportButton.interactable = v;
        if (cancelButton)    cancelButton.interactable = v;
        if (closeMenuButton) closeMenuButton.interactable = v;
    }
}
