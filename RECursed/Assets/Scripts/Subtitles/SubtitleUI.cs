using System.Collections;
using UnityEngine;
using TMPro;

public class SubtitleUI : MonoBehaviour
{
    public static SubtitleUI Instance { get; private set; }
    public TMP_Text text;
    public CanvasGroup group;
    public float fadeSeconds = 0.15f;

    Coroutine running;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (!group) group = GetComponent<CanvasGroup>();
        if (group) group.alpha = 0f;
    }

    public void Show(string msg) {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ShowRoutine(msg, -1f));
    }

    public void ShowForSeconds(string msg, float seconds) {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ShowRoutine(msg, seconds));
    }

    public void Hide() {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeTo(0f));
    }

    IEnumerator ShowRoutine(string msg, float seconds) {
        text.text = msg;
        yield return FadeTo(1f);
        if (seconds > 0f) {
            float t = 0f;
            while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
            yield return FadeTo(0f);
        }
        running = null;
    }

    IEnumerator FadeTo(float target) {
        if (!group) yield break;
        float start = group.alpha, t = 0f;
        while (t < fadeSeconds) {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, t / fadeSeconds);
            yield return null;
        }
        group.alpha = target;
    }
}
