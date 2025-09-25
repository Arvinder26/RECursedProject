using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Small controller that opens/closes my tablet UI,
/// saves/restores cursor state, disables gameplay behaviours while open,
/// and plays a little SFX on open/close.
/// </summary>
public class TabletPanelController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Root object of the tablet UI (TabletUIRoot or Panel). This is what gets SetActive(true/false).")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text hintText; // (left for future hint text; not used right now)

    [Header("Input")]
    [SerializeField] private KeyCode openKey = KeyCode.E;     // E opens/toggles the tablet
    [Tooltip("Optional. If set (e.g. Escape), E opens/toggles and this key closes.")]
    [SerializeField] private KeyCode closeKey = KeyCode.None; // None = only E is used
    [Tooltip("If true, pressing keys while mouse is over UI will NOT toggle/close the tablet.")]
    [SerializeField] private bool ignoreKeyWhenPointerOverUI = true;

    [Header("Disable while open")]
    [Tooltip("Drag the components you want disabled while the tablet is open (e.g. PlayerMovement, MouseMovement).")]
    [SerializeField] private Behaviour[] disableWhileOpen;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource; // UI AudioSource (2D)
    [SerializeField] private AudioClip openSfx;
    [SerializeField] private AudioClip closeSfx;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    // Whether the tablet is currently open. I keep the setter private so only this class flips it.
    public bool IsOpen { get; private set; }

    // I stash cursor state so I can restore it when the tablet closes.
    bool prevCursorVisible;
    CursorLockMode prevCursorLock;

    void Awake()
    {
        // Start hidden to avoid flashes on scene load.
        if (panelRoot) panelRoot.SetActive(false);

        // Make sure my UI audio source behaves like a one-shot player.
        if (sfxSource)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop        = false;
            sfxSource.spatialBlend = 0f; // 2D sound for UI
        }
    }

    void Update()
    {
        // If I'm configured to ignore keyboard while pointer is over UI, bail early.
        if (ignoreKeyWhenPointerOverUI && IsPointerOverUI())
            return;

        // Dedicated close key (if provided), e.g., Escape.
        if (closeKey != KeyCode.None && Input.GetKeyDown(closeKey))
        {
            Close();
            return;
        }

        // Main open/toggle key.
        if (Input.GetKeyDown(openKey))
        {
            if (closeKey != KeyCode.None) Toggle(); // if I have a separate close key, E toggles
            else                          Open();   // otherwise E only opens
        }
    }

    /// <summary>Flip the tablet state.</summary>
    public void Toggle()
    {
        if (IsOpen) Close();
        else        Open();
    }

    /// <summary>Open the tablet: show UI, unlock cursor, disable gameplay behaviours.</summary>
    public void Open()
    {
        if (IsOpen || panelRoot == null) return;

        // Remember the current cursor setup so I can restore it later.
        prevCursorVisible = Cursor.visible;
        prevCursorLock    = Cursor.lockState;

        panelRoot.SetActive(true);
        Cursor.visible  = true;
        Cursor.lockState = CursorLockMode.None;

        SetBehavioursEnabled(false);
        IsOpen = true;

        PlayOneShot(openSfx);
    }

    /// <summary>Close the tablet: hide UI, restore cursor, re-enable gameplay behaviours.</summary>
    public void Close()
    {
        if (!IsOpen || panelRoot == null) return;

        panelRoot.SetActive(false);

        Cursor.visible  = prevCursorVisible;
        Cursor.lockState = prevCursorLock;

        SetBehavioursEnabled(true);
        IsOpen = false;

        PlayOneShot(closeSfx);
    }

    /// <summary>Convenience hook for UI button OnClick().</summary>
    public void CloseFromUI() => Close();

    /// <summary>Turn a set of behaviours on/off while the tablet is open.</summary>
    void SetBehavioursEnabled(bool enabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var b in disableWhileOpen)
            if (b) b.enabled = enabled;
    }

    /// <summary>Small wrapper so I can play one-shots with a centralized volume.</summary>
    void PlayOneShot(AudioClip clip)
    {
        if (sfxSource && clip)
            sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>True if the pointer is currently over any UI element (EventSystem based).</summary>
    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }
}
