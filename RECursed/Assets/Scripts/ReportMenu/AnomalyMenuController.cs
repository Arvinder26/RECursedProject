using UnityEngine;
using TMPro;

// Simple controller for the small “open/close anomaly menu” panel:

public class AnomalyMenuController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] RectTransform panelRoot;    // The root panel  rect for the panel. 
    [SerializeField] CanvasGroup panelGroup;     // Controls alpha & raycasts.

    [Header("Open/Close button")]
    [SerializeField] UnityEngine.UI.Button openCloseButton; // The big toggle button
    [SerializeField] TMP_Text openCloseLabel;               // the button label OPEN/CLOSE
    [SerializeField] string openText  = "OPEN ANOMALY MENU";
    [SerializeField] string closeText = "CLOSE ANOMALY MENU";

    [Header("Labels")]
    [SerializeField] TMP_Text roomLabel;   // tiny label that mirrors the selected room (or header)
    [SerializeField] TMP_Text typeLabel;   // tiny label that mirrors the selected type (or header)

    // runtime state
    bool   isOpen;
    string selectedRoom;
    string selectedType;

    // Runtime state of the mini-panel and current selections.
    string _roomHeaderDefault;
    string _typeHeaderDefault;

    void Awake()
    {
        // Capture default header text from the labels set in the editor.
        _roomHeaderDefault = roomLabel  ? roomLabel.text  : "";
        _typeHeaderDefault = typeLabel ? typeLabel.text : "";

        HideMenuImmediate(); // No flash on scene load
    }

    /// <summary>Bound to the big button: flips between open and close.</summary>
    public void ToggleOpenClose()
    {
        if (isOpen) CloseMenu();
        else        OpenMenu();
    }

    // Show the panel and swap the button label to CLOSE.
    public void OpenMenu()
    {
        isOpen = true;
        SetPanelVisible(true);
        if (openCloseLabel) openCloseLabel.text = closeText;

        // Ensure labels are correct every time we open.
        if (roomLabel) roomLabel.text  = string.IsNullOrEmpty(selectedRoom) ? _roomHeaderDefault : selectedRoom;
        if (typeLabel) typeLabel.text = string.IsNullOrEmpty(selectedType) ? _typeHeaderDefault : selectedType;
    }

    // Hide the panel and swap the button label back to OPEN.
    public void CloseMenu()
    {
        isOpen = false;
        SetPanelVisible(false);
        if (openCloseLabel) openCloseLabel.text = openText;
    }

    // Hard-hide with no transition, used on Awake to avoid flashes.
    void HideMenuImmediate()
    {
        isOpen = false;
        SetPanelVisible(false, instant: true);
        if (openCloseLabel) openCloseLabel.text = openText;
    }

    // Core show/hide logic. Keeps GameObject active state
    void SetPanelVisible(bool show, bool instant = false)
    {
        if (panelRoot) panelRoot.gameObject.SetActive(show);

        if (panelGroup)
        {
            panelGroup.interactable   = show;  // enable focus / nav when visible
            panelGroup.blocksRaycasts = show;  // block clicks behind the panel
            panelGroup.alpha          = show ? 1f : 0f;
        }
    }

    // Called by a room button (via AnomalyChoiceButton).
    public void SelectRoom(string room)
    {
        selectedRoom = room;
        if (roomLabel) roomLabel.text = room;
    }

    // Called by a type button (via AnomalyChoiceButton)
    public void SelectType(string type)
    {
        selectedType = type;
        if (typeLabel) typeLabel.text = type;
    }

    // Clear both selections and close the mini-panel.
    public void OnCancel()
    {
        selectedRoom = null;
        selectedType = null;

        if (roomLabel)  roomLabel.text  = _roomHeaderDefault;
        if (typeLabel)  typeLabel.text  = _typeHeaderDefault;

        CloseMenu();
    }

   
    // Clear both selections and close the panel.
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
