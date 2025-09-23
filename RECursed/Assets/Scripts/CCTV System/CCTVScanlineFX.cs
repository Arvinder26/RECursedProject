using UnityEngine;
using UnityEngine.UI;

public class CCTVScanlineFX : MonoBehaviour
{
    public RawImage scanlines;              // the overlay RawImage
    public Vector2 scrollSpeed = new(0, -0.2f);
    [Range(0,1)] public float flicker = 0.08f;

    void Reset() { scanlines = GetComponent<RawImage>(); }

    void Update()
    {
        if (scanlines)
        {
            
            var r = scanlines.uvRect;
            r.position += scrollSpeed * Time.unscaledDeltaTime;
            scanlines.uvRect = r;

            
            var c = scanlines.color;
            float n = Mathf.PerlinNoise(Time.time * 18f, 0f) * flicker;
            c.a = Mathf.Clamp01(0.22f + n);
            scanlines.color = c;
        }
    }
}
