using UnityEngine;

/// <summary>
/// Little adapter that sits between the SegmentBattery and my loss UI.
/// I wire this to the battery's OnDepleted UnityEvent so I don't have
/// to reference the loss UI from the battery script directly.
/// </summary>
public class BatteryLossHook : MonoBehaviour
{
    // The battery I'm listening to. If I forget to assign this,
    // Reset() will try to grab one from the same GameObject.
    [SerializeField] SegmentBattery battery;

    // The panel I want to show when the battery dies.
    // (If I ever switch to a LossScreen component, I can call Show() instead.)
    [SerializeField] GameObject lossPanel; // or a LossScreen script

    void Reset()
    {
        // Quality-of-life: auto-fill the battery ref if this lives on the same object.
        if (!battery) battery = GetComponent<SegmentBattery>();
    }

    /// <summary>
    /// Hook this to SegmentBattery.onDepleted in the Inspector.
    /// Fires once, right when the last bar is consumed.
    /// </summary>
    public void OnBatteryDepleted()
    {
        if (!lossPanel) return;

        // If I'm using a raw panel:
        lossPanel.SetActive(true);

        // If I'm using the LossScreen script instead:
        // lossPanel.GetComponent<LossScreen>()?.Show();

        // Pause the game + free the cursor so the player can read the screen.
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
