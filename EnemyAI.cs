using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    [Header("�Q�ƃI�u�W�F�N�g")]
    public Transform player;
    public Transform exitPoint;

    [Header("����|�C���g�ݒ�")]
    public Transform[] patrolPoints;      // Inspector�ݒ�p�iTransform�j
    public Vector3[] patrolPositions;    // ���ۂ̈ړ��p���W
    private int patrolIndex = 0;

    [Header("���E�ݒ�")]
    public float viewAngle = 45f;
    public float viewDistance = 10f;

    [Header("�s���ݒ�")]
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
    private bool skipNextPatrolUpdate = false; // ���̃p�g���[���|�C���g�X�V���X�L�b�v����


    [Header("�f�o�b�O�ݒ�")]
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

        // �V�[�����[�h����͎��̃p�g���[���|�C���g�X�V���X�L�b�v
        skipNextPatrolUpdate = true;

        // �����_���� WitchManager ���A�N�e�B�u�Ȃ�ĕ\��
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

            // ���_�ؑ֒���͎��̃p�g���[���|�C���g�X�V���X�L�b�v
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
            // �Z���_�F����FootPoint���^�[�Q�b�g
            if (GameManager.Instance != null && GameManager.Instance.sisterFootPoint != null)
                player = GameManager.Instance.sisterFootPoint;

            // �p�g���[���X�V
            if (state == State.Patrol)
                PatrolUpdate();

            // ������������`�F�C�X
            if (CanSeePlayer())
            {
                SetStateToChase(GameManager.Instance.sisterFootPoint.position);
            }
        }
        else
        {
            // �����_�F�ʏ��Player�^�[�Q�b�g
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

        // �����_�� WitchManager �ɏ�ԍX�V
        if (!GameManager.isInBookWorld && WitchManager.Instance != null)
            WitchManager.Instance.UpdateWitchStateFromEnemy(transform.position, CurrentState, state == State.Chase);

        // ���ɉB�ꂽ��T����Ԃ�
        if (GameManager.isHiding && state != State.Search && state != State.Exit)
        {
            SetStateToSearch(GameManager.Instance.sisterFootPoint.position);
            return;
        }

        // �f�o�b�O���C���`��
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

    // �I�[�o�[���[�h�F�^�[�Q�b�g�ʒu���w�肵�Ēǐ�
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
            if (enableDebugLog) Debug.Log("[PatrolUpdate] �V�[�����[�h��X�L�b�v: patrolIndex�͐i�܂Ȃ�");
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
            // �������ɉB�ꂽ�瑦����Search��Ԃ�
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

        // ��莞�Ԍo�߂őޏo
        if (searchTimer > 10f) // ��F10�b�T��������Exit
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
            Debug.LogWarning("[EnemyAI] ����|�C���g���ݒ肳��Ă��܂���B");
            return;
        }

        Vector3 target = patrolPositions[patrolIndex];
        agent.destination = target;
        gizmoTargetPosition = target;

        patrolIndex = (patrolIndex + 1) % patrolPositions.Length;

        // patrolDestinationSet = true; // �폜
        if (enableDebugLog)
            Debug.Log($"[GoToNextPatrolPoint] ���̃p�g���[���|�C���g��: {target}");
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

        // �����`�F�b�N
        if (dist > viewDistance) return false;

        // �p�x�`�F�b�N
        if (Vector3.Angle(transform.forward, dir) > viewAngle) return false;

        // Raycast�ŏ�Q������i�G���C���[�͖����j
        if (Physics.Raycast(transform.position + Vector3.up, dir.normalized, out RaycastHit hit, viewDistance, ~LayerMask.GetMask("Enemy")))
        {
            // �q�b�g�����ʒu�����̍Ō�̍��W�ɋ߂���Δ���
            if (Vector3.Distance(hit.point, sisterPos) < 0.5f)
                return true;
            else
                return false;
        }

        return true; // ��Q���Ȃ��Ō��ʂ���ꍇ�������Ƃ���
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
