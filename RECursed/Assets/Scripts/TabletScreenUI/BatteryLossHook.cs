using UnityEngine;

public class BatteryLossHook : MonoBehaviour
{
    [SerializeField] SegmentBattery battery;
    [SerializeField] GameObject lossPanel; // or a LossScreen script

    void Reset()
    {
        if (!battery) battery = GetComponent<SegmentBattery>();
    }

    public void OnBatteryDepleted()
    {
        if (!lossPanel) return;
        lossPanel.SetActive(true); // or lossPanel.GetComponent<LossScreen>().Show();
        Time.timeScale = 0f;       // if your loss flow pauses the game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
