using UnityEngine;
using TMPro;
using System.Collections;

public class ReportFeedbackOverlay : MonoBehaviour
{
    [SerializeField] CanvasGroup group;
    [SerializeField] TMP_Text label;
    [SerializeField] float showSeconds = 1.25f;

    Coroutine hideRoutine;

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        if (!label) label = GetComponentInChildren<TMP_Text>(true);
        // Start hidden but active so Awake runs
        SetVisible(false);
    }

    void SetVisible(bool v)
    {
        group.alpha = v ? 1f : 0f;
        group.interactable = v;       // <- important for keyboard/gamepad
        group.blocksRaycasts = v;     // <- critical: blocks clicks to buttons behind
    }

    public void Show(string message, float? duration = null)
    {
        label.text = message;
        SetVisible(true);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfter(duration ?? showSeconds));
    }

    IEnumerator HideAfter(float t)
    {
        yield return new WaitForSecondsRealtime(t); // works even if you pause time on loss
        SetVisible(false);
    }
}
