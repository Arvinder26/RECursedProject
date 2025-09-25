using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Spawns the anomaly on given colliders and guarantees it faces the player.
public class AnomalySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject anomalyPrefab;
    public Collider[] spawnAreas;              // drag your Plane/MeshColliders here
    public Transform player;                   // drag playerMAIN or Main Camera

    [Header("Timing")]
    public float initialDelaySeconds = 0f;
    public Vector2 spawnIntervalRange = new Vector2(4f, 10f);
    public float lifetimeSeconds = 15f;
    public int spawnsPerRound = 1;

    [Header("Placement")]
    public float yOffset = 0.05f;              // lift slightly off surface
    public float minDistanceFromPlayer = 0f;
    public float rayStartPadding = 2f;

    [Header("Facing")]
    [Tooltip("Keeps rotating to face the player every frame.")]
    public bool facePlayerContinuously = true;
    [Tooltip("Use 0 if model's front is +Z, 180 if it's -Z, 90/-90 if sideways.")]
    public float yawOffsetDegrees = 0f;

    // internal
    int spawnsThisRound = 0;
    readonly List<int> order = new List<int>();
    int cursor = 0;

    void Awake()
    {
        if (!player && Camera.main) player = Camera.main.transform;
    }

    void OnEnable()
    {
        BuildOrder();
        StartRound();
    }

    void OnDisable() => StopAllCoroutines();

    public void StartRound()
    {
        StopAllCoroutines();
        spawnsThisRound = 0;
        if (spawnAreas != null && spawnAreas.Length > 0 && order.Count == 0)
            BuildOrder();
        StartCoroutine(RunRound());
    }

    IEnumerator RunRound()
    {
        if (initialDelaySeconds > 0f) yield return new WaitForSeconds(initialDelaySeconds);

        while (enabled && spawnsThisRound < spawnsPerRound)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalRange.x, spawnIntervalRange.y));
            if (!anomalyPrefab || spawnAreas == null || spawnAreas.Length == 0 || !player) continue;

            int tries = spawnAreas.Length;
            while (tries-- > 0)
            {
                Collider area = spawnAreas[NextIndex()];
                if (TryPointOnArea(area, out Vector3 pos))
                {
                    if (minDistanceFromPlayer <= 0f ||
                        Vector3.Distance(pos, player.position) >= minDistanceFromPlayer)
                    {
                        yield return SpawnOnce(pos);
                        spawnsThisRound++;
                        break;
                    }
                }
            }
        }
    }

    void BuildOrder()
    {
        order.Clear();
        if (spawnAreas == null) return;
        for (int i = 0; i < spawnAreas.Length; i++) order.Add(i);
        for (int i = 0; i < order.Count; i++)
        {
            int j = Random.Range(i, order.Count);
            (order[i], order[j]) = (order[j], order[i]);
        }
        cursor = 0;
    }

    int NextIndex()
    {
        if (order.Count == 0) BuildOrder();
        int idx = order[cursor];
        cursor = (cursor + 1) % order.Count;
        return idx;
    }

    IEnumerator SpawnOnce(Vector3 pos)
    {
        GameObject go = Instantiate(anomalyPrefab, pos, Quaternion.identity);

        // Ensure the spawned object faces the player continuously (Animator-safe).
        var fp = go.GetComponent<FacePlayerSimple>();
        if (!fp) fp = go.AddComponent<FacePlayerSimple>();
        fp.target = player;
        fp.yawOnly = true;
        fp.yawOffsetDegrees = yawOffsetDegrees;
        fp.enabled = facePlayerContinuously;

        // Also snap immediately on spawn (in case facePlayerContinuously is off)
        FacePlayerSimple.FaceNow(go.transform, player, true, yawOffsetDegrees);

        if (!facePlayerContinuously) yield return new WaitForSeconds(lifetimeSeconds);
        else
        {
            float t = 0f;
            while (t < lifetimeSeconds && go) { t += Time.deltaTime; yield return null; }
        }

        if (go) Destroy(go);
    }

    // Sample a point within the collider by raycasting down from above its bounds
    bool TryPointOnArea(Collider area, out Vector3 point)
    {
        point = default;
        if (!area) return false;

        Bounds b = area.bounds;

        for (int i = 0; i < 25; i++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float z = Random.Range(b.min.z, b.max.z);
            float y = b.max.y + Mathf.Max(rayStartPadding, 0.5f);

            Ray r = new Ray(new Vector3(x, y, z), Vector3.down);
            if (area.Raycast(r, out RaycastHit hit, (y - b.min.y) + 5f))
            {
                point = hit.point + Vector3.up * yOffset;
                return true;
            }
        }
        return false;
    }
}

/// Minimal, Animator-safe “face the target” helper that runs in LateUpdate.
public class FacePlayerSimple : MonoBehaviour
{
    public Transform target;
    public bool yawOnly = true;            // rotate only around Y
    public float yawOffsetDegrees = 0f;    // add 180/90/-90 if model forward differs

    void LateUpdate()
    {
        if (!target) return;
        FaceNow(transform, target, yawOnly, yawOffsetDegrees);
    }

    public static void FaceNow(Transform self, Transform tgt, bool yawOnly, float yawOffset)
    {
        if (!self || !tgt) return;

        Vector3 dir = tgt.position - self.position;
        if (yawOnly) dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f) return;

        Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
        if (Mathf.Abs(yawOffset) > 0.01f)
            look = Quaternion.AngleAxis(yawOffset, Vector3.up) * look;

        self.rotation = look;
    }
}
