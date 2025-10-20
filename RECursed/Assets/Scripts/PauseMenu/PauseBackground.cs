using UnityEngine;
using UnityEngine.Video;

public class PauseBackground : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "pausescreen.mp4");
        videoPlayer.url = path;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }
}
