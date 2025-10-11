using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// -----------------------------------------------------------------------------
/// Tiny CCTV feed switcher.
///  - Cycles a list of cameras (order matters)
///  - All cameras render to one shared RenderTexture
///  - A single RawImage displays that texture
///  - We enable exactly one camera at a time and update a label
///  - Next/Prev steps through the list
/// -----------------------------------------------------------------------------
public class CCTVFeedController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage feedImage;            // where the feed appears
    [SerializeField] private RenderTexture feedTexture;     // shared RT target for all cameras
    [SerializeField] private TMP_Text cameraLabel;          // label UI ("KITCHEN_CAMERA")
    [SerializeField] private string labelSuffix = "_CAMERA";// label suffix

    [Header("Cameras (cycling order)")]
    [SerializeField] private List<Camera> cameras = new List<Camera>();      // order = cycle order
    [Tooltip("Optional: if set and length matches cameras, these names override GameObject names.")]
    [SerializeField] private List<string> customNames = new List<string>();  // custom label overrides

    // current index into the cameras list
    private int index = -1;

    private void Awake()
    {
        // Auto-wire UI refs if left empty
        if (!feedImage)   feedImage  = GetComponentInChildren<RawImage>(true);
        if (!cameraLabel) cameraLabel = GetComponentInChildren<TMP_Text>(true);

        // Keep the RawImage pointing at our shared texture.
        if (feedImage && feedTexture) feedImage.texture = feedTexture;

        // Point each camera at the shared RenderTexture and keep them disabled by default.
        for (int i = 0; i < cameras.Count; i++)
        {
            var cam = cameras[i];
            if (!cam) continue;

            cam.enabled = false;
            cam.targetTexture = null;
        }
    }

    private void OnEnable()
    {
        // On re-enable, ensure the RawImage is hooked up and show the currently selected camera (if any).
        if (feedImage && feedTexture) feedImage.texture = feedTexture;

        if (cameras.Count > 0)
        {
            if (index < 0) Show(0);
            else           Show(index);
        }
    }

    private void Start()
    {
        // On first run I prefer to force index 0 so everything starts consistent.
        if (index < 0 && cameras.Count > 0) Show(0);
    }

    /// <summary>Go to the next camera in the list.</summary>
    public void NextCam() => Show(index + 1);

    /// <summary>Go to the previous camera in the list.</summary>
    public void PrevCam() => Show(index - 1);

    /// <summary>
    /// Core show logic:
    ///  - Safe wrap for negatives/overflow
    ///  - Enables only the active camera
    ///  - Routes the active camera to the shared RT
    ///  - Updates the label with either a custom name or the GameObject name
    /// </summary>
    private void Show(int newIndex)
    {
        if (cameras == null || cameras.Count == 0) return;

        // Safe modulo for negatives (wraps both directions).
        index = ((newIndex % cameras.Count) + cameras.Count) % cameras.Count;

        // Enable just the active camera; keep others off.
        for (int i = 0; i < cameras.Count; i++)
        {
            var cam = cameras[i];
            if (!cam) continue;

            bool isActive = (i == index);
            cam.enabled = isActive;
            cam.targetTexture = isActive ? feedTexture : null;
        }

        // Make sure the RawImage shows the shared RT.
        if (feedImage && feedTexture) feedImage.texture = feedTexture;

        // Update the label (nice to have, but optional).
        if (cameraLabel)
        {
            string name;

            // If we have a custom label, prefer that.
            if (customNames != null &&
                index < customNames.Count &&
                !string.IsNullOrWhiteSpace(customNames[index]))
            {
                name = customNames[index];
            }
            else
            {
                // Otherwise prettify the GameObject name.
                var cam = cameras[index];
                name = cam ? cam.gameObject.name : "CAMERA";
                name = name.Replace("_", " ").Replace("-", " "); // minor formatting
            }

            cameraLabel.SetText($"{name}{labelSuffix}");
        }
    }
}
