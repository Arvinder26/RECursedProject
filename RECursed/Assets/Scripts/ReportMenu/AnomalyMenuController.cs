using UnityEngine;
using TMPro;

public class AnomalyMenuController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] RectTransform panelRoot; 
    [SerializeField] CanvasGroup panelGroup;  

    [Header("Open/Close button")]
    [SerializeField] UnityEngine.UI.Button openCloseButton; 
    [SerializeField] TMP_Text openCloseLabel;
    [SerializeField] string openText  = "OPEN ANOMALY MENU";
    [SerializeField] string closeText = "CLOSE ANOMALY MENU";

    
    [SerializeField] TMP_Text roomLabel;
    [SerializeField] TMP_Text typeLabel;

    bool isOpen;
    string selectedRoom;
    string selectedType;

    void Awake()
    {
        HideMenuImmediate();
    }

    public void ToggleOpenClose()
    {
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        isOpen = true;
        SetPanelVisible(true);
        if (openCloseLabel) openCloseLabel.text = closeText;
    }

    public void CloseMenu()
    {
        isOpen = false;
        SetPanelVisible(false);
        if (openCloseLabel) openCloseLabel.text = openText;
    }

    void HideMenuImmediate()
    {
        isOpen = false;
        SetPanelVisible(false, instant:true);
        if (openCloseLabel) openCloseLabel.text = openText;
    }

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

    
    public void SelectRoom(string room)
    {
        selectedRoom = room;
        if (roomLabel) roomLabel.text = room;
    }

    
    public void SelectType(string type)
    {
        selectedType = type;
        if (typeLabel) typeLabel.text = type;
    }

    
    public void OnCancel()
    {
        
        selectedRoom = null;
        selectedType = null;
        if (roomLabel) roomLabel.text = "";
        if (typeLabel) typeLabel.text = "";
        CloseMenu();
    }

    public void OnReport()
    {
        if (string.IsNullOrEmpty(selectedRoom) || string.IsNullOrEmpty(selectedType))
        {
            Debug.LogWarning("Pick both a room and an anomaly type before reporting.");
            return;
        }

        Debug.Log($"REPORT sent: Room={selectedRoom}, Type={selectedType}");
        

        
        OnCancel();
    }
}
