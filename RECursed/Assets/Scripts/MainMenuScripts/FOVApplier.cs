using UnityEngine;

public class FOVApplier : MonoBehaviour
{
    private const string FOVKey = "PlayerFOV";
    private Camera cam;

    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        if (!cam) return;

        float savedFOV = PlayerPrefs.GetFloat(FOVKey, 60f);
        cam.fieldOfView = savedFOV;

        Debug.Log("FOV Applied: " + savedFOV);
    }
}
