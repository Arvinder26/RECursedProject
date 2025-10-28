using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Controls the tablet UI panel opening/closing with keyboard input.
/// Handles cursor visibility, disabling player controls, and playing audio feedback.
/// </summary>
public class TabletPanelController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Root object of the tablet UI (TabletUIRoot or Panel). This is what gets SetActive(true/false).")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text hintText;

    [Header("Input")]
    [SerializeField] private KeyCode openKey  = KeyCode.E;
    [Tooltip("Optional. If None, E toggles open/close. If set (e.g. Escape), E opens and this key closes.")]
    [SerializeField] private KeyCode closeKey = KeyCode.None;
    [Tooltip("If true, pressing keys while the mouse is over UI will NOT toggle/close the tablet.")]
    [SerializeField] private bool ignoreKeyWhenPointerOverUI = true;

    [Header("Disable while open")]
    [Tooltip("Drag the components you want disabled while the tablet is open (e.g. PlayerMovement, MouseMovement).")]
    [SerializeField] private Behaviour[] disableWhileOpen;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;    
    [SerializeField] private AudioClip openSfx;
    [SerializeField] private AudioClip closeSfx;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    public bool IsOpen { get; private set; }

    /// <summary>
    /// Initialize the tablet panel and audio source settings.
    /// Ensures the panel starts closed.
    /// </summary>
    void Awake()
    {
        if (panelRoot) panelRoot.SetActive(false);

        if (sfxSource)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f; 
        }
    }

    /// <summary>
    /// Lock the cursor at the start of the game.
    /// </summary>
    void Start()
    {
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Check for keyboard input to open/close the tablet.
    /// Respects UI hover detection if enabled.
    /// </summary>
    void Update()
    {
        // Don't process input if mouse is hovering over UI elements
        if (ignoreKeyWhenPointerOverUI && IsPointerOverUI())
            return;

        // Handle dedicated close key if one is assigned
        if (closeKey != KeyCode.None && Input.GetKeyDown(closeKey))
        {
            Close();
            return;
        }

        // Handle open key (either toggles or just opens depending on closeKey setting)
        if (Input.GetKeyDown(openKey))
        {
            if (closeKey == KeyCode.None)
                Toggle();
            else
                Open();
        }
    }

    /// <summary>
    /// Toggle the tablet open or closed.
    /// </summary>
    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    /// <summary>
    /// Open the tablet panel, show cursor, and disable player controls.
    /// </summary>
    public void Open()
    {
        if (IsOpen || panelRoot == null) return;

        panelRoot.SetActive(true);
        
        // Show cursor so player can interact with the tablet UI
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;

        SetBehavioursEnabled(false);
        IsOpen = true;

        PlayOneShot(openSfx);
    }

    /// <summary>
    /// Close the tablet panel, hide cursor, and re-enable player controls.
    /// </summary>
    public void Close()
    {
        if (!IsOpen || panelRoot == null) return;

        panelRoot.SetActive(false);

        // Hide cursor and lock it for first-person gameplay
        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Locked;

        SetBehavioursEnabled(true);
        IsOpen = false;

        PlayOneShot(closeSfx);
    }

    /// <summary>
    /// Called by UI button to close the tablet.
    /// </summary>
    public void CloseFromUI() => Close();

    /// <summary>
    /// Enable or disable the player control scripts (movement, camera, etc.)
    /// </summary>
    void SetBehavioursEnabled(bool enabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var b in disableWhileOpen)
            if (b) b.enabled = enabled;
    }

    /// <summary>
    /// Play a sound effect at the specified volume.
    /// </summary>
    void PlayOneShot(AudioClip clip)
    {
        if (sfxSource && clip)
            sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// Check if the mouse cursor is currently over any UI element.
    /// </summary>
    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        
        return EventSystem.current.IsPointerOverGameObject();
    }
}