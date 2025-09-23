using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Where to play")]
    public AudioSource source;

    [Header("Clips")]
    public AudioClip hoverClip;
    public AudioClip clickClip;

    
    private bool pressedInside;
    private float cooldownUntil;           
    private const float cooldown = 0.05f;

    public void OnPointerEnter(PointerEventData e)
    {
        if (hoverClip && source) source.PlayOneShot(hoverClip, 1f);
    }

    public void OnPointerDown(PointerEventData e)
    {
        pressedInside = true;
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!pressedInside || Time.unscaledTime < cooldownUntil) return;
        pressedInside = false;
        cooldownUntil = Time.unscaledTime + cooldown;

        if (clickClip && source) source.PlayOneShot(clickClip, 1f);
    }
}
