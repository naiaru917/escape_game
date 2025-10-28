using System.Linq;
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
    public AudioSource witchBGM;
    public RawImage redFrame;
    public float maxSoundDistance = 20f;
    public float minSoundDistance = 3f;
    public float maxRedAlpha = 0.8f;

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
        // --- BGM 初期化 ---
        if (witchBGM == null)
        {
            witchBGM = gameObject.AddComponent<AudioSource>();
            witchBGM.loop = true;
            witchBGM.playOnAwake = false;
            witchBGM.volume = 0f;
            AudioClip clip = Resources.Load<AudioClip>("Objects/旅のさがしもの");
            if (clip != null) witchBGM.clip = clip;
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

        // 魔女FootPoint基準
        Vector3 witchFoot = (WitchFootPoint != null) ? WitchFootPoint.position : CurrentWitch.transform.position;

        // 妹の位置を常に更新（妹視点）
        if (!GameManager.isInBookWorld && GameManager.Instance != null && GameManager.Instance.sisterFootPoint != null)
        {
            sisterPosition = GameManager.Instance.sisterFootPoint.position;
        }

        if (!GameManager.isInBookWorld)
        {
            UpdateSisterWorldBehavior(witchFoot);
        }
        else
        {
            UpdateBrotherWorldBehavior(witchFoot);
        }

    }

    private void UpdateSisterWorldBehavior(Vector3 witchFoot)
    {
        if (GameManager.isHiding)
        {
            // 魔女がChase中ならSearchに変更
            if (CurrentWitch != null && CurrentWitch.CurrentState == WitchState.Chase)
            {
                CurrentWitch.SetStateToSearch(GameManager.Instance.sisterFootPoint.position);
            }

            // 捕獲や距離判定をスキップ
            return;
        }

        Vector3 sisterFoot = (GameManager.Instance != null && GameManager.Instance.sisterFootPoint != null)
            ? GameManager.Instance.sisterFootPoint.position
            : sisterPosition;

        float distanceXZ = Vector2.Distance(
            new Vector2(sisterFoot.x, sisterFoot.z),
            new Vector2(witchFoot.x, witchFoot.z)
        );
        float heightDiff = Mathf.Abs(sisterFoot.y - witchFoot.y);

        // 赤フレーム更新
        if (redFrame != null)
        {
            float t = Mathf.InverseLerp(minSoundDistance, maxSoundDistance, distanceXZ);
            float targetAlpha = Mathf.Lerp(maxRedAlpha, 0f, t);
            Color c = redFrame.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 5f);
            redFrame.color = c;
        }

        // BGM更新
        if (witchBGM != null)
        {
            float soundT = Mathf.InverseLerp(minSoundDistance, maxSoundDistance, distanceXZ);
            witchBGM.volume = Mathf.Lerp(1f, 0.2f, soundT);
            if (!witchBGM.isPlaying) witchBGM.Play();
        }

        // 捕獲判定
        if (!GameManager.isHiding) // ← 隠れている場合はスキップ
        {
            if (distanceXZ <= witchCatchDistance && heightDiff < witchCatchHeight)
            {
                GameManager.Instance.TriggerGameOver("魔女に捕まった");
                DeactivateWitch();
            }
        }
        else
        {
            // 隠れている場合、もし魔女がまだChase中なら → Search状態に遷移させる
            if (CurrentWitch != null && CurrentWitch.CurrentState != WitchState.Exit)
            {
                CurrentWitch.SetStateToSearch(GameManager.Instance.sisterFootPoint.position);
            }
        }

        //Debug.Log($"[DistanceCheck] DistanceXZ={distanceXZ:F2}, HeightDiff={heightDiff:F2}, WitchActive={isWitchActive}, WitchFoot={witchFoot}, SisterFoot={sisterFoot}");

    }

    private void UpdateBrotherWorldBehavior(Vector3 witchFoot)
    {
        if (CurrentWitch == null || CurrentWitch.agent == null) return;

        Vector3 sisterFoot = sisterPosition; // 妹の最後FootPoint

        // 発見判定
        if (CurrentWitch.CanSeeSisterBrotherView(sisterFoot))
        {
            // 発見した場合は追跡状態に変更
            CurrentWitch.SetStateToChase(sisterFoot);
            Debug.Log("[BrotherView] 妹を発見、追跡開始");
        }

        // 状態復元（Patrolは兄視点で無効）
        switch (CurrentWitch.CurrentState)
        {
            case WitchState.Patrol: break;
            case WitchState.Chase: CurrentWitch.SetStateToChase(sisterFoot); break;
            case WitchState.Search: CurrentWitch.SetStateToSearch(sisterFoot); break;
            case WitchState.Exit: CurrentWitch.SetStateToExit(); break;
        }

        CurrentWitch.SetVisible(false);
        CurrentWitch.EnableAgent(true);

        float distanceXZ = Vector2.Distance(
            new Vector2(sisterFoot.x, sisterFoot.z),
            new Vector2(witchFoot.x, witchFoot.z)
        );
        float heightDiff = Mathf.Abs(sisterFoot.y - witchFoot.y);

        // デバッグログ
        if (enableDebugBrotherLog)
        {
            Debug.Log($"[BrotherView Debug] DistanceXZ={distanceXZ:F2}, HeightDiff={heightDiff:F2}, WitchState={CurrentWitch.CurrentState}, WitchChasing={CurrentWitch.IsChasing}");
        }

        if (distanceXZ <= witchCatchDistance && heightDiff < witchCatchHeight)
        {
            Debug.Log("[BrotherView Debug] 妹が魔女に捕まった！");
            GameManager.Instance.TriggerGameOver("妹が魔女に捕まった…（兄視点）");
            DeactivateWitch();
        }

        // 赤フレーム非表示
        if (redFrame != null)
        {
            Color c = redFrame.color;
            c.a = Mathf.Lerp(c.a, 0f, Time.deltaTime * 5f);
            redFrame.color = c;
        }

        // BGM低音量再生
        if (witchBGM != null)
        {
            witchBGM.volume = Mathf.MoveTowards(witchBGM.volume, 0.25f, Time.deltaTime * 0.5f);
            if (!witchBGM.isPlaying) witchBGM.Play();
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
            float t = Mathf.InverseLerp(minSoundDistance, maxSoundDistance, distanceXZ);
            float targetAlpha = Mathf.Lerp(maxRedAlpha, 0f, t);
            Color c = redFrame.color;
            c.a = targetAlpha;
            redFrame.color = c;
        }

        // --- BGM即更新 ---
        if (witchBGM != null)
        {
            float soundT = Mathf.InverseLerp(maxSoundDistance, minSoundDistance, distanceXZ);
            witchBGM.volume = Mathf.Lerp(0.2f, 1f, 1f - soundT);
        }

        Debug.Log("[WitchManager] ForceImmediateUpdate() により初回同期完了");
    }




    public void DeactivateWitch()
    {
        isWitchActive = false;
        witchIsChasing = false;

        if (witchBGM != null) witchBGM.Stop();
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

    public void StopWitchBGM()
    {
        if (witchBGM != null && witchBGM.isPlaying)
        {
            witchBGM.Stop();
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
}
