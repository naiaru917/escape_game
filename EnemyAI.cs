using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("参照オブジェクト")]
    public Transform player;

    [Header("巡回ポイント設定")]
    public Transform[] patrolPoints;      // Inspector設定用（Transform）
    public Vector3[] patrolPositions;    // 実際の移動用座標
    private int patrolIndex = 0;
    private int lastPatrolIndexBeforeChase = 0; // 追跡前の巡回インデックスを保存
    private Vector3? savedDestination = null; // 現在の目的地を保存
    private bool isExiting = false; // Exit処理中かどうかのフラグ

    [Header("退出ポイント設定")]
    public Transform exitPoint;       // 妹視点で設定される実オブジェクト
    public Vector3 exitPosition;      // 兄視点用に保持する座標値（妹視点開始時にコピー）

    [Header("視界設定")]
    public float viewAngle = 45f;
    public float viewDistance = 10f;

    [Header("行動設定")]
    public float patrolSpeed = 0.5f;
    public float chaseSpeed = 0.5f;
    public float searchSpeed = 0.5f;
    public float exitSpeed = 0.5f;

    [Header("音声設定")]
    public AudioSource footAudioSource;  // 足音用AudioSource
    public AudioClip witchFootClip;      // 再生する足音クリップ
    [Range(0f, 1f)] public float footstepVolume = 0.1f;

    // 足音ピッチ設定
    private float patrolPitch = 1f;
    private float chasePitch = 2f;
    private float searchPitch = 1.5f;
    private float exitPitch = 1f;

    public NavMeshAgent agent;
    private Renderer[] renderers;
    private Collider[] colliders;

    private float patrolTimer = 0f;
    private float searchTimer = 0f;
    private Vector3 lastKnownPlayerPos;

    private bool initialized = false;
    private enum State { Patrol, Chase, Search, Exit }
    private State state = State.Patrol;

    public Transform footPoint;
    private bool skipNextPatrolUpdate = false; // 次のパトロールポイント更新をスキップする


    private bool wasSeeingPlayerRecently = false;
    private float lastSeeTime = -999f;

    // 箱探索中かどうかを管理
    private bool isHideSearch = false;
    private bool wasHidingLastFrame = false;



    public bool WasSeeingPlayerRecently => wasSeeingPlayerRecently;

    [Header("デバッグ設定")]
    public bool enableDebugLog = true;
    public bool drawDebugGizmos = true;
    private Vector3 gizmoTargetPosition;

    public bool IsChasing
    {
        get { return state == State.Chase; }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        SceneManager.sceneLoaded += OnSceneLoaded;

        SetupFootAudio();
    }
    private void Start()
    {
        // ExitPoint 座標取得
        if (exitPoint != null)
        {
            exitPosition = exitPoint.position;
            if (enableDebugLog)
                Debug.Log($"[EnemyAI] ExitPoint座標を保存(Start): {exitPosition}");
        }

        // PatrolPoint 座標取得
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            patrolPositions = new Vector3[patrolPoints.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                    patrolPositions[i] = patrolPoints[i].position;
            }
        }
    }



    private void SetupFootAudio()
    {
        if (footPoint == null)
            footPoint = transform; // 念のため安全策

        if (footAudioSource == null)
        {
            footAudioSource = footPoint.gameObject.AddComponent<AudioSource>();
            footAudioSource.playOnAwake = false;
            footAudioSource.loop = true;
            footAudioSource.spatialBlend = 1.0f;       // 完全3D
            footAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            footAudioSource.maxDistance = 15f;
            footAudioSource.dopplerLevel = 0.5f;
            footAudioSource.volume = footstepVolume;
        }

        if (witchFootClip != null)
            footAudioSource.clip = witchFootClip;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetAgent();
        UpdateVisibility();
        RefreshPlayerReference();

        // シーンロード直後は次のパトロールポイント更新をスキップ
        skipNextPatrolUpdate = true;

        // 妹視点かつ WitchManager がアクティブなら再表示
        if (!GameManager.isInBookWorld && WitchManager.Instance != null && WitchManager.Instance.isWitchActive)
        {
            SetVisible(true);
        }

        // 妹視点でExitPointの座標を保存
        if (!GameManager.isInBookWorld && exitPoint != null)
        {
            exitPosition = exitPoint.position;
        }


        // 巡回中の目的地を復元
        RestoreDestination();
    }

    private void OnEnable()
    {
        if (!initialized)
        {
            initialized = true;
            RefreshPlayerReference();
            agent.speed = patrolSpeed;

            // 視点切替直後は次のパトロールポイント更新をスキップ
            skipNextPatrolUpdate = true;

            SetVisible(!GameManager.isInBookWorld);

            // 足音の制御
            UpdateFootAudioVisibility();

            // 巡回中の目的地を復元
            RestoreDestination();
        }
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        UpdateFootstepPitch();
        UpdateVisibility();
        patrolTimer += Time.deltaTime;

        if (GameManager.isInBookWorld)
        {
            // 兄視点：妹のFootPointをターゲット
            if (GameManager.Instance != null && GameManager.Instance.sisterFootPoint != null)
                player = GameManager.Instance.sisterFootPoint;

            // 兄視点でもExit処理を行う
            switch (state)
            {
                case State.Patrol:
                    PatrolUpdate();
                    break;
                case State.Exit:
                    ExitUpdate();
                    break;
                case State.Chase:
                    ChaseUpdate(); // optional（追跡させたい場合）
                    break;
                case State.Search:
                    SearchUpdate();
                    break;
            }

            // 妹を見つけたらチェイス
            if (CanSeePlayer())
                SetStateToChase(GameManager.Instance.sisterFootPoint.position);
        }

        else
        {
            // 妹視点：通常のPlayerターゲット
            player = GameObject.FindWithTag("Player")?.transform;
            switch (state)
            {
                case State.Patrol:
                    PatrolUpdate();
                    break;
                case State.Chase:
                    ChaseUpdate();
                    break;
                case State.Search:
                    // 箱探索中は通常のSearchUpdateをスキップ
                    if (!isHideSearch)
                        SearchUpdate();
                    break;
                case State.Exit:
                    ExitUpdate();
                    break;
            }
        }

        // 妹視点で WitchManager に状態更新
        if (!GameManager.isInBookWorld && WitchManager.Instance != null)
            WitchManager.Instance.UpdateWitchStateFromEnemy(transform.position, CurrentState, state == State.Chase);

        // 箱に隠れたら探索状態に
        bool isCurrentlyHiding = GameManager.isHiding;

        if (isCurrentlyHiding && !wasHidingLastFrame)
        {
            Vector3 hidePos = GameManager.Instance.sisterFootPoint.position;

            // Exit中でも箱探索を優先（Chase中だけは除外）
            if (state != State.Chase)
            {
                if (!isHideSearch)
                {
                    if (enableDebugLog)
                        Debug.Log($"[EnemyAI] 隠れ検知: 状態={state} → 箱位置({hidePos})を探索に上書き");

                    isHideSearch = true;
                    StopAllCoroutines();

                    state = State.Search;
                    agent.speed = searchSpeed;
                    agent.SetDestination(hidePos);
                    lastKnownPlayerPos = hidePos;

                    StartCoroutine(HideSearchRoutine());
                }
            }
        }

        // 現在の隠れ状態を次フレームに引き継ぐ
        wasHidingLastFrame = isCurrentlyHiding;

        // デバッグライン
        if (enableDebugLog)
            Debug.DrawLine(transform.position, agent.destination, Color.magenta);
    }

    // 探索状態を維持する簡易コルーチン
    private IEnumerator HideSearchRoutine()
    {
        if (enableDebugLog)
            Debug.Log("[EnemyAI] 箱探索開始。箱へ移動中…");

        // 状態をSearchに設定
        state = State.Search;
        agent.speed = searchSpeed;

        // 箱位置を目的地に設定
        Vector3 hidePos = GameManager.Instance.sisterFootPoint.position;
        agent.SetDestination(hidePos);

        // 箱まで移動中
        while (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            // プレイヤーを発見したら追跡に切り替え
            if (CanSeePlayer())
            {
                if (enableDebugLog)
                    Debug.Log("[EnemyAI] 箱に向かう途中でプレイヤーを発見 → 追跡再開");
                SetStateToChase(GameManager.Instance.sisterFootPoint.position);
                isHideSearch = false;
                yield break;
            }

            yield return null;
        }

        // 箱に到達！
        if (enableDebugLog)
            Debug.Log("[EnemyAI] 箱に到達。5秒間探索開始。");

        // 探索時間
        float duration = 5f;
        float timer = 0f;

        while (timer < duration)
        {
            // 探索中に発見したら追跡へ
            if (CanSeePlayer())
            {
                if (enableDebugLog)
                    Debug.Log("[EnemyAI] 箱探索中にプレイヤーを発見 → 追跡へ切り替え");
                SetStateToChase(GameManager.Instance.sisterFootPoint.position);
                isHideSearch = false;
                yield break;
            }

            // 周辺をランダムにうろうろ（半径2m）
            if (!agent.pathPending && agent.remainingDistance < 0.3f)
            {
                Vector3 randomOffset = Random.insideUnitSphere * 2f;
                randomOffset.y = 0f;
                Vector3 randomPoint = hidePos + randomOffset;

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (enableDebugLog)
            Debug.Log("[EnemyAI] 箱探索完了 → Exitへ移行。");

        isHideSearch = false;

        // Exit地点に移動命令を出す
        if (exitPoint != null)
        {
            SetStateToExit();
            agent.SetDestination(exitPosition);
            if (enableDebugLog)
                Debug.Log($"[EnemyAI] Exit地点({exitPoint.position})へ移動開始。");
        }
        else
        {
            if (enableDebugLog)
                Debug.LogWarning("[EnemyAI] ExitPointが未設定。非アクティブ化のみ実行。");
            state = State.Exit;
            gameObject.SetActive(false);
        }
    }


    public void RefreshPlayerReference()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void ResetAgent()
    {
        agent.enabled = false;
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            transform.position = hit.position;
        agent.enabled = true;
    }

    private void UpdateVisibility()
    {
        bool isBook = GameManager.isInBookWorld;

        // 表示は Renderer のみ操作（兄視点では不可視にしても物理/Agentは動かすため）
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        foreach (var r in renderers)
            if (r != null)
                r.enabled = !isBook;
    }

    public WitchState CurrentState
    {
        get
        {
            switch (state)
            {
                case State.Patrol: return WitchState.Patrol;
                case State.Chase: return WitchState.Chase;
                case State.Search: return WitchState.Search;
                case State.Exit: return WitchState.Exit;
                default: return WitchState.Patrol;
            }
        }
    }

    public void SetStateToPatrol()
    {
        state = State.Patrol;
        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();

        WitchFootSound();
    }

    private void WitchFootSound()
    {
        // 兄視点（BookWorld）なら魔女の足音を再生しない
        if (GameManager.isInBookWorld)
            return;

        if (footAudioSource != null && witchFootClip != null)
        {
            // AudioClip（音源）が設定されていない、または異なっていたら再設定
            if (footAudioSource.clip != witchFootClip)
                footAudioSource.clip = witchFootClip;

            // まだ足音が再生されていない場合のみ再生を開始
            if (!footAudioSource.isPlaying)
                footAudioSource.Play();
        }
    }

    private void UpdateFootAudioVisibility()
    {
        if (footAudioSource == null) return;

        if (GameManager.isInBookWorld) // 兄視点
        {
            // 再生中の音も確実に停止
            if (footAudioSource.isPlaying)
                footAudioSource.Stop();
        }
        else
        {
            // 妹視点では再生許可（必要に応じて再生開始）
            WitchFootSound();
        }
    }

    // 足音ピッチ調整
    private void UpdateFootstepPitch()
    {
        if (footAudioSource == null || !footAudioSource.isPlaying)
            return;

        // 現在ステートに応じた基準速度を決定
        switch (state)
        {
            case State.Patrol:
                footAudioSource.pitch = patrolPitch;
                break;
            case State.Chase:
                footAudioSource.pitch = chasePitch;
                break;
            case State.Search:
                footAudioSource.pitch = searchPitch;
                break;
            case State.Exit:
                footAudioSource.pitch = exitPitch;
                break;
        }
    }

    public void SetStateToChase()
    {
        if (player == null) return;

        // 追跡開始前の巡回インデックスを記録
        lastPatrolIndexBeforeChase = patrolIndex;

        state = State.Chase;
        agent.speed = chaseSpeed;
        agent.destination = player.position;
    }

    // オーバーロード：ターゲット位置を指定して追跡
    public void SetStateToChase(Vector3 targetPos)
    {
        lastPatrolIndexBeforeChase = patrolIndex;
        state = State.Chase;
        agent.speed = chaseSpeed;
        agent.destination = targetPos;
    }

    public void SetStateToSearch(Vector3 searchPos)
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }

        state = State.Search;
        agent.speed = searchSpeed;
        agent.destination = searchPos;
        lastKnownPlayerPos = searchPos;
        searchTimer = 0f;
    }

    public void SetStateToExit()
    {
        state = State.Exit;
        agent.speed = exitSpeed;

        if (exitPosition != Vector3.zero)
        {
            agent.destination = exitPosition;
            if (enableDebugLog)
                Debug.Log($"[EnemyAI] Exit開始: 目的地={exitPosition}");
        }
        else
        {
            Debug.LogWarning("[EnemyAI] ExitPosition未設定 → 非アクティブ化");
            StartCoroutine(DeactivateAfterDelay(0.5f));
        }
    }



    void PatrolUpdate()
    {
        if (skipNextPatrolUpdate)
        {
            skipNextPatrolUpdate = false;
            if (enableDebugLog) Debug.Log("[PatrolUpdate] シーンロード後スキップ: patrolIndexは進まない");
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPositions.Length;

            // 一巡完了したらExitへ
            if (patrolIndex == 0)
            {
                if (enableDebugLog)
                    Debug.Log("[EnemyAI] 巡回一周完了。Exitへ移行。");
                SetStateToExit();
                return;
            }

            GoToNextPatrolPoint();
        }

        if (CanSeePlayer())
            SetStateToChase();
    }

    void ChaseUpdate()
    {
        if (player == null) return;

        // 先に視界をチェック（隠れていても判定）
        bool isPlayerVisible = CanSeePlayer(true);

        if (GameManager.isHiding)
        {
            if (isPlayerVisible)
            {
                // 視界内で箱に隠れた → 捕獲ムービー
                if (enableDebugLog)
                    Debug.Log("[EnemyAI] 視界内で箱に隠れました。捕獲イベントを発生。");
                GameManager.Instance.TriggerCaughtEvent();
            }
            else
            {
                // 視界外で箱に隠れた → 音を聞いて探索→退出
                if (enableDebugLog)
                    Debug.Log("[EnemyAI] 箱に隠れた音を検知。周辺探索→退出。");

                Vector3 hidePos = GameManager.Instance.sisterFootPoint.position;
                SetStateToSearch(hidePos);
                StartCoroutine(SearchAndExitAfterDelay(5f));
            }
            return;
        }

        // 通常の追跡処理
        agent.destination = player.position;
        gizmoTargetPosition = agent.destination;

        if (CanSeePlayer())
        {
            lastSeeTime = Time.time;
            wasSeeingPlayerRecently = true;
            lastKnownPlayerPos = player.position;
        }
        else
        {
            if (Time.time - lastSeeTime > 0.5f)
                wasSeeingPlayerRecently = false; // 0.5秒間見失ったらフラグリセット

            if (enableDebugLog)
                Debug.Log("[EnemyAI] プレイヤーを見失いました。Search状態へ移行します。");
            SetStateToSearch(lastKnownPlayerPos);
            return;
        }
    }

    // Search状態を維持してからExitに移行
    private IEnumerator SearchAndExitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enableDebugLog)
            Debug.Log("[EnemyAI] 探索完了。退出します。");

        SetStateToExit();
    }


    void SearchUpdate()
    {
        searchTimer += Time.deltaTime;

        if (CanSeePlayer())
        {
            SetStateToChase();
            return;
        }

        // 一定時間経過で退出
        if (searchTimer > 10f)
        {
            if (enableDebugLog)
                Debug.Log("[EnemyAI] 探索時間終了。発見前の巡回ポイントへ復帰します。");

            if (GameManager.isHiding)
            {
                // 箱に隠れていた場合はExitへ
                if (enableDebugLog)
                    Debug.Log("[EnemyAI] 隠れ中に探索終了 → Exit状態へ移行");
                SetStateToExit();
                return;
            }
            else
            {
                // 通常の探索終了時はパトロールに戻る
                patrolIndex = lastPatrolIndexBeforeChase;

                if (patrolIndex < 0 || patrolIndex >= patrolPositions.Length)
                    patrolIndex = 0;

                SetStateToPatrol();
                return;
            }
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 3f;
            randomOffset.y = 0;
            Vector3 randomPoint = lastKnownPlayerPos + randomOffset;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                agent.destination = hit.position;
        }
    }

    void ExitUpdate()
    {
        Vector3 targetPos = exitPosition;

        if (!isExiting)
        {
            agent.speed = exitSpeed;
            agent.SetDestination(targetPos);
            isExiting = true;

            if (enableDebugLog)
                Debug.Log($"[EnemyAI] Exit開始。目的地={targetPos}");
        }

        if (isExiting && !agent.pathPending)
        {
            float distanceToExit = Vector3.Distance(transform.position, targetPos);
            if (enableDebugLog)
                Debug.Log($"[EnemyAI] Exit中… 残距離={distanceToExit:F2}");

            if (distanceToExit < 0.3f)
            {
                if (enableDebugLog)
                    Debug.Log($"[EnemyAI] Exit地点到達 → 非アクティブ化開始");
                StartCoroutine(DeactivateAfterDelay(0.5f));
                patrolIndex = 0;
                isExiting = false;
            }
        }
    }



    private IEnumerator DeactivateAfterDelay(float delay)
    {
        if (enableDebugLog)
            Debug.Log($"[EnemyAI] Exit完了。{delay}秒後に非アクティブ化します。");

        yield return new WaitForSeconds(delay);

        if (WitchManager.Instance != null)
            WitchManager.Instance.DeactivateWitch();

        gameObject.SetActive(false);
    }


    void GoToNextPatrolPoint()
    {
        if (patrolPositions == null || patrolPositions.Length == 0)
        {
            Debug.LogWarning("[EnemyAI] 巡回ポイントが設定されていません。");
            return;
        }

        Vector3 target = patrolPositions[patrolIndex];
        agent.destination = target;
        gizmoTargetPosition = target;

        patrolIndex = (patrolIndex + 1) % patrolPositions.Length;

        savedDestination = target;  // 現在の目的地を保存

        if (enableDebugLog)
            Debug.Log($"[GoToNextPatrolPoint] 次のパトロールポイントへ: {target}");
    }
    public void RestoreDestination()
    {
        if (savedDestination.HasValue && agent != null && agent.isOnNavMesh)
        {
            // Agent が停止していたら動かす
            if (agent.isStopped)
                agent.isStopped = false;

            agent.enabled = true;
            agent.SetDestination(savedDestination.Value);

            if (enableDebugLog)
                Debug.Log($"[EnemyAI] 目的地を復元しました: {savedDestination.Value}");
        }

    }

    bool CanSeePlayer(bool ignoreHiding = false)
    {
        if (!ignoreHiding && (GameManager.isHiding || player == null))
            return false;

        if (player == null) return false;

        Vector3 dir = player.position - transform.position;
        float dist = dir.magnitude;

        if (dist > viewDistance) return false;
        if (Vector3.Angle(transform.forward, dir) > viewAngle) return false;

        if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, out RaycastHit hit, viewDistance, ~LayerMask.GetMask("Enemy")))
            return hit.transform == player;

        return false;
    }


    public bool CanSeeSisterBrotherView(Vector3 sisterPos)
    {
        if (GameManager.isHiding) return false;

        Vector3 dir = sisterPos - transform.position;
        float dist = dir.magnitude;

        // 距離チェック
        if (dist > viewDistance) return false;

        // 角度チェック
        if (Vector3.Angle(transform.forward, dir) > viewAngle) return false;

        // Raycastで障害物判定（敵レイヤーは無視）
        if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, out RaycastHit hit, viewDistance, ~LayerMask.GetMask("Enemy")))
        {
            // ヒットした位置が妹の最後の座標に近ければ発見
            if (Vector3.Distance(hit.point, sisterPos) < 0.5f)
                return true;
            else
                return false;
        }

        return true; // 障害物なしで見通せる場合も発見とする
    }

    public void ForceDeactivate(bool notifyWitchManager = true)
    {
        agent.isStopped = true;
        SetVisible(false);
        state = State.Patrol;

        if (notifyWitchManager && WitchManager.Instance != null)
            WitchManager.Instance.isWitchActive = false;
    }

    public void SetVisible(bool visible)
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);
        if (colliders == null || colliders.Length == 0)
            colliders = GetComponentsInChildren<Collider>(true);

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        if (renderers != null)
        {
            foreach (var r in renderers)
                if (r != null) r.enabled = visible;
        }

        if (enableDebugLog)
            Debug.Log($"[EnemyAI] Visible={visible}");
    }

    public void EnableAgent(bool enable)
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.isStopped = !enable;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, gizmoTargetPosition);
        Gizmos.DrawWireSphere(gizmoTargetPosition, 0.2f);

        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, left * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);
    }
}
