using UnityEngine;
using UnityEngine.Video;
using System;
using System.Collections;

public class MoviePlayer : MonoBehaviour
{
    public static MoviePlayer Instance;

    [SerializeField] private VideoPlayer videoPlayer;      // Inspectorでアサイン
    [SerializeField] private RenderTexture renderTexture;  // InspectorでMovieRenderTextureをアサイン
    [SerializeField] private GameObject rawImageGO;        // Inspectorで RawImage の GameObject をアサイン
    [SerializeField] private CanvasGroup fadeCanvas;       // 任意（フェード用）

    private Action onFinishCallback;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.gameObject.SetActive(false);
        }

        if (rawImageGO != null) rawImageGO.SetActive(false);
        if (fadeCanvas != null) fadeCanvas.gameObject.SetActive(false);
    }

    public void PlayMovie(VideoClip clip, Action onFinish = null)
    {
        if (videoPlayer == null)
        {
            Debug.LogWarning("MoviePlayer: VideoPlayerが未設定です");
            onFinish?.Invoke();
            return;
        }

        onFinishCallback = onFinish;
        StartCoroutine(PlayMovieRoutine(clip));
    }

    private IEnumerator PlayMovieRoutine(VideoClip clip)
    {
        Debug.Log($"[MoviePlayer] PlayMovieRoutine start. clip={(clip != null ? clip.name : "null")}");

        // RawImage を有効にする（描画領域の表示）
        if (rawImageGO != null) rawImageGO.SetActive(true);

        // フェード表示（あれば）
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1f;
            fadeCanvas.gameObject.SetActive(true);
        }

        // VideoPlayer のターゲットを設定して準備
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.targetTexture = renderTexture; // Inspectorでセットしてある場合は不要
        videoPlayer.clip = clip;

        // Audio のセット（AudioSource を利用している場合）
        if (videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
        {
            var aud = videoPlayer.GetTargetAudioSource(0);
            if (aud != null) aud.Play(); // 通常は videoPlayer.Play() で同期されるが保険
        }

        // Prepare を使って最初のフレームを待つ
        videoPlayer.Prepare();
        float prepareTimeout = 3f;
        float t = 0f;
        while (!videoPlayer.isPrepared && t < prepareTimeout)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("[MoviePlayer] VideoPlayer の準備がタイムアウトしましたが再生を試みます。");
        }
        else
        {
            Debug.Log("[MoviePlayer] VideoPlayer isPrepared");
        }

        videoPlayer.Play();
        Debug.Log("[MoviePlayer] videoPlayer.Play() called");

        // 再生が始まるまで少し待つ（任意）
        yield return null;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("MoviePlayer: ムービー再生完了");

        vp.Stop();
        vp.gameObject.SetActive(false);

        // RawImage 非表示に戻す
        if (rawImageGO != null) rawImageGO.SetActive(false);

        if (fadeCanvas != null) fadeCanvas.gameObject.SetActive(false);

        // 切断（ターゲット解放）
        videoPlayer.targetTexture = null;

        onFinishCallback?.Invoke();
        onFinishCallback = null;
    }
}
