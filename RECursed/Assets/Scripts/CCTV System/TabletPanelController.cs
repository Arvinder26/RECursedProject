using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

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

    
    bool prevCursorVisible;
    CursorLockMode prevCursorLock;

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

    void Update()
    {
        if (ignoreKeyWhenPointerOverUI && IsPointerOverUI())
            return;

        
        if (closeKey != KeyCode.None && Input.GetKeyDown(closeKey))
        {
            Close();
            return;
        }

        
        if (Input.GetKeyDown(openKey))
        {
            if (closeKey == KeyCode.None)
                Toggle();
            else
                Open();
        }
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (IsOpen || panelRoot == null) return;

        
        prevCursorVisible = Cursor.visible;
        prevCursorLock    = Cursor.lockState;

        panelRoot.SetActive(true);
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;

        SetBehavioursEnabled(false);
        IsOpen = true;

        PlayOneShot(openSfx);
    }

    public void Close()
    {
        if (!IsOpen || panelRoot == null) return;

        panelRoot.SetActive(false);

        
        Cursor.visible   = prevCursorVisible;
        Cursor.lockState = prevCursorLock;

        SetBehavioursEnabled(true);
        IsOpen = false;

        PlayOneShot(closeSfx);
    }

    
    public void CloseFromUI() => Close();

    void SetBehavioursEnabled(bool enabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var b in disableWhileOpen)
            if (b) b.enabled = enabled;
    }

    void PlayOneShot(AudioClip clip)
    {
        if (sfxSource && clip)
            sfxSource.PlayOneShot(clip, sfxVolume);
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        
        return EventSystem.current.IsPointerOverGameObject();
    }
}
