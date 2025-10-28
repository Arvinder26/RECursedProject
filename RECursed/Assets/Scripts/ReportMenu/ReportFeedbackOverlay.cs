using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReportFeedbackOverlay : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private CanvasGroup group;       // CanvasGroup to fade & block raycasts
    [SerializeField] private TMP_Text label;          // Text that displays the overlay message
    [SerializeField] private GameObject root;         // Root object for the overlay (enabled/disabled)
    [SerializeField] private Image background;        // Optional background image (not required)

    [Header("Timing")]
    [SerializeField] private float showSeconds = 1.25f;   // How long the overlay stays visible by default
    private Coroutine hideRoutine;                        // So we can cancel/restart if Show() is called again

    [Header("Optional SFX")]
    [SerializeField] private AudioSource sfx;             // One-shot audio source (optional)
    [SerializeField] private AudioClip showClip;          // Clip to play when the overlay appears
    [SerializeField] private AudioClip hideClip;          // Clip to play when the overlay hides

    private void Awake()
    {
        // Auto-wire references if left empty in the Inspector
        if (!group) group = GetComponent<CanvasGroup>();
        if (!label) label = GetComponentInChildren<TMP_Text>(true);

        // Start hidden but keep this GameObject active so Awake runs already
        SetVisible(false);
        if (root) root.SetActive(false);
    }

    // Central place to toggle visibility + input blocking.
    public void SetVisible(bool v)
    {
        if (group)
        {
            group.alpha = v ? 1f : 0f;
            group.interactable = v;        // keyboard/gamepad focus (nice to have)
            group.blocksRaycasts = v;      // prevents clicking UI or world behind this
        }

        if (root) root.SetActive(v);
    }

    // Show the overlay with a message.
    public void Show(string message, float? duration = null)
    {
        if (label) label.text = message;
        SetVisible(true);

        // (Re)start the hide timer.
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfter(duration ?? showSeconds));

        // Play show SFX (optional)
        if (sfx && showClip) sfx.PlayOneShot(showClip);
    }

    // Update the message text while already visible.
    public void SetText(string message)
    {
        if (label) label.text = message;
    }

    private IEnumerator HideAfter(float t)
    {
        // Waits in real time so overlay still hides even if gameplay is paused.
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, t));

        SetVisible(false);
        if (root) root.SetActive(false);

        // Play disappear SFX.
        if (sfx && hideClip) sfx.PlayOneShot(hideClip);

        hideRoutine = null;
    }
}
