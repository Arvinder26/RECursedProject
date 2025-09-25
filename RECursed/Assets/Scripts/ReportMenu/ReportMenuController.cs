using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReportMenuController : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private Transform roomsParent;     // Left column parent
    [SerializeField] private Transform typesParent;     // Right column parent
    [SerializeField] private Button cancelButton;       // Bottom-left
    [SerializeField] private Button reportButton;       // Bottom-right
    [SerializeField] private AnomalyManager anomalyManager;

    [Header("Feedback overlay (optional)")]
    [SerializeField] private CanvasGroup overlay;       // Drag CanvasGroup panel here
    [SerializeField] private float overlaySeconds = 2f; // How long to show
    [SerializeField] private string overlaySuccessText = "ANOMALY REPORTED";
    [SerializeField] private string overlayFailText    = "NO ANOMALY MATCH";

    [Header("Battery / Loss (optional)")]
    [SerializeField] private SegmentBattery battery;    // Drag battery UI (with SegmentBattery)

    [Header("Selection visuals")]
    [SerializeField] private Color normalColor   = new Color(1f, 1f, 1f, 0.65f);
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField, Min(1f)] private float selectedScale = 1.05f;

    
    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;       // 2D UI source
    [SerializeField] private AudioClip reportClickSfx;    // played immediately on click
    [SerializeField] private AudioClip successSfx;        // on correct report
    [SerializeField] private AudioClip failSfx;           // on wrong report
    [SerializeField] private bool resetAfterReport = true; // reset selections even on fail

    // --- internals ---
    private readonly List<Button> _roomButtons = new();
    private readonly List<Button> _typeButtons = new();

    private Button _selectedRoomBtn;
    private Button _selectedTypeBtn;

    private Room? _selectedRoom;
    private AnomalyType? _selectedType;

    private Coroutine _overlayCo;

    
    private Dictionary<string, Room> _roomMap;
    private Dictionary<string, AnomalyType> _typeMap;

    void Awake()
    {
        BuildEnumMaps();

        WireButtons(roomsParent, isRoom: true);
        WireButtons(typesParent, isRoom: false);

        if (cancelButton) cancelButton.onClick.AddListener(Cancel);
        if (reportButton) reportButton.onClick.AddListener(Report);

        ResetButtons(_roomButtons);
        ResetButtons(_typeButtons);

        
        if (overlay)
        {
            overlay.alpha = 0f;
            overlay.blocksRaycasts = false;
            overlay.interactable = false;
        }

        
        if (sfxSource)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f; 
        }
    }

    // ---------- mapping helpers ----------
    void BuildEnumMaps()
    {
        _roomMap = Enum.GetNames(typeof(Room))
            .ToDictionary(Sanitize, n => (Room)Enum.Parse(typeof(Room), n));

        _typeMap = Enum.GetNames(typeof(AnomalyType))
            .ToDictionary(Sanitize, n => (AnomalyType)Enum.Parse(typeof(AnomalyType), n));

        
        _typeMap[Sanitize("Moved Object")]           = AnomalyType.MovedObject;
        _typeMap[Sanitize("Object Disappeared")]     = AnomalyType.ObjectDisappeared;
        _typeMap[Sanitize("Extra Object")]           = AnomalyType.ExtraObject;
        _typeMap[Sanitize("Moved")]                  = AnomalyType.MovedObject;
        _typeMap[Sanitize("Disappeared")]            = AnomalyType.ObjectDisappeared;
        _typeMap[Sanitize("Extra")]                  = AnomalyType.ExtraObject;

        
        _roomMap[Sanitize("Bedroom 1")]              = Room.Bedroom1;
        _roomMap[Sanitize("Walk-in Wardrobe")]       = Room.WalkinWardrobe;
        _roomMap[Sanitize("Walk in Wardrobe")]       = Room.WalkinWardrobe;
    }

    static string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        
        var arr = s.Where(char.IsLetterOrDigit).ToArray();
        return new string(arr).ToUpperInvariant();
    }

    // ---------- UI wiring ----------
    void WireButtons(Transform parent, bool isRoom)
    {
        if (!parent) return;

        foreach (Transform child in parent)
        {
            var b = child.GetComponent<Button>();
            if (!b) continue;

            var label = b.GetComponentInChildren<TMP_Text>(true);
            var text  = label ? label.text : b.name;
            var key   = Sanitize(text);

            if (isRoom)
            {
                if (!_roomMap.TryGetValue(key, out var roomEnum))
                {
                    Debug.LogWarning($"[ReportMenu] Room label '{text}' doesn’t map to Room enum.");
                    continue;
                }
                b.onClick.AddListener(() => OnRoomClicked(b, roomEnum));
                _roomButtons.Add(b);
            }
            else
            {
                if (!_typeMap.TryGetValue(key, out var typeEnum))
                {
                    Debug.LogWarning($"[ReportMenu] Type label '{text}' doesn’t map to AnomalyType enum.");
                    continue;
                }
                b.onClick.AddListener(() => OnTypeClicked(b, typeEnum));
                _typeButtons.Add(b);
            }
        }
    }

    void OnRoomClicked(Button btn, Room value)
    {
        _selectedRoom = value;
        _selectedRoomBtn = btn;
        HighlightExclusive(_roomButtons, _selectedRoomBtn);
    }

    void OnTypeClicked(Button btn, AnomalyType value)
    {
        _selectedType = value;
        _selectedTypeBtn = btn;
        HighlightExclusive(_typeButtons, _selectedTypeBtn);
    }

    void HighlightExclusive(List<Button> list, Button selected)
    {
        foreach (var b in list)
        {
            var g  = b.targetGraphic;
            var rt = b.transform as RectTransform;

            bool isSel = b == selected;
            if (g)  g.color          = isSel ? selectedColor : normalColor;
            if (rt) rt.localScale    = isSel ? Vector3.one * selectedScale : Vector3.one;
        }
    }

    void ResetButtons(List<Button> list)
    {
        foreach (var b in list)
        {
            if (!b) continue;
            if (b.targetGraphic) b.targetGraphic.color = normalColor;
            var rt = b.transform as RectTransform;
            if (rt) rt.localScale = Vector3.one;
        }
    }

    // ---------- Actions ----------
    public void Cancel()
    {
        _selectedRoom = null;
        _selectedType = null;
        _selectedRoomBtn = null;
        _selectedTypeBtn = null;

        ResetButtons(_roomButtons);
        ResetButtons(_typeButtons);
    }

    public void Report()
    {
        
        PlayOneShot(reportClickSfx);

        if (!anomalyManager)
        {
            Debug.LogWarning("[ReportMenu] No AnomalyManager reference.");
            if (resetAfterReport) Cancel();
            return;
        }

        if (!_selectedRoom.HasValue || !_selectedType.HasValue)
        {
            ShowOverlay(overlayFailText);
            PlayOneShot(failSfx);
            if (resetAfterReport) Cancel();
            return;
        }

        bool ok = anomalyManager.ValidateAndResolve(_selectedRoom.Value, _selectedType.Value);

        if (ok)
        {
            ShowOverlay(overlaySuccessText);
            PlayOneShot(successSfx);
        }
        else
        {
            ShowOverlay(overlayFailText);
            PlayOneShot(failSfx);
            if (battery) battery.ApplyPenalty(1);
        }

       
        if (resetAfterReport) Cancel();
    }

    // ---------- Overlay ----------
    private void ShowOverlay(string message)
    {
        if (!overlay) return;

        if (_overlayCo != null) StopCoroutine(_overlayCo);
        _overlayCo = StartCoroutine(OverlayCo(message));
    }

    private IEnumerator OverlayCo(string message)
    {
        
        TMP_Text label = overlay.GetComponentInChildren<TMP_Text>(true);
        if (label) label.text = message;

        overlay.alpha = 1f;
        overlay.blocksRaycasts = true;
        overlay.interactable = true;

        yield return new WaitForSeconds(overlaySeconds);

        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
        overlay.interactable = false;
    }

    
    public void SelectRoomByName(string labelText)
    {
        var key = Sanitize(labelText);
        if (_roomMap.TryGetValue(key, out var room))
        {
            _selectedRoom = room;
            var btn = _roomButtons.FirstOrDefault(b =>
            {
                var t = b.GetComponentInChildren<TMP_Text>(true)?.text ?? b.name;
                return Sanitize(t) == key;
            });
            if (btn) { _selectedRoomBtn = btn; HighlightExclusive(_roomButtons, btn); }
        }
    }

    public void SelectTypeByName(string labelText)
    {
        var key = Sanitize(labelText);
        if (_typeMap.TryGetValue(key, out var type))
        {
            _selectedType = type;
            var btn = _typeButtons.FirstOrDefault(b =>
            {
                var t = b.GetComponentInChildren<TMP_Text>(true)?.text ?? b.name;
                return Sanitize(t) == key;
            });
            if (btn) { _selectedTypeBtn = btn; HighlightExclusive(_typeButtons, btn); }
        }
    }

    // NEW: tiny helper
    private void PlayOneShot(AudioClip clip)
    {
        if (sfxSource && clip) sfxSource.PlayOneShot(clip);
    }
}
