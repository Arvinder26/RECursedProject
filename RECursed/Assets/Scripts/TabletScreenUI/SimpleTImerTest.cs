using UnityEngine;
using TMPro;

/// <summary>
/// SUPER SIMPLE test version - if this doesn't work, something is fundamentally wrong
/// </summary>
public class SimpleTimerTest : MonoBehaviour
{
    public TMP_Text timerText;
    public CanvasGroup canvasGroup;

    void Awake()
    {
        Debug.LogError("========== SIMPLE TIMER TEST AWAKE! ==========");
        
        if (!timerText) Debug.LogError("Timer Text is NULL!");
        if (!canvasGroup) Debug.LogError("Canvas Group is NULL!");
        
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f; // Force visible immediately
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        if (timerText)
        {
            timerText.text = "TEST - IF YOU SEE THIS, THE SCRIPT WORKS!";
        }
    }

    void Update()
    {
        if (timerText)
        {
            timerText.text = $"TEST WORKING!\nTime: {Time.time:F1}s";
        }
    }
}