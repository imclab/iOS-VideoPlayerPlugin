using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class VideoPlayer : MonoBehaviour
{
#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool VideoPlayerPluginCanOutputToTexture(string videoURL);
    [DllImport("__Internal")]
    private static extern bool VideoPlayerPluginPlayerReady();
    [DllImport("__Internal")]
    private static extern float VideoPlayerPluginDurationSeconds();
    [DllImport("__Internal")]
    private static extern void VideoPlayerPluginExtents(ref int width, ref int height);
    [DllImport("__Internal")]
    private static extern int VideoPlayerPluginCurFrameTexture();
    [DllImport("__Internal")]
    private static extern void VideoPlayerPluginPlayVideo(string videoURL);
    [DllImport("__Internal")]
    private static extern void VideoPlayerPluginPlayVideoView(int x, int y, int width, int height, string videoURL);
    [DllImport("__Internal")]
    private static extern void VideoPlayerPluginPauseVideo();
     [DllImport("__Internal")]
    private static extern void VideoPlayerPluginResumeVideo();
    [DllImport("__Internal")]
    private static extern void VideoPlayerPluginRewindVideo();
    [DllImport("__Internal")]
    private static extern void VideoPlayerPluginSeekToVideo(float time);
    [DllImport("__Internal")]
    private static extern bool VideoPlayerPluginIsPlaying();
    [DllImport("__Internal")]
    private static extern void VideoPlayerPluginStopVideo();
#else
    private static bool VideoPlayerPluginCanOutputToTexture(string videoURL) { return false; }
    private static bool VideoPlayerPluginPlayerReady() { return false; }
    private static float VideoPlayerPluginDurationSeconds() { return 0f; }
    private static void VideoPlayerPluginExtents(ref int width, ref int height) { }
    private static int VideoPlayerPluginCurFrameTexture() { return 0; }
    private static void VideoPlayerPluginPlayVideo(string videoURL) { }
    private static void VideoPlayerPluginPlayVideoView(int x, int y, int width, int height, string videoURL) { }
    private static void VideoPlayerPluginPauseVideo() { }
    private static void VideoPlayerPluginResumeVideo() { }
    private static void VideoPlayerPluginRewindVideo() { }
    private static void VideoPlayerPluginSeekToVideo(float time) { }
    private static bool VideoPlayerPluginIsPlaying() { return false; }
    private static void VideoPlayerPluginStopVideo() { }
#endif

    /// <summary>
    /// 再生可能のときtrueを返す
    /// </summary>
    public bool ready
    {
        get
        {
            return VideoPlayerPluginPlayerReady();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public float duration
    {
        get
        {
            return VideoPlayerPluginDurationSeconds();
        }
    }

    public bool isPlaying
    {
        get
        {
            return VideoPlayerPluginIsPlaying();
        }
    }
    public Vector2 videoSize
    {
        get
        {
            int width = 0, height = 0;
            VideoPlayerPluginExtents(ref width, ref height);
            return new Vector2(width, height);
        }
    }

    private Texture2D _videoTexture;

    public TextureFormat videoTextureFormat = TextureFormat.BGRA32;

    public Texture2D videoTexture
    {
        get
        {
            int nativeTex = ready ? VideoPlayerPluginCurFrameTexture() : 0;
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
        VideoPlayerPluginPlayVideoView((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, videoURL);
    }

    public void PlayView(int left, int top, int right, int bottom, string videoURL)
    {
        VideoPlayerPluginPlayVideoView(left, top, right, bottom, videoURL);
    }

    public void PlayTexture(string videoURL)
    {
        PlayTexture(videoURL, null);
    }

    public void PlayTexture(string videoURL, Material material)
    {
        if (VideoPlayerPluginCanOutputToTexture(videoURL))
        {
            VideoPlayerPluginPlayVideo(videoURL);
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
        VideoPlayerPluginPauseVideo();
    }
    public void Resume()
    {
        VideoPlayerPluginResumeVideo();
        StartCoroutine(UpdateTexture(null, true));
    }

    public void Stop()
    {
        VideoPlayerPluginStopVideo();
    }
    /// <summary>
    /// RewindはPlayToTextureだと動かない（テクスチャが更新されない）ので現状では使えないようにしている
    /// </summary>
    //public static void Rewind()
    //{
    //    VideoPlayerPluginRewindVideo();
    //}

    //    public static void SeekTo(float time)
    //    {
    //        VideoPlayerPluginSeekToVideo(time);
    //    }
}
