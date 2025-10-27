using UnityEngine;
using TMPro;

public class SubtitleUI : MonoBehaviour
{
    public static SubtitleUI Instance { get; private set; }

    public TMP_Text text;
    public CanvasGroup group;
    public float fadeSeconds = 0.05f;
    public float minVisibleSeconds = 0.12f;

    float holdUntil = -1f;
    string lastMsg = "";
    bool hardShow;
    float FadeVel => (fadeSeconds <= 1e-4f) ? 999f : 1f / fadeSeconds;

    // ---- Helpers used by FootstepSubtitles ----
    public bool IsShowing => group && (Time.unscaledTime <= holdUntil);
    public string CurrentText => lastMsg;
    public bool IsShowingOther(string myCaption) => IsShowing && lastMsg != myCaption;
    public float VisibleRemaining => Mathf.Max(0f, holdUntil - Time.unscaledTime);
    public void HideIfTextEquals(string s) { if (lastMsg == s) Hide(); }
    // -------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (!group) group = GetComponent<CanvasGroup>();
        if (group) { group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false; }
        if (text) { var c = text.color; if (c.a < 0.99f) text.color = new Color(c.r,c.g,c.b,1f); text.raycastTarget=false; text.enabled=true; }
    }

    void Update()
    {
        if (!group) return;
        if (hardShow) { group.alpha = 1f; hardShow = false; return; }
        float target = (Time.unscaledTime <= holdUntil) ? 1f : 0f;
        group.alpha = Mathf.MoveTowards(group.alpha, target, FadeVel * Time.unscaledDeltaTime);
    }

    public void Show(string msg) {
        if (!PlayerPrefs.HasKey("SubtitlesEnabled") || PlayerPrefs.GetInt("SubtitlesEnabled") == 0) return;

        SetMsg(msg); 
        hardShow = true; 
        holdUntil = float.MaxValue; 
    }
    public void ShowForSeconds(string msg, float seconds)
    {
        if (!PlayerPrefs.HasKey("SubtitlesEnabled") || PlayerPrefs.GetInt("SubtitlesEnabled") == 0) return;

        SetMsg(msg); hardShow = true;
        float reqEnd = Time.unscaledTime + Mathf.Max(seconds, minVisibleSeconds);
        if (reqEnd > holdUntil) holdUntil = reqEnd;
    }
    public void Hide() { holdUntil = -1f; }

    void SetMsg(string msg)
    {
        if (!text) return;
        if (!text.gameObject.activeSelf) text.gameObject.SetActive(true);
        if (!text.enabled) text.enabled = true;
        if (msg != lastMsg) { lastMsg = msg; text.text = msg; text.ForceMeshUpdate(true); }
    }
}
