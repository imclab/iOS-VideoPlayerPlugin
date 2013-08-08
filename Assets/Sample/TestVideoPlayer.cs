using UnityEngine;

public class TestVideoPlayer : MonoBehaviour
{
    private VideoPlayer video;

    void Start()
    {
        video = GetComponent<VideoPlayer>();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
        if (GUILayout.Button("<size=42>ViewPlay</size>", GUILayout.Height(200)))
        {
            video.Stop();
            video.PlayView(new Rect(12, Screen.height / 2 + 12, 12, 12), "movie.mp4");
        }

        if (GUILayout.Button("<size=42>TexturePlay</size>", GUILayout.Height(200)))
        {
            video.Stop();
            video.PlayTexture("movie.mp4");
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));

        if (GUILayout.Button("<size=42>Pause</size>", GUILayout.Height(200)))
        {
            video.Pause();
        }
        if (GUILayout.Button("<size=42>Resume</size>", GUILayout.Height(200)))
        {
            video.Resume();
        }
        if (GUILayout.Button("<size=42>Stop</size>", GUILayout.Height(200)))
        {
            video.Stop();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(30);
        GUILayout.Label("" + Time.time);
        GUILayout.Label("Ready " + video.ready);
        GUILayout.Label("Duration " + video.duration + "");
        GUILayout.Label("VideoSize " + video.videoSize + "");
    }

    void Update()
    {
        transform.Rotate(new Vector3(-1, 1, 0) * 10 * Time.deltaTime);
    }
}
