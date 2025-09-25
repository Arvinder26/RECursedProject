using UnityEngine;
using TMPro;

public class ReportFeedbackOverlay : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject root;          // Defaults to this object
    [SerializeField] private TMP_Text textLabel;       // Your OverlayText TMP
    [SerializeField] private CanvasGroup cg;           // Optional (leave empty if you don't use it)

    void Reset()
    {
        root = gameObject;
        if (!textLabel) textLabel = GetComponentInChildren<TMP_Text>(true);
        if (!cg) cg = GetComponent<CanvasGroup>(); // ok if null
        HideImmediate();
    }

    // --- API used by your ReportMenuController ---
    public void Show(string message)
    {
        if (textLabel) textLabel.text = message;

        if (root && !root.activeSelf) root.SetActive(true);

        // If you added a CanvasGroup, we’ll respect it. Otherwise this is ignored.
        if (cg)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }
    }

    public void SetText(string message)
    {
        if (textLabel) textLabel.text = message;
    }

    public void Hide()
    {
        if (cg)
        {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        if (root) root.SetActive(false);
    }

    public void HideImmediate() => Hide();
}
