using UnityEngine;

/// <summary>
/// Simple utility script to open a UI panel.
/// Typically called by UI button OnClick events.
/// </summary>
public class PanelOpener : MonoBehaviour
{

    public GameObject Panel;

    /// <summary>
    /// Activates the assigned panel GameObject.
    /// Call this from a UI button to open the panel.
    /// </summary>
    public void OpenPanel()
    {
        if (Panel != null)
        {
            Panel.SetActive(true);
        }
    }
}