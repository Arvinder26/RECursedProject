using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReportMenuController : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private Transform roomsParent;     // LeftColumn container
    [SerializeField] private Transform typesParent;     // RightColumn container
    [SerializeField] private Button cancelButton;       // Bottom left
    [SerializeField] private Button reportButton;       // Bottom right
    [SerializeField] private AnomalyManager anomalyManager; // auto-found if left empty

    [Header("Feedback overlay (optional)")]
    [SerializeField] private ReportFeedbackOverlay overlay;
    [SerializeField] private float overlaySeconds = 2f;
    [SerializeField] private string overlayText = "ANOMALY REPORTED";

    [Header("Selection visuals")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.65f);
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private float selectedScale = 1.05f;

    // runtime state
    private readonly List<Button> _roomButtons = new List<Button>();
    private readonly List<Button> _typeButtons = new List<Button>();
    private readonly Dictionary<Button, Room> _roomMap = new Dictionary<Button, Room>();
    private readonly Dictionary<Button, AnomalyType> _typeMap = new Dictionary<Button, AnomalyType>();

    private Room? _selectedRoom = null;
    private AnomalyType? _selectedType = null;

    void Awake()
    {
        
#if UNITY_2023_1_OR_NEWER
        if (!anomalyManager) anomalyManager = FindFirstObjectByType<AnomalyManager>(FindObjectsInactive.Include);
#else
        if (!anomalyManager) anomalyManager = FindObjectOfType<AnomalyManager>(true);
#endif

        BuildRoomButtons();
        BuildTypeButtons();

        if (cancelButton) cancelButton.onClick.AddListener(Cancel);
        if (reportButton) reportButton.onClick.AddListener(Report);

        ResetVisuals(_roomButtons);
        ResetVisuals(_typeButtons);
    }

    // ---------- Build UI lists (now searches all descendants) ----------

    void BuildRoomButtons()
    {
        _roomButtons.Clear();
        _roomMap.Clear();
        if (!roomsParent) return;

        foreach (var btn in roomsParent.GetComponentsInChildren<Button>(true))
        {
            btn.onClick.RemoveAllListeners();

            var label = btn.GetComponentInChildren<TMP_Text>(true)?.text ?? "";
            if (TryParseEnum(label, out Room room))
            {
                _roomButtons.Add(btn);
                _roomMap[btn] = room;
                btn.onClick.AddListener(() => OnRoomClicked(btn));
            }
            else
            {
                Debug.LogWarning($"[ReportMenu] Could not map room label '{label}' to Room enum.");
            }
        }
    }

    void BuildTypeButtons()
    {
        _typeButtons.Clear();
        _typeMap.Clear();
        if (!typesParent) return;

        foreach (var btn in typesParent.GetComponentsInChildren<Button>(true))
        {
            btn.onClick.RemoveAllListeners();

            var label = btn.GetComponentInChildren<TMP_Text>(true)?.text ?? "";
            if (TryParseEnum(label, out AnomalyType type))
            {
                _typeButtons.Add(btn);
                _typeMap[btn] = type;
                btn.onClick.AddListener(() => OnTypeClicked(btn));
            }
            else
            {
                Debug.LogWarning($"[ReportMenu] Could not map type label '{label}' to AnomalyType enum.");
            }
        }
    }

    // ---------- Click handlers ----------

    void OnRoomClicked(Button btn)
    {
        if (!_roomMap.TryGetValue(btn, out var room)) return;
        _selectedRoom = room;
        HighlightExclusive(_roomButtons, btn);
    }

    void OnTypeClicked(Button btn)
    {
        if (!_typeMap.TryGetValue(btn, out var type)) return;
        _selectedType = type;
        HighlightExclusive(_typeButtons, btn);
    }

    // ---------- Report / Cancel ----------

    public void Cancel()
    {
        _selectedRoom = null;
        _selectedType = null;

        ResetVisuals(_roomButtons);
        ResetVisuals(_typeButtons);
    }

    public void Report()
    {
        if (!anomalyManager)
        {
            Debug.LogWarning("[ReportMenu] No AnomalyManager assigned or found.");
            return;
        }
        if (!_selectedRoom.HasValue || !_selectedType.HasValue)
        {
            Debug.Log("[ReportMenu] Select a room and an anomaly type first.");
            return;
        }

        StartCoroutine(ResolveWithOverlay(_selectedRoom.Value, _selectedType.Value));
    }

    private IEnumerator ResolveWithOverlay(Room room, AnomalyType type)
    {
        if (overlay) overlay.Show(overlayText);

        
        yield return null;

        bool ok = anomalyManager.ValidateAndResolve(room, type);

        if (overlay) overlay.SetText(ok ? "REPORT ACCEPTED" : "NO ANOMALY FOUND");

        
        float hold = Mathf.Max(0.1f, overlaySeconds);
        float end = Time.realtimeSinceStartup + hold;
        while (Time.realtimeSinceStartup < end)
            yield return null;

        if (overlay) overlay.Hide();

        if (ok) Cancel();
    }

    // ---------- Visual helpers ----------

    void HighlightExclusive(List<Button> list, Button selected)
    {
        foreach (var b in list)
        {
            bool isSel = (b == selected);

            
            var g = b.targetGraphic;
            if (!g)
            {
                var tmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp) g = tmp; 
            }
            if (g) g.color = isSel ? selectedColor : normalColor;

            b.transform.localScale = isSel ? Vector3.one * selectedScale : Vector3.one;
        }
    }

    void ResetVisuals(List<Button> list)
    {
        foreach (var b in list)
        {
            var g = b.targetGraphic;
            if (!g)
            {
                var tmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp) g = tmp;
            }
            if (g) g.color = normalColor;

            b.transform.localScale = Vector3.one;
        }
    }

    

    static bool TryParseEnum<TEnum>(string label, out TEnum value) where TEnum : struct
    {
        string key = (label ?? "").Trim()
                     .Replace(" ", "")
                     .Replace("-", "")
                     .Replace("'", "")
                     .Replace("&", "And")
                     .Replace("(", "")
                     .Replace(")", "");

        return System.Enum.TryParse(key, true, out value);
    }
}
