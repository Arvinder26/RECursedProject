using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReportMenuController : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private Transform roomsParent;     // Left column (buttons in enum order)
    [SerializeField] private Transform typesParent;     // Right column
    [SerializeField] private Button cancelButton;       // "Cancel" in the panel
    [SerializeField] private Button reportButton;       // "Report" in the panel
    [SerializeField] private Button closeMenuButton;    // "Close Anomaly Menu" (optional)
    [SerializeField] private AnomalyManager anomalyManager;

    [Header("Selection visuals")]
    [SerializeField] private Color normalColor = new Color(1, 1, 1, 0.65f);
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField, Min(1f)] private float selectedScale = 1.05f;

    [Header("Feedback overlay")]
    [SerializeField] private CanvasGroup overlay;     // CanvasGroup on your ReportOverlay object
    [SerializeField] private TMP_Text overlayLabel;   // TMP child of the overlay
    [SerializeField, Min(0f)] private float overlaySeconds = 2f;
    [SerializeField] private string overlaySuccessText = "ANOMALY REPORTED";
    [SerializeField] private string overlayFailText = "NO ANOMALY MATCH";

    [Header("Overlay SFX")]
    [SerializeField] private AudioSource sfxSource;       // <- drag an AudioSource (UI) here
    [SerializeField] private AudioClip overlaySuccessSfx; // <- clip for success
    [SerializeField] private AudioClip overlayFailSfx;    // <- clip for fail
    [SerializeField, Range(0f, 1f)] private float overlaySfxVolume = 1f;

    [Header("Battery / Loss")]
    [SerializeField] private SegmentBattery battery; // your BatteryUI (SegmentBattery)
    [SerializeField, Min(1)] private int wrongReportCost = 1;
    [SerializeField] private LossScreen lossScreen;   // drag your LossScreen here (optional but recommended)

    // runtime
    private readonly List<Button> _roomButtons = new();
    private readonly List<Button> _typeButtons = new();
    private int _selectedRoom = -1;
    private int _selectedType = -1;
    private Coroutine _overlayCo;

    void Awake()
    {
        BuildButtons(roomsParent, _roomButtons, OnRoomClicked);
        BuildButtons(typesParent, _typeButtons, OnTypeClicked);

        if (cancelButton)  cancelButton.onClick.AddListener(Cancel);
        if (reportButton)  reportButton.onClick.AddListener(Report);

        ResetButtonVisuals(_roomButtons);
        ResetButtonVisuals(_typeButtons);

        if (overlay)
        {
            if (!overlayLabel) overlayLabel = overlay.GetComponentInChildren<TMP_Text>(true);
            overlay.alpha = 0f;
            overlay.interactable   = false;
            overlay.blocksRaycasts = false;
            overlay.gameObject.SetActive(false);
        }
    }

    // ---------- UI building / selection ----------
    private void BuildButtons(Transform parent, List<Button> list, System.Action<int> onClick)
    {
        list.Clear();
        if (!parent) return;
        for (int i = 0; i < parent.childCount; i++)
        {
            var b = parent.GetChild(i).GetComponent<Button>();
            if (!b) continue;
            int idx = i;
            b.onClick.AddListener(() => onClick(idx));
            list.Add(b);
        }
    }

    private void OnRoomClicked(int idx) { _selectedRoom = idx; SetHighlight(_roomButtons, idx); }
    private void OnTypeClicked(int idx) { _selectedType = idx; SetHighlight(_typeButtons, idx); }

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

    private void ResetButtonVisuals(List<Button> list)
    {
        foreach (var b in list)
        {
            if (b.targetGraphic) b.targetGraphic.color = normalColor;
            var rt = b.transform as RectTransform;
            if (rt) rt.localScale = Vector3.one;
        }
    }

    public void Cancel()
    {
        _selectedRoom = -1;
        _selectedType = -1;
        ResetButtonVisuals(_roomButtons);
        ResetButtonVisuals(_typeButtons);
    }

    // ---------- Report flow ----------
    public void Report()
    {
        // Ignore reports if battery is dead; also show loss if wired
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
        }
        else
        {
            ShowOverlay(overlayFailText, false);
            if (battery) battery.Consume(wrongReportCost);
            if (battery && battery.Current <= 0 && lossScreen) lossScreen.Show();
        }

        // Always clear selections after any report (success or fail)
        Cancel();
    }

    // ---------- Overlay ----------
    private void ShowOverlay(string text, bool success)
    {
        if (!overlay) return;

        overlay.transform.SetAsLastSibling(); // draw above other UI

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

    private void SetButtonsInteractable(bool v)
    {
        if (reportButton)    reportButton.interactable = v;
        if (cancelButton)    cancelButton.interactable = v;
        if (closeMenuButton) closeMenuButton.interactable = v;
    }
}
