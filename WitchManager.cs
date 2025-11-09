using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// 魔女の状態列挙
public enum WitchState { Patrol, Chase, Search, Exit }

// WitchManager: 魔女情報を一元管理するDDOLスクリプト
public class WitchManager : MonoBehaviour
{
    public static WitchManager Instance;

    [Header("魔女位置・状態")]
    public Vector3 witchPosition;          // 魔女の現在位置
    public Vector3 sisterPosition;         // 妹の現在位置
    public bool isWitchActive = false;     // 魔女が出現しているか
    public bool witchIsChasing = false;    // 追跡中かどうか
    public WitchState witchState = WitchState.Patrol;

    [Header("魔女関連オブジェクト")]
    public GameObject WitchObj;
    private EnemyAI witchAI;

    [Header("追跡設定")]
    public float witchSpeed = 3f;          // 追跡速度
    public float witchCatchDistance = 0.5f; // 捕獲距離（水平距離）
    public float witchCatchHeight = 1.5f;   // 捕獲できる高さ差の閾値

    [Header("BGM・赤フレーム")]
    public RawImage redFrame;
    public float maxRedAlpha = 0.8f;

    [Header("心音設定（BGM→心音SE化）")]
    public AudioSource heartbeatSource;       // 心音用AudioSource
    public float maxHeartDistance = 5f;      // 心音が聞こえなくなる距離
    public float minHeartDistance = 3f;       // 最大強度の距離
    public float maxHeartVolume = 0.2f;         // 最大音量
    public float minHeartVolume = 0.01f;       // 最小音量
    public float maxHeartPitch = 1.4f;        // 近距離での速い鼓動
    public float minHeartPitch = 0.8f;        // 遠距離でのゆっくり鼓動
    public float heartSmoothSpeed = 2.0f; // 変化スピード（1～3くらいが自然）


    [Header("デバッグ設定")]
    public bool enableDebugBrotherLog = true; // 兄視点ログ
    public bool drawDebugGizmos = true;       // Gizmos表示

    public EnemyAI CurrentWitch { get; private set; }

    public Transform WitchFootPoint =>
        (CurrentWitch != null && CurrentWitch.footPoint != null)
            ? CurrentWitch.footPoint
            : null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (WitchObj != null)
        {
            WitchObj.hideFlags = HideFlags.None;

            DontDestroyOnLoad(WitchObj);
            witchAI = WitchObj.GetComponent<EnemyAI>();
            WitchObj.SetActive(false);

            // PatrolPositions 初期化
            if (witchAI.patrolPoints != null && witchAI.patrolPoints.Length > 0)
            {
                witchAI.patrolPositions = witchAI.patrolPoints
                    .Where(p => p != null)
                    .Select(p => p.position)
                    .ToArray();
            }

            RegisterWitch(witchAI);
        }


        InitializeBGMAndFrame();
    }


    private void InitializeBGMAndFrame()
    {
        // --- 心音SE 初期化 ---
        if (heartbeatSource == null)
        {
            heartbeatSource = gameObject.AddComponent<AudioSource>();
            heartbeatSource.loop = true;                // 常にループ再生
            heartbeatSource.playOnAwake = false;
            heartbeatSource.volume = 0f;
            heartbeatSource.spatialBlend = 0f;          // 2D再生（プレイヤー視点用）
            heartbeatSource.rolloffMode = AudioRolloffMode.Linear;

            AudioClip clip = Resources.Load<AudioClip>("Sounds/Heartbeat04-1(Slow-Reverb)");
            if (clip != null)
            {
                heartbeatSource.clip = clip;
            }
            else
            {
                Debug.LogWarning("心音SEが見つかりません。Resourcesフォルダを確認してください。");
            }
        }


        // --- 赤フレーム初期化 ---
        if (redFrame != null)
        {
            // 透明化
            Color c = redFrame.color;
            c.a = 0f;
            redFrame.color = c;

            // CanvasGroupがある場合はアルファ1固定
            CanvasGroup cg = redFrame.GetComponentInParent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }
    }


    // StartはAwake後に必ず呼ばれるので、Instance確定後の安全なタイミング
    void Start()
    {
        // WitchManagerとEnemyAIのリンクを再確認
        if (witchAI != null)
        {
            // EnemyAI側でRegisterWitchできていなければ、ここで登録
            if (CurrentWitch == null)
            {
                RegisterWitch(witchAI);
                Debug.Log("[WitchManager] Awake順のズレ補正により手動登録しました。");
            }
        }
    }


    void Update()
    {
        if (!isWitchActive || CurrentWitch == null) return;

        HandleWitchFootsteps();

        // 妹位置更新（妹視点時のみ）
        if (!GameManager.isInBookWorld && GameManager.Instance?.sisterFootPoint != null)
            sisterPosition = GameManager.Instance.sisterFootPoint.position;

        // 距離を一括取得
        GetDistanceInfo(out float distanceXZ, out float heightDiff);

        // 視点ごとの音量補正
        float heartbeatVolumeMultiplier = GameManager.isInBookWorld ? 0.5f : 1.0f;

        // 共通演出
        UpdateHeartbeatAndFrame(distanceXZ, heartbeatVolumeMultiplier);

        // 視点ごとのロジック
        if (GameManager.isInBookWorld)
            UpdateBrotherSpecific(distanceXZ, heightDiff);
        else
            UpdateSisterSpecific(distanceXZ, heightDiff);
    }


    // 魔女と妹の距離
    private Vector3 GetSisterPositionForDistance()
    {
        // 「妹は移動しないので最後の位置で固定」が仕様なら、兄視点では保存値を使う
        if (GameManager.isInBookWorld)
            return sisterPosition;

        // 妹視点では実際の sisterFootPoint の座標を優先して使う（存在しない場合は保存値）
        if (GameManager.Instance != null && GameManager.Instance.sisterFootPoint != null)
            return GameManager.Instance.sisterFootPoint.position;

        return sisterPosition;
    }

    // 魔女と妹の距離を「XZ距離＋高さ差」で取得
    private void GetDistanceInfo(out float distanceXZ, out float heightDiff)
    {
        Vector3 witchFoot = (WitchFootPoint != null) ? WitchFootPoint.position : CurrentWitch.transform.position;
        Vector3 sisterFoot = GetSisterPositionForDistance();

        distanceXZ = Vector2.Distance(new Vector2(witchFoot.x, witchFoot.z), new Vector2(sisterFoot.x, sisterFoot.z));
        heightDiff = Mathf.Abs(sisterFoot.y - witchFoot.y);
    }

    // --- 共通：魔女足音制御 ---
    private void HandleWitchFootsteps()
    {
        if (CurrentWitch?.footAudioSource == null) return;
        CurrentWitch.footAudioSource.mute = GameManager.isInBookWorld;
    }

    // --- 共通：心音と赤フレーム更新 ---
    private void UpdateHeartbeatAndFrame(float distanceXZ, float volumeMultiplier)
    {
        // 赤フレーム処理
        if (redFrame != null)
        {
            if (!GameManager.isInBookWorld)
            {
                float t = Mathf.InverseLerp(minHeartDistance, maxHeartDistance, distanceXZ);
                float targetAlpha = Mathf.Lerp(maxRedAlpha, 0f, t);
                Color c = redFrame.color;
                c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 5f);
                redFrame.color = c;
            }
            else
            {
                // 兄視点なら非表示へ
                Color c = redFrame.color;
                c.a = Mathf.Lerp(c.a, 0f, Time.deltaTime * 5f);
                redFrame.color = c;
            }
        }

        // 心音処理
        if (heartbeatSource != null)
        {
            // 距離を 0～1 の範囲に正規化（近いほど0、遠いほど1に近づく）
            float t = Mathf.InverseLerp(minHeartDistance, maxHeartDistance, distanceXZ);

            // 距離が近いほど音量が大きくなる
            float targetVolume = Mathf.Lerp(maxHeartVolume, minHeartVolume, t) * volumeMultiplier;

            // 距離が近いほど鼓動が速く（ピッチが高く）なる
            float targetPitch = Mathf.Lerp(maxHeartPitch, minHeartPitch, t);

            // 現在の音量・ピッチを滑らかに変化させる（急な変化を防ぐ）
            heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, targetVolume, Time.deltaTime * heartSmoothSpeed);
            heartbeatSource.pitch = Mathf.Lerp(heartbeatSource.pitch, targetPitch, Time.deltaTime * heartSmoothSpeed);

            // 心音が再生されていない場合は再生を開始
            if (!heartbeatSource.isPlaying)
                heartbeatSource.Play();
        }
    }


    // --- 共通：捕獲判定 ---
    private bool CheckCatchCondition()
    {
        GetDistanceInfo(out float distanceXZ, out float heightDiff);
        return (distanceXZ <= witchCatchDistance && heightDiff < witchCatchHeight);
    }


    // --- 妹視点専用 ---
    private void UpdateSisterSpecific(float distanceXZ, float heightDiff)
    {
        if (!GameManager.isHiding && CheckCatchCondition())
        {
            GameManager.Instance.TriggerGameOver("魔女に捕まった");
            DeactivateWitch();
        }
    }


    // --- 兄視点専用 ---
    private void UpdateBrotherSpecific(float distanceXZ, float heightDiff)
    {
        if (CurrentWitch == null || CurrentWitch.agent == null) return;

        // 妹の最後の位置（固定）
        Vector3 sisterFoot = sisterPosition;

        // 見えているなら追跡開始
        if (CurrentWitch.CanSeeSisterBrotherView(sisterFoot))
        {
            CurrentWitch.SetStateToChase(sisterFoot);
            if (enableDebugBrotherLog)
                Debug.Log("[BrotherView] 妹を発見、追跡開始");
        }

        // 現在の状態に応じたAI制御
        switch (CurrentWitch.CurrentState)
        {
            case WitchState.Patrol:
                break;
            case WitchState.Chase:
                CurrentWitch.SetStateToChase(sisterFoot);
                break;
            case WitchState.Search:
                CurrentWitch.SetStateToSearch(sisterFoot);
                break;
            case WitchState.Exit:
                CurrentWitch.SetStateToExit();
                break;
        }

        // 兄視点では魔女を非表示にして移動は継続
        CurrentWitch.SetVisible(false);
        CurrentWitch.EnableAgent(true);

        // 捕獲判定
        if (CheckCatchCondition())
        {
            GameManager.Instance.TriggerGameOver("妹が魔女に捕まった…（兄視点）");
            DeactivateWitch();
        }
    }


    // オプション：Gizmosで魔女と妹の最後位置を表示
    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos || CurrentWitch == null) return;

        Vector3 witchPos = (WitchFootPoint != null) ? WitchFootPoint.position : CurrentWitch.transform.position;
        Vector3 sisterPos = sisterPosition;

        // 線の色を距離で変える
        float distXZ = Vector2.Distance(new Vector2(witchPos.x, witchPos.z), new Vector2(sisterPos.x, sisterPos.z));
        Gizmos.color = distXZ <= witchCatchDistance ? Color.yellow : Color.cyan;

        Gizmos.DrawLine(witchPos + Vector3.up * 0.5f, sisterPos + Vector3.up * 0.5f);
        Gizmos.DrawSphere(witchPos + Vector3.up * 0.5f, 0.2f);   // 魔女
        Gizmos.DrawSphere(sisterPos + Vector3.up * 0.5f, 0.2f);  // 妹
    }


    public void ActivateWitch()
    {
        isWitchActive = true;

        if (CurrentWitch == null)
        {
            EnemyAI found = Object.FindFirstObjectByType<EnemyAI>(FindObjectsInactive.Include);
            if (found != null)
            {
                RegisterWitch(found);
                Debug.Log("[WitchManager] CurrentWitchを自動登録しました。");
            }
        }

        if (CurrentWitch == null)
        {
            Debug.LogWarning("魔女が登録されていません。");
            return;
        }

        // 妹FootPoint安全化
        if (GameManager.Instance != null && GameManager.Instance.sisterFootPoint != null)
            sisterPosition = GameManager.Instance.sisterFootPoint.position;

        // 魔女を出現
        CurrentWitch.gameObject.SetActive(true);
        CurrentWitch.SetVisible(true);
        CurrentWitch.EnableAgent(true);
        CurrentWitch.SetStateToPatrol();

        ForceImmediateUpdate();
        isWitchActive = true;
    }



    private void ForceImmediateUpdate()
    {
        if (!isWitchActive || CurrentWitch == null) return;

        Vector3 witchFoot = (WitchFootPoint != null) ? WitchFootPoint.position : CurrentWitch.transform.position;
        Vector3 sisterFoot = (GameManager.Instance != null && GameManager.Instance.sisterFootPoint != null)
            ? GameManager.Instance.sisterFootPoint.position
            : sisterPosition;

        float distanceXZ = Vector2.Distance(
            new Vector2(sisterFoot.x, sisterFoot.z),
            new Vector2(witchFoot.x, witchFoot.z)
        );
        float heightDiff = Mathf.Abs(sisterFoot.y - witchFoot.y);

        // --- 赤フレーム即更新 ---
        if (redFrame != null)
        {
            float t = Mathf.InverseLerp(minHeartDistance, maxHeartDistance, distanceXZ);
            float targetAlpha = Mathf.Lerp(maxRedAlpha, 0f, t);
            Color c = redFrame.color;
            c.a = targetAlpha;
            redFrame.color = c;
        }

        // --- 心音即更新（距離に応じて音量・ピッチ変化） ---
        if (heartbeatSource != null)
        {
            float t = Mathf.InverseLerp(minHeartDistance, maxHeartDistance, distanceXZ);
            heartbeatSource.volume = Mathf.Lerp(maxHeartVolume, minHeartVolume, t);
            heartbeatSource.pitch = Mathf.Lerp(maxHeartPitch, minHeartPitch, t);

            if (!heartbeatSource.isPlaying)
            {
                heartbeatSource.Play();
            }
        }

        Debug.Log("[WitchManager] ForceImmediateUpdate() により初回同期完了");
    }




    public void DeactivateWitch()
    {
        isWitchActive = false;
        witchIsChasing = false;

        if (heartbeatSource != null) heartbeatSource.Stop();
        if (redFrame != null)
        {
            Color c = redFrame.color;
            c.a = 0f;
            redFrame.color = c;
        }

        if (CurrentWitch != null)
        {
            CurrentWitch.ForceDeactivate();
        }
    }

    public void UpdateWitchStateFromEnemy(Vector3 pos, WitchState state, bool isChasing)
    {
        witchPosition = pos;
        witchState = state;
        witchIsChasing = isChasing;
    }

    public void UpdateSisterPosition(Vector3 pos)
    {
        sisterPosition = pos;
    }

    public void ResetForSisterScene()
    {
        isWitchActive = true;
        if (CurrentWitch != null)
        {
            CurrentWitch.gameObject.SetActive(true);
            CurrentWitch.SetVisible(true);
            CurrentWitch.EnableAgent(true);
        }
    }

    public void StopHeartbeat()
    {
        if (heartbeatSource != null && heartbeatSource.isPlaying)
        {
            heartbeatSource.Stop();
        }
    }


    public void RegisterWitch(EnemyAI witch)
    {
        if (witch == null) return;

        // まだ登録が無ければ登録する。既に別のオブジェクトが登録済みであれば基本は上書きしない（ログのみ）
        if (CurrentWitch == null)
        {
            CurrentWitch = witch;
            Debug.Log($"[WitchManager] Registered witch: {CurrentWitch.name}");
        }
        else if (CurrentWitch != witch)
        {
            // 既に別のインスタンスが登録済み → シーンコピーが登録しようとしている可能性
            Debug.LogWarning($"[WitchManager] RegisterWitch ignored: another witch already registered ({CurrentWitch.name}). New: {witch.name}");
        }
    }



    public void SetVisible(bool visible)
    {
        if (CurrentWitch == null) return;
        CurrentWitch.SetVisible(visible);
    }

    public void StopAllSounds()
    {
        // --- 魔女の足音停止 ---
        if (CurrentWitch != null && CurrentWitch.footAudioSource != null)
        {
            CurrentWitch.footAudioSource.Stop();
        }

        // --- プレイヤーの足音停止 ---
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.StopFootsteps(); // ← ここで安全に停止できる
            }
        }

        // --- 心音停止 ---
        if (heartbeatSource != null && heartbeatSource.isPlaying)
        {
            heartbeatSource.Stop();
        }

        Debug.Log("[WitchManager] StopAllSounds(): 魔女・プレイヤーの足音、心音をすべて停止しました。");
    }


}
