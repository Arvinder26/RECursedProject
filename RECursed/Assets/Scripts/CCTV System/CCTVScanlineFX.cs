using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates an animated scanline effect for CCTV feeds.
/// Scrolls the texture and adds random flicker to simulate old security camera footage.
/// </summary>
public class CCTVScanlineFX : MonoBehaviour
{
    public RawImage scanlines;              // the overlay RawImage
    public Vector2 scrollSpeed = new(0, -0.2f);
    [Range(0,1)] public float flicker = 0.08f;

    /// <summary>
    /// Auto-assign the RawImage component when first added to a GameObject.
    /// </summary>
    void Reset() { scanlines = GetComponent<RawImage>(); }

    /// <summary>
    /// Animate the scanlines by scrolling the texture and applying random flicker.
    /// </summary>
    void Update()
    {
        if (scanlines)
        {
            // Scroll the scanline texture vertically for that classic CCTV look
            var r = scanlines.uvRect;
            r.position += scrollSpeed * Time.unscaledDeltaTime;
            scanlines.uvRect = r;

            // Add random flicker using Perlin noise for realistic static
            var c = scanlines.color;
            float n = Mathf.PerlinNoise(Time.time * 18f, 0f) * flicker;
            c.a = Mathf.Clamp01(0.22f + n);
            scanlines.color = c;
        }
    }
}