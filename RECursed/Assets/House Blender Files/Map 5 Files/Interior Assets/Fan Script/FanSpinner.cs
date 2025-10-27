using UnityEngine;

public class FanSpinner : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f; // Degrees per second

    void Update()
    {
        // Rotate the fan around its Y-axis (up direction)
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}