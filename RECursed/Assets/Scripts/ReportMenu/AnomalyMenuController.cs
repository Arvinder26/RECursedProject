using UnityEngine;
using TMPro;

/// <summary>
/// Simple controller for the small “open/close anomaly menu” panel:
/// - Toggles the panel on/off and updates the button label
/// - Shows current selections (room/type) in the tiny header labels
/// - Exposes SelectRoom/SelectType that my buttons call
/// - OnCancel clears the selections and restores header text
/// </summary>
public class AnomalyMenuController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] RectTransform panelRoot;    // root rect for the panel (so I can SetActive on/off)
    [SerializeField] CanvasGroup panelGroup;     // lets me fade/disable raycasts when needed

    [Header("Open/Close button")]
    [SerializeField] UnityEngine.UI.Button openCloseButton; // the button the player clicks to open/close
    [SerializeField] TMP_Text openCloseLabel;               // the text element that says OPEN/CLOSE
    [SerializeField] string openText  = "OPEN ANOMALY MENU";
    [SerializeField] string closeText = "CLOSE ANOMALY MENU";

    [Header("Labels")]
    [SerializeField] TMP_Text roomLabel;   // tiny label that mirrors the selected room (or header)
    [SerializeField] TMP_Text typeLabel;   // tiny label that mirrors the selected type (or header)

    // runtime state
    bool   isOpen;
    string selectedRoom;
    string selectedType;

    // cached defaults for the header text so I can restore after Cancel/Close
    string _roomHeaderDefault;
    string _typeHeaderDefault;

    void Awake()
    {
        // remember what the labels said in the editor (likely the static headers)
        _roomHeaderDefault = roomLabel  ? roomLabel.text  : "";
        _typeHeaderDefault = typeLabel ? typeLabel.text : "";

        HideMenuImmediate();
    }

    /// <summary>Bound to the big button: flips between open and close.</summary>
    public void ToggleOpenClose()
    {
        if (isOpen) CloseMenu();
        else        OpenMenu();
    }

    /// <summary>Show the panel and swap the button label to CLOSE.</summary>
    public void OpenMenu()
    {
        isOpen = true;
        SetPanelVisible(true);
        if (openCloseLabel) openCloseLabel.text = closeText;
        
        // ensure this panel draws above time/battery/etc. (same Canvas only)
        if (panelRoot) panelRoot.SetAsLastSibling();

        // Ensure labels are correct every time we open.
        if (roomLabel) roomLabel.text = string.IsNullOrEmpty(selectedRoom) ? _roomHeaderDefault : selectedRoom;
        if (typeLabel) typeLabel.text = string.IsNullOrEmpty(selectedType) ? _typeHeaderDefault : selectedType;
    }

    /// <summary>Hide the panel and swap the button label back to OPEN.</summary>
    public void CloseMenu()
    {
        isOpen = false;
        SetPanelVisible(false);
        if (openCloseLabel) openCloseLabel.text = openText;
    }

    /// <summary>Hard-hide with no transition; used on Awake to avoid flashes on load.</summary>
    void HideMenuImmediate()
    {
        isOpen = false;
        SetPanelVisible(false, instant: true);
        if (openCloseLabel) openCloseLabel.text = openText;
    }

    /// <summary>
    /// Core show/hide for the panel. I update:
    /// - GameObject active (fast on/off)
    /// - CanvasGroup flags (so it blocks clicks only when visible)
    /// - Alpha (0/1) so the intent is obvious in the editor
    /// </summary>
    void SetPanelVisible(bool show, bool instant = false)
    {
        if (panelRoot) panelRoot.gameObject.SetActive(show);

        if (panelGroup)
        {
            panelGroup.interactable   = show;
            panelGroup.blocksRaycasts = show;
            panelGroup.alpha          = show ? 1f : 0f;
        }
    }

    /// <summary>Called by room buttons. Stores the text and reflects it in the label.</summary>
    public void SelectRoom(string room)
    {
        selectedRoom = room;
        if (roomLabel) roomLabel.text = room;
    }

    /// <summary>Called by anomaly-type buttons. Stores the text and reflects it in the label.</summary>
    public void SelectType(string type)
    {
        selectedType = type;
        if (typeLabel) typeLabel.text = type;
    }

    /// <summary>
    /// Clear both selections and close the mini-panel.
    /// Important: restore the header text instead of blanking the labels.
    /// </summary>
    public void OnCancel()
    {
        selectedRoom = null;
        selectedType = null;

        if (roomLabel)  roomLabel.text  = _roomHeaderDefault;
        if (typeLabel)  typeLabel.text  = _typeHeaderDefault;

        CloseMenu();
    }

    /// <summary>
    /// For this small controller I just log the selection. The actual
    /// report/validation happens in the main ReportMenuController.
    /// </summary>
    public void OnReport()
    {
        if (string.IsNullOrEmpty(selectedRoom) || string.IsNullOrEmpty(selectedType))
        {
            Debug.LogWarning("Pick both a room and an anomaly type before reporting.");
            return;
        }

        Debug.Log($"REPORT sent: Room={selectedRoom}, Type={selectedType}");

        // After sending, clear + close so the next interaction starts fresh.
        OnCancel();
    }
}
