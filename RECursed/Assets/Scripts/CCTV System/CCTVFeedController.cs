using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tiny CCTV feed switcher.
/// - Assign a list of Cameras (order matters)
/// - I pipe them into a shared RenderTexture
/// - I enable exactly one camera at a time and update a label
/// - Next/Prev methods cycle through the list
/// </summary>
public class CCTVFeedController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage      feedImage;        // where the feed appears
    [SerializeField] private RenderTexture feedTexture;      // shared RT target for all cameras
    [SerializeField] private TMP_Text      cameraLabel;      // label UI ("KITCHEN_CAMERA")
    [SerializeField] private string        labelSuffix = "_CAMERA";

    [Header("Cameras (cycling order)")]
    [SerializeField] private List<Camera> cameras = new List<Camera>(); // order = cycle order

    [Tooltip("Optional: if set and length matches cameras, these names override GameObject names.")]
    [SerializeField] private List<string> customNames = new List<string>();

    // Current index into the cameras list.
    int index;

    void Awake()
    {
        // Point each camera at the shared RenderTexture and keep them disabled by default.
        foreach (var cam in cameras)
        {
            if (!cam) continue;
            cam.targetTexture = feedTexture;
            cam.enabled = false;
        }

        // Wire the RT into the RawImage so it actually displays something.
        if (feedImage) feedImage.texture = feedTexture;
    }

    void OnEnable()
    {
        // On re-enable, show the currently selected camera (if any).
        if (cameras.Count > 0) Show(index);
    }

    void Start()
    {
        // On first run I prefer to force index 0 so everything starts consistent.
        if (cameras.Count > 0) Show(0);
    }

    /// <summary>Go to the next camera in the list (wraps).</summary>
    public void NextCam() => Show(index + 1);

    /// <summary>Go to the previous camera in the list (wraps).</summary>
    public void PrevCam() => Show(index - 1);

    /// <summary>
    /// Core swap logic:
    /// - Wraps the index cleanly
    /// - Enables only the active camera
    /// - Updates the label with either a custom name or the GameObject name
    /// </summary>
    void Show(int newIndex)
    {
        if (cameras.Count == 0) return;

        // Safe modulo for negatives (wraps both directions).
        index = (newIndex % cameras.Count + cameras.Count) % cameras.Count;

        // Enable just the active camera; keep others off.
        for (int i = 0; i < cameras.Count; i++)
        {
            if (!cameras[i]) continue;
            cameras[i].enabled = (i == index);
        }

        // Update the label (nice to have, but optional).
        if (cameraLabel)
        {
            string name;
            if (customNames != null &&
                customNames.Count == cameras.Count &&
                !string.IsNullOrWhiteSpace(customNames[index]))
            {
                name = customNames[index];
            }
            else
            {
                // If I don't have a custom label, I prettify the GameObject name a bit.
                name = cameras[index].gameObject.name.Replace("_", " "); // nicer formatting
            }

            cameraLabel.SetText($"{name}{labelSuffix}");
        }
    }
}
