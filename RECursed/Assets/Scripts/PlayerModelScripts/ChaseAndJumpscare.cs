using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ChaseAndJumpscare : MonoBehaviour
{
    [Header("Targets")]
    public Transform playerRoot;
    public Transform playerCamera;

    [Header("Chase")]
    public float moveSpeed = 2.3f;
    public float rotateSpeed = 8f;
    public float scareDistance = 1.6f;
    public float startChaseDistance = 100f;
    public bool ignoreTimeScaleWhileChasing = true;

    [Header("Spawn Timer (real-time)")]
    public bool delaySpawn = false;
    public float spawnAfterSeconds = 10f;
    public float spawnJitterSeconds = 0f;
    public bool hideRenderersUntilSpawn = true;

    [Header("Animator (Idle/Walk only)")]
    public Animator anim;
    public string walkBool = "IsChasing";
    public string walkStateName = "Walk";
    public string idleStateName = "Idle";   // used only if you don't use the bool
    public bool useUnscaledAnimator = true; // keep as-is if you want
    public float walkAnimSpeed = 1.0f;

    [Header("Jumpscare")]
    public AudioClip scream;
    public GameObject jumpscareUI;
    public UnityEvent onScare;
    public Behaviour[] disableOnScare;
    public bool lockPlayerCameraOnScare = true;
    public float cameraLockDuration = 0.35f;
    public bool freezeTimeOnScare = true;
    public bool unlockCursorOnScare = true;
    public bool destroyAfterScare = true;
    public bool deactivateInstead = false;
    public float extraDespawnDelay = 0.05f;

    // --- private ---
    AudioSource audioSrc;
    CharacterController cc;
    Renderer[] rends;
    Collider[] colls;
    bool spawned;
    bool scared;

    // track edge so we don't restart the animation every frame
    bool lastChasing = false;
    int walkHash; // cached state hash

    float DT => (ignoreTimeScaleWhileChasing && Time.timeScale == 0f)
                ? Time.unscaledDeltaTime : Time.deltaTime;

    void Reset() { AutoFindRefs(); }

    void OnValidate()
    {
        if (!anim) anim = GetComponentInChildren<Animator>(true);
        if (anim && useUnscaledAnimator) anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        cc = GetComponent<CharacterController>();
        AutoFindRefs();

        if (anim)
        {
            anim.applyRootMotion = false;
            if (useUnscaledAnimator) anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate; // keep animating off-camera
        }

        walkHash = !string.IsNullOrEmpty(walkStateName) ? Animator.StringToHash(walkStateName) : 0;

        rends = GetComponentsInChildren<Renderer>(true);
        colls = GetComponentsInChildren<Collider>(true);

        if (delaySpawn) PrepareHiddenState();
        if (gameObject.isStatic)
            Debug.LogWarning("[ChaseAndJumpscare] Enemy is Static — unset Static for moving objects.");
    }

    void Start()
    {
        if (delaySpawn) StartCoroutine(SpawnRoutine());
        else spawned = true;
    }

    void AutoFindRefs()
    {
        if (!playerRoot)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) playerRoot = p.transform;
        }
        if (!playerRoot && !playerCamera && Camera.main)
        {
            playerCamera = Camera.main.transform;
            playerRoot = playerCamera.root;
        }
        if (!anim) anim = GetComponentInChildren<Animator>();
    }

    void PrepareHiddenState()
    {
        spawned = false;
        if (hideRenderersUntilSpawn && rends != null)
            foreach (var r in rends) r.enabled = false;
        if (colls != null)
            foreach (var c in colls) c.enabled = false;
        if (anim) anim.enabled = false;
    }

    System.Collections.IEnumerator SpawnRoutine()
    {
        float jitter = (spawnJitterSeconds > 0f) ? Random.Range(-spawnJitterSeconds, spawnJitterSeconds) : 0f;
        float wait = Mathf.Max(0f, spawnAfterSeconds + jitter);
        float t0 = Time.unscaledTime;
        while (Time.unscaledTime - t0 < wait) yield return null;

        if (hideRenderersUntilSpawn && rends != null)
            foreach (var r in rends) r.enabled = true;
        if (colls != null)
            foreach (var c in colls) c.enabled = true;
        if (anim) { anim.enabled = true; anim.speed = walkAnimSpeed; }
        spawned = true;
    }

    void Update()
    {
        if (!spawned || scared || playerRoot == null) return;

        Vector3 to = playerRoot.position - transform.position;
        Vector3 flat = new Vector3(to.x, 0f, to.z);
        float dist = flat.magnitude;

        bool shouldChase = dist <= startChaseDistance;

        // rotate to face player
        if (flat.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(flat.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotateSpeed * DT);
        }

        // move toward player if within chase range
        if (shouldChase)
        {
            Vector3 step = (flat.sqrMagnitude > 0.0001f ? flat.normalized : transform.forward) * moveSpeed * DT;
            if (cc != null) cc.Move(step);
            else transform.position += step;
        }

        SetWalk(shouldChase);

        if (dist <= scareDistance)
            StartCoroutine(DoScareRoutine());
    }

    void SetWalk(bool chasing)
    {
        if (!anim) return;

        anim.speed = walkAnimSpeed;

        if (HasBoolParam(anim, walkBool))
        {
            // Rely on Animator transitions; no looping issues if the clip/state is looped.
            anim.SetBool(walkBool, chasing);
        }
        else
        {
            // Only crossfade on edges (state changes), not every frame.
            if (chasing && !lastChasing)
            {
                if (!string.IsNullOrEmpty(walkStateName))
                    anim.CrossFadeInFixedTime(walkHash, 0.1f, 0, 0f);
            }
            else if (!chasing && lastChasing)
            {
                if (!string.IsNullOrEmpty(idleStateName))
                    anim.CrossFadeInFixedTime(idleStateName, 0.1f, 0, 0f);
            }
        }

        lastChasing = chasing;
    }

    // UPDATED: plays audio immediately; runs camera lock in parallel; restores time & disabled behaviours
    System.Collections.IEnumerator DoScareRoutine()
    {
        if (scared) yield break;
        scared = true;

        // remember current timescale so we can restore it later
        float prevTimeScale = Time.timeScale;

        // freeze enemy pose
        if (anim)
        {
            if (HasBoolParam(anim, walkBool)) anim.SetBool(walkBool, false);
            anim.speed = 0f;
        }

        // 1) Show UI immediately
        if (jumpscareUI) jumpscareUI.SetActive(true);

        // 2) Play audio immediately (2D so distance/occlusion don't mute it)
        float screamLen = 0f;
        if (scream && audioSrc)
        {
            audioSrc.spatialBlend = 0f; // 2D sound
            audioSrc.PlayOneShot(scream);
            screamLen = scream.length;
        }

        // 3) Disable listed behaviours and remember which ones we disabled
        var disabledNow = new List<Behaviour>();
        if (disableOnScare != null)
        {
            foreach (var b in disableOnScare)
            {
                if (b && b.enabled)
                {
                    b.enabled = false;
                    disabledNow.Add(b);
                }
            }
        }

        // 4) Camera lock IN PARALLEL (do NOT yield here)
        if (lockPlayerCameraOnScare && playerCamera)
            StartCoroutine(LockCameraToTarget(playerCamera, transform, cameraLockDuration));

        // 5) Freeze time & unlock cursor (UI keeps working; we wait using unscaled time)
        if (freezeTimeOnScare) Time.timeScale = 0f;
        if (unlockCursorOnScare) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        // 6) Hold until audio finishes (+ small extra)
        float t0 = Time.unscaledTime;
        while (Time.unscaledTime - t0 < screamLen + extraDespawnDelay)
            yield return null;

        // --- RESTORE STATE ---

        // restore timeScale
        if (freezeTimeOnScare) Time.timeScale = prevTimeScale;

        // re-enable anything we disabled
        foreach (var b in disabledNow)
            if (b) b.enabled = true;

        // hide UI if we are not destroying/deactivating this object
        if (!destroyAfterScare && !deactivateInstead && jumpscareUI)
            jumpscareUI.SetActive(false);

        // clean up enemy
        if (destroyAfterScare) Destroy(gameObject);
        else if (deactivateInstead) gameObject.SetActive(false);

        onScare?.Invoke();
    }

    System.Collections.IEnumerator LockCameraToTarget(Transform cam, Transform target, float duration)
    {
        if (!cam || !target || duration <= 0f) yield break;
        Quaternion start = cam.rotation;
        Vector3 dir = target.position - cam.position; if (dir.sqrMagnitude < 0.0001f) yield break;
        Quaternion end = Quaternion.LookRotation(dir.normalized, Vector3.up);

        float t0 = Time.unscaledTime;
        while (Time.unscaledTime - t0 < duration)
        {
            float t = (Time.unscaledTime - t0) / duration;
            cam.rotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }
        cam.rotation = end;
    }

    bool HasBoolParam(Animator a, string name)
    {
        if (!a || string.IsNullOrEmpty(name)) return false;
        var ps = a.parameters;
        for (int i = 0; i < ps.Length; i++)
            if (ps[i].type == AnimatorControllerParameterType.Bool && ps[i].name == name) return true;
        return false;
    }
}
