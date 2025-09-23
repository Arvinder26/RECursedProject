using UnityEngine;
using System.Collections;

public class AnomalySpawner : MonoBehaviour
{
    public enum Axis { Z, NegZ, X, NegX, Y, NegY }

    [Header("References")]
    public GameObject anomalyPrefab;
    public Collider spawnArea;
    public Transform player;

    [Header("Timing")]
    public Vector2 spawnIntervalRange = new Vector2(4f, 10f);
    public Vector2 lifetimeRange = new Vector2(2f, 3f);

    [Header("Placement")]
    public LayerMask groundMask;
    public float surfaceRaycastPadding = 5f;
    public float yOffset = 0.05f;
    public float minDistanceFromPlayer = 0f;

    [Header("Behaviour")]
    [Tooltip("If true, it keeps turning to face the player while alive.")]
    public bool facePlayerContinuously = false;

    [Tooltip("Extra yaw tweak after alignment (degrees). Try 180/90/-90 if needed.")]
    public float yawOffsetDegrees = 0f;

    [Header("Mesh Targeting")]
    [Tooltip("OPTIONAL: name of the child to rotate (e.g. your mesh like 'tripo_node_...'). If empty, the first Renderer found will be used.")]
    public string meshChildName = "";

    [Tooltip("Which local axis of the mesh should face the player.")]
    public Axis modelForwardAxis = Axis.Z;

    void Awake()
    {
        if (player == null && Camera.main != null) player = Camera.main.transform;
        if (groundMask.value == 0) groundMask = ~0; // default to Everything
    }

    void OnEnable()  => StartCoroutine(SpawnLoop());
    void OnDisable() => StopAllCoroutines();

    IEnumerator SpawnLoop()
    {
        while (enabled)
        {
            float wait = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            yield return new WaitForSeconds(wait);
            yield return SpawnOnce();
        }
    }

    IEnumerator SpawnOnce()
    {
        if (!anomalyPrefab || !spawnArea || !player) yield break;
        if (!TryGetRandomPointOnArea(out Vector3 pos)) yield break;

        if (minDistanceFromPlayer > 0f &&
            Vector3.Distance(pos, player.position) < minDistanceFromPlayer)
            yield break;

        GameObject go = Instantiate(anomalyPrefab, pos, Quaternion.identity);

        Transform rotTarget = GetMeshRoot(go.transform);
        FacePlayer(rotTarget);

        float life = Random.Range(lifetimeRange.x, lifetimeRange.y);

        if (facePlayerContinuously)
        {
            float t = 0f;
            while (t < life && go != null)
            {
                FacePlayer(rotTarget);
                t += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(life);
        }

        if (go != null) Destroy(go);
    }

    void FacePlayer(Transform rotateThis)
    {
        if (!rotateThis || !player) return;

        // Target direction on XZ plane
        Vector3 dir = player.position - rotateThis.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f) return;
        dir.Normalize();

        // Current world-space vector that we consider the model's "forward"
        Vector3 currentForward = GetWorldAxisVector(rotateThis, modelForwardAxis);

        // Rotate the mesh so that its chosen forward aligns to dir
        Quaternion look = Quaternion.FromToRotation(currentForward, dir) * rotateThis.rotation;

        if (Mathf.Abs(yawOffsetDegrees) > 0.01f)
            look = Quaternion.AngleAxis(yawOffsetDegrees, Vector3.up) * look;

        rotateThis.rotation = look;
    }

    Vector3 GetWorldAxisVector(Transform t, Axis axis)
    {
        switch (axis)
        {
            case Axis.Z:    return t.forward;
            case Axis.NegZ: return -t.forward;
            case Axis.X:    return t.right;
            case Axis.NegX: return -t.right;
            case Axis.Y:    return t.up;
            case Axis.NegY: return -t.up;
        }
        return t.forward;
    }

    Transform GetMeshRoot(Transform instanceRoot)
    {
        if (!string.IsNullOrEmpty(meshChildName))
        {
            var child = instanceRoot.Find(meshChildName);
            if (child) return child;
        }

        var rend = instanceRoot.GetComponentInChildren<Renderer>();
        if (rend) return rend.transform;

        return instanceRoot;
    }

    bool TryGetRandomPointOnArea(out Vector3 point)
    {
        Bounds b = spawnArea.bounds;

        for (int i = 0; i < 10; i++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float z = Random.Range(b.min.z, b.max.z);
            float y = b.max.y + surfaceRaycastPadding;

            Ray ray = new Ray(new Vector3(x, y, z), Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore))
            {
                point = hit.point + Vector3.up * yOffset;
                return true;
            }
        }

        point = default;
        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (spawnArea)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
        }
    }
#endif
}
