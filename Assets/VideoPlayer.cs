using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class VideoPlayer : MonoBehaviour
{
#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool CanOutputToTexture(string videoURL);
    [DllImport("__Internal")]
    private static extern bool PlayerReady();
    [DllImport("__Internal")]
    private static extern float DurationSeconds();
    [DllImport("__Internal")]
    private static extern void Extents(ref int width, ref int height);
    [DllImport("__Internal")]
    private static extern int CurFrameTexture();
    [DllImport("__Internal")]
    private static extern void PlayVideo(string videoURL);
    [DllImport("__Internal")]
    private static extern void PlayVideoView(int x, int y, int width, int height, string videoURL);
    [DllImport("__Internal")]
    private static extern void PauseVideo();
     [DllImport("__Internal")]
    private static extern void ResumeVideo();
    [DllImport("__Internal")]
    private static extern void RewindVideo();
    [DllImport("__Internal")]
    private static extern void SeekToVideo(float time);
    [DllImport("__Internal")]
    private static extern bool IsPlaying();
    [DllImport("__Internal")]
    private static extern void StopVideo();
#else
    private static bool CanOutputToTexture(string videoURL) { return false; }
    private static bool PlayerReady() { return false; }
    private static float DurationSeconds() { return 0f; }
    private static void Extents(ref int width, ref int height) { }
    private static int CurFrameTexture() { return 0; }
    private static void PlayVideo(string videoURL) { }
    private static void PlayVideoView(int x, int y, int width, int height, string videoURL) { }
    private static void PauseVideo() { }
    private static void ResumeVideo() { }
    private static void RewindVideo() { }
    private static void SeekToVideo(float time) { }
    private static bool IsPlaying() { return false; }
    private static void StopVideo() { }
#endif

    /// <summary>
    /// 再生可能のときtrueを返す
    /// </summary>
    public bool ready
    {
        get
        {
            return PlayerReady();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public float duration
    {
        get
        {
            return DurationSeconds();
        }
    }

    public bool isPlaying
    {
        get
        {
            return IsPlaying();
        }
    }
    public Vector2 videoSize
    {
        get
        {
            int width = 0, height = 0;
            Extents(ref width, ref height);
            return new Vector2(width, height);
        }
    }

    private Texture2D _videoTexture;

    public TextureFormat videoTextureFormat = TextureFormat.BGRA32;

    public Texture2D videoTexture
    {
        get
        {
            int nativeTex = ready ? CurFrameTexture() : 0;
            if (nativeTex != 0)
            {
                if (_videoTexture == null)
                {
                    _videoTexture = Texture2D.CreateExternalTexture((int)videoSize.x, (int)videoSize.y, videoTextureFormat,
                        false, false, (IntPtr)nativeTex);
                    _videoTexture.filterMode = FilterMode.Bilinear;
                    _videoTexture.wrapMode = TextureWrapMode.Repeat;
                }
                _videoTexture.UpdateExternalTexture((IntPtr)nativeTex);
            }
            else
            {
                _videoTexture = null;
            }
            return _videoTexture;
        }
    }

    public void PlayView(Rect rect, string videoURL)
    {
        PlayVideoView((int) rect.x, (int) rect.y, (int) rect.width, (int) rect.height, videoURL);
    }

    public void PlayView(int left, int top, int right, int bottom, string videoURL)
    {
        PlayVideoView(left, top, right, bottom, videoURL);
    }

    public void PlayTexture(string videoURL)
    {
        PlayTexture(videoURL, null);
    }

    public void PlayTexture(string videoURL, Material material)
    {
        if (CanOutputToTexture(videoURL))
        {
            PlayVideo(videoURL);
            StartCoroutine(UpdateTexture(material, false));
        }
    }

    private Material lastMaterial;
    private IEnumerator UpdateTexture(Material material, bool isResume)
    {
        if (isResume)
        {
            material = lastMaterial;
        }
        while (!isPlaying)
        {
            yield return null;
        }

        while (isPlaying)
        {
            if (!material)
            {
                renderer.material.mainTexture = videoTexture;
            }
            else
            {
                material.mainTexture = videoTexture;
            }
            yield return new WaitForEndOfFrame();
        }
        lastMaterial = material;
    }

    public void Pause()
    {
        PauseVideo();
    }
    public void Resume()
    {
        ResumeVideo();
        StartCoroutine(UpdateTexture(null, true));
    }

    public void Stop()
    {
        StopVideo();
    }
    /// <summary>
    /// RewindはPlayToTextureだと動かない（テクスチャが更新されない）ので現状では使えないようにしている
    /// </summary>
    //public static void Rewind()
    //{
    //    RewindVideo();
    //}

    //    public static void SeekTo(float time)
    //    {
    //        SeekToVideo(time);
    //    }
}
