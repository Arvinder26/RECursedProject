using UnityEngine;

[DisallowMultipleComponent]
public class RoundMusicController : MonoBehaviour
{
    public AudioSource music;          // drag your Audio Source with the loop
    [Range(0f,1f)] public float targetVolume = 0.45f;
    public float fadeTime = 1.2f;

    Coroutine fader;

    void Awake()
    {
        if (!music) music = GetComponent<AudioSource>();
        if (music) { music.loop = true; if (!music.playOnAwake) music.volume = 0f; }
    }

    public void StartRoundMusic()
    {
        if (!music) return;
        if (!music.isPlaying) music.Play();
        FadeTo(targetVolume);
    }

    public void StopRoundMusic()
    {
        FadeTo(0f);
    }

    void FadeTo(float v)
    {
        if (fader != null) StopCoroutine(fader);
        fader = StartCoroutine(Fade(v));
    }

    System.Collections.IEnumerator Fade(float v)
    {
        float start = music.volume;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.05f, fadeTime);
            music.volume = Mathf.Lerp(start, v, t);
            yield return null;
        }
        music.volume = v;
        if (Mathf.Approximately(v, 0f)) music.Pause();
    }
}
