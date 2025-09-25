using UnityEngine;
using TMPro;

public class LossScreen : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text messageText;
    [SerializeField] string defaultMessage = "You Lost";

    public void Show(string msg = null)
    {
        if (messageText) messageText.text = string.IsNullOrEmpty(msg) ? defaultMessage : msg;
        if (panel) panel.SetActive(true);

        // optional: pause & unlock cursor
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
