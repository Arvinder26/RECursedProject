using UnityEngine;

public class AnomalyLog : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 3f;
    public float moveSpeed = 5f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    
    private bool isMoved = false;

    void Start()
    {
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

        // Smoothly move towards the target position
        Vector3 destination = isMoved ? targetPosition : originalPosition;
        transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime * moveSpeed);
    }
}
