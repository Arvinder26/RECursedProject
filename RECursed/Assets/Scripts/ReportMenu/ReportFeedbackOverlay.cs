using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ReportFeedbackOverlay : MonoBehaviour
{
    [Header("Hook these")]
    [SerializeField] private GameObject root;    
    [SerializeField] private TMP_Text label;     
    [SerializeField] private Image background;   

    [Header("Optional SFX")]
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip showClip;
    [SerializeField] private AudioClip hideClip;

    void Awake()
    {
        if (!root) root = gameObject;
        root.SetActive(false);
    }

    public void Show(string message)
    {
        if (!root) return;
        if (label) label.text = message;
        root.SetActive(true);
        if (sfx && showClip) sfx.PlayOneShot(showClip);
    }

    public void SetText(string message)
    {
        if (label) label.text = message;
    }

    public void Hide()
    {
        if (!root) return;
        root.SetActive(false);
        if (sfx && hideClip) sfx.PlayOneShot(hideClip);
    }
}
