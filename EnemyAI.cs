using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    [Header("参照オブジェクト")]
    public Transform player;
    public Transform exitPoint;

    [Header("巡回ポイント設定")]
    public Transform[] patrolPoints;      // Inspector設定用（Transform）
    public Vector3[] patrolPositions;    // 実際の移動用座標
    private int patrolIndex = 0;

    [Header("視界設定")]
    public float viewAngle = 45f;
    public float viewDistance = 10f;

    [Header("行動設定")]
    public float patrolSpeed = 0.5f;
    public float chaseSpeed = 0.5f;
    public float searchSpeed = 0.5f;
    public float exitSpeed = 0.5f;

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
        }
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        UpdateVisibility();
        patrolTimer += Time.deltaTime;

        if (GameManager.isInBookWorld)
        {
            // 兄視点：妹のFootPointをターゲット
            if (GameManager.Instance != null && GameManager.Instance.sisterFootPoint != null)
                player = GameManager.Instance.sisterFootPoint;

            // パトロール更新
            if (state == State.Patrol)
                PatrolUpdate();

            // 妹を見つけたらチェイス
            if (CanSeePlayer())
            {
                SetStateToChase(GameManager.Instance.sisterFootPoint.position);
            }
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
        if (GameManager.isHiding && state != State.Search && state != State.Exit)
        {
            SetStateToSearch(GameManager.Instance.sisterFootPoint.position);
            return;
        }

        // デバッグライン描画
        if (enableDebugLog)
            Debug.DrawLine(transform.position, agent.destination, Color.magenta);
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
        foreach (var r in renderers) r.enabled = !isBook;
        foreach (var c in colliders) c.enabled = !isBook;
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
    }

    public void SetStateToChase()
    {
        if (player == null) return;
        state = State.Chase;
        agent.speed = chaseSpeed;
        agent.destination = player.position;
    }

    // オーバーロード：ターゲット位置を指定して追跡
    public void SetStateToChase(Vector3 targetPos)
    {
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
        if (exitPoint != null)
            agent.destination = exitPoint.position;
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
            GoToNextPatrolPoint();
        }

        if (CanSeePlayer())
            SetStateToChase();
    }

    void ChaseUpdate()
    {
        if (player == null) return;

        if (GameManager.isHiding)
        {
            // 妹が箱に隠れたら即座にSearch状態へ
            SetStateToSearch(GameManager.Instance.sisterFootPoint.position);
            return;
        }

        agent.destination = player.position;
        gizmoTargetPosition = agent.destination;

        if (CanSeePlayer())
            lastKnownPlayerPos = player.position;

        
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
        if (searchTimer > 10f) // 例：10秒探索したらExit
        {
            SetStateToExit();
            return;
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
        if (CanSeePlayer())
        {
            SetStateToChase();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolTimer = 0f;
            searchTimer = 0f;
            state = State.Patrol;
            if (WitchManager.Instance != null)
                WitchManager.Instance.DeactivateWitch();
            gameObject.SetActive(false);
        }
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

        // patrolDestinationSet = true; // 削除
        if (enableDebugLog)
            Debug.Log($"[GoToNextPatrolPoint] 次のパトロールポイントへ: {target}");
    }

    bool CanSeePlayer()
    {
        if (GameManager.isHiding || player == null) return false;

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

        if (renderers != null)
        {
            foreach (var r in renderers)
                if (r != null) r.enabled = visible;
        }

        if (colliders != null)
        {
            foreach (var c in colliders)
                if (c != null) c.enabled = visible;
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
