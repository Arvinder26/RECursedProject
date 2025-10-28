using UnityEngine;

public class FOVApplier : MonoBehaviour
{
    private const string FOVKey = "PlayerFOV";
    private Camera cam;

    void Awake()
    {
        // Find camera in the parent object's child objects
        cam = GetComponentInChildren<Camera>();
        if (!cam) return;

        // Load saved FOV, default is 60
        float savedFOV = PlayerPrefs.GetFloat(FOVKey, 60f);
        
        // Apply FOV to camera
        cam.fieldOfView = savedFOV;

        Debug.Log("FOV Applied: " + savedFOV);
    }
}
