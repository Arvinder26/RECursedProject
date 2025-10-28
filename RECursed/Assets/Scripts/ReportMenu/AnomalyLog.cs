using UnityEngine;

// Simple Demo: Toggles an object between its original position and an offset when "E" is pressed.

public class AnomalyLog : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 3f;
    public float moveSpeed = 5f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    
    // Current toggle state (false = at original, true = at target).
    private bool isMoved = false;

    void Start()
    {
	// Record start, precompute target this avoids doing math every frame.
        originalPosition = transform.position;
        targetPosition = originalPosition + Vector3.right * moveDistance;
    }

    void Update()
    {
        // Toggle target position when E is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            isMoved = !isMoved;
        }

        // Pick destination based on state and ease towards it.
        Vector3 destination = isMoved ? targetPosition : originalPosition;
        transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime * moveSpeed);
    }
}
