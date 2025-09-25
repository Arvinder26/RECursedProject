using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnomalySpawnerAreas : MonoBehaviour
{
    [Header("References")]
    public GameObject anomalyPrefab;
    public Collider[] spawnAreas;     // your Mesh/Box Colliders
    public Transform player;          // drag player or camera

    [Header("Timing")]
    public float initialDelaySeconds = 0f;
    public Vector2 spawnIntervalRange = new Vector2(4f, 10f);
    public float lifetimeSeconds = 15f;
    public int spawnsPerRound = 3;

    [Header("Placement")]
    public float yOffset = 0.05f;
    public float minDistanceFromPlayer = 0f;
    public float rayStartPadding = 2f;

    [Header("Facing")]
    public bool facePlayerContinuously = true;       // keep turning every frame
    [Tooltip("Add 180 if your model’s ‘front’ is backwards.")]
    public float yawOffsetDegrees = 0f;

    // internal
    int spawnsThisRound = 0;
    List<int> order = new List<int>();
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
            float wait = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            yield return new WaitForSeconds(wait);

            if (!anomalyPrefab || spawnAreas == null || spawnAreas.Length == 0 || !player) continue;

            int tries = spawnAreas.Length;
            bool spawned = false;

            while (tries-- > 0 && !spawned)
            {
                Collider area = spawnAreas[NextIndex()];
                if (TryPointOnArea(area, out Vector3 pos))
                {
                    if (minDistanceFromPlayer <= 0f ||
                        Vector3.Distance(pos, player.position) >= minDistanceFromPlayer)
                    {
                        yield return SpawnOnce(pos);
                        spawnsThisRound++;
                        spawned = true;
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

        // Face once at spawn
        FacePlayerYaw(go.transform);

        if (facePlayerContinuously)
        {
            float t = 0f;
            while (t < lifetimeSeconds && go)
            {
                FacePlayerYaw(go.transform);
                t += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(lifetimeSeconds);
        }

        if (go) Destroy(go);
    }

    // ——— helpers ———

    // Raycast straight down within the collider’s bounds to find a surface point
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

    // Rotate ONLY around Y so +Z looks at the player (add yaw offset if needed)
    void FacePlayerYaw(Transform t)
    {
        if (!t || !player) return;

        Vector3 to = player.position - t.position;
        to.y = 0f;
        if (to.sqrMagnitude < 1e-6f) return;

        float yaw = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg + yawOffsetDegrees;
        t.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (spawnAreas == null) return;
        Gizmos.color = new Color(0, 1, 1, 0.35f);
        foreach (var c in spawnAreas)
        {
            if (!c) continue;
            Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
        }
    }
#endif
}
