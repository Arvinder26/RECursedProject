using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Plays sound effects for UI button interactions (hover and click).
/// Implements Unity's event system interfaces to detect pointer events.
/// Includes cooldown to prevent rapid-fire clicking sounds.
/// </summary>
public class ButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Where to play")]
    public AudioSource source;

    [Header("Clips")]
    public AudioClip hoverClip;
    public AudioClip clickClip;

    // Track if the pointer was pressed while over this button
    private bool pressedInside;
    // Prevents click sound spam by enforcing a minimum delay between clicks
    private float cooldownUntil;           
    private const float cooldown = 0.05f;

    /// <summary>
    /// Called when the pointer enters the button area.
    /// Plays the hover sound effect.
    /// </summary>
    public void OnPointerEnter(PointerEventData e)
    {
        if (hoverClip && source) source.PlayOneShot(hoverClip, 1f);
    }

    /// <summary>
    /// Called when the pointer is pressed down on the button.
    /// Tracks that the click started inside this button.
    /// </summary>
    public void OnPointerDown(PointerEventData e)
    {
        pressedInside = true;
    }

    /// <summary>
    /// Called when the pointer is released.
    /// Plays the click sound only if the press started inside and cooldown has expired.
    /// </summary>
    public void OnPointerUp(PointerEventData e)
    {
        // Don't play click sound if press didn't start here or if still in cooldown
        if (!pressedInside || Time.unscaledTime < cooldownUntil) return;
        pressedInside = false;
        cooldownUntil = Time.unscaledTime + cooldown;

        if (clickClip && source) source.PlayOneShot(clickClip, 1f);
    }
}