using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// �����̏�ԗ�
public enum WitchState { Patrol, Chase, Search, Exit }

// WitchManager: ���������ꌳ�Ǘ�����DDOL�X�N���v�g
public class WitchManager : MonoBehaviour
{
    public static WitchManager Instance;

    [Header("�����ʒu�E���")]
    public Vector3 witchPosition;          // �����̌��݈ʒu
    public Vector3 sisterPosition;         // ���̌��݈ʒu
    public bool isWitchActive = false;     // �������o�����Ă��邩
    public bool witchIsChasing = false;    // �ǐՒ����ǂ���
    public WitchState witchState = WitchState.Patrol;

    [Header("�����֘A�I�u�W�F�N�g")]
    public GameObject WitchObj;
    private EnemyAI witchAI;

    [Header("�ǐՐݒ�")]
    public float witchSpeed = 3f;          // �ǐՑ��x
    public float witchCatchDistance = 0.5f; // �ߊl�����i���������j
    public float witchCatchHeight = 1.5f;   // �ߊl�ł��鍂������臒l

    [Header("BGM�E�ԃt���[��")]
    public AudioSource witchBGM;
    public RawImage redFrame;
    public float maxSoundDistance = 20f;
    public float minSoundDistance = 3f;
    public float maxRedAlpha = 0.8f;

    [Header("�f�o�b�O�ݒ�")]
    public bool enableDebugBrotherLog = true; // �Z���_���O
    public bool drawDebugGizmos = true;       // Gizmos�\��

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

            // PatrolPositions ������
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
        // --- BGM ������ ---
        if (witchBGM == null)
        {
            witchBGM = gameObject.AddComponent<AudioSource>();
            witchBGM.loop = true;
            witchBGM.playOnAwake = false;
            witchBGM.volume = 0f;
            AudioClip clip = Resources.Load<AudioClip>("Objects/���̂���������");
            if (clip != null) witchBGM.clip = clip;
        }

        // --- �ԃt���[�������� ---
        if (redFrame != null)
        {
            // ������
            Color c = redFrame.color;
            c.a = 0f;
            redFrame.color = c;

            // CanvasGroup������ꍇ�̓A���t�@1�Œ�
            CanvasGroup cg = redFrame.GetComponentInParent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }
    }


    // Start��Awake��ɕK���Ă΂��̂ŁAInstance�m���̈��S�ȃ^�C�~���O
    void Start()
    {
        // WitchManager��EnemyAI�̃����N���Ċm�F
        if (witchAI != null)
        {
            // EnemyAI����RegisterWitch�ł��Ă��Ȃ���΁A�����œo�^
            if (CurrentWitch == null)
            {
                RegisterWitch(witchAI);
                Debug.Log("[WitchManager] Awake���̃Y���␳�ɂ��蓮�o�^���܂����B");
            }
        }
    }


    void Update()
    {
        if (!isWitchActive || CurrentWitch == null) return;

        // ����FootPoint�
        Vector3 witchFoot = (WitchFootPoint != null) ? WitchFootPoint.position : CurrentWitch.transform.position;

        // ���̈ʒu����ɍX�V�i�����_�j
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
            // ������Chase���Ȃ�Search�ɕύX
            if (CurrentWitch != null && CurrentWitch.CurrentState == WitchState.Chase)
            {
                CurrentWitch.SetStateToSearch(GameManager.Instance.sisterFootPoint.position);
            }

            // �ߊl�⋗��������X�L�b�v
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

        // �ԃt���[���X�V
        if (redFrame != null)
        {
            float t = Mathf.InverseLerp(minSoundDistance, maxSoundDistance, distanceXZ);
            float targetAlpha = Mathf.Lerp(maxRedAlpha, 0f, t);
            Color c = redFrame.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 5f);
            redFrame.color = c;
        }

        // BGM�X�V
        if (witchBGM != null)
        {
            float soundT = Mathf.InverseLerp(minSoundDistance, maxSoundDistance, distanceXZ);
            witchBGM.volume = Mathf.Lerp(1f, 0.2f, soundT);
            if (!witchBGM.isPlaying) witchBGM.Play();
        }

        // �ߊl����
        if (!GameManager.isHiding) // �� �B��Ă���ꍇ�̓X�L�b�v
        {
            if (distanceXZ <= witchCatchDistance && heightDiff < witchCatchHeight)
            {
                GameManager.Instance.TriggerGameOver("�����ɕ߂܂���");
                DeactivateWitch();
            }
        }
        else
        {
            // �B��Ă���ꍇ�A�����������܂�Chase���Ȃ� �� Search��ԂɑJ�ڂ�����
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

        Vector3 sisterFoot = sisterPosition; // ���̍Ō�FootPoint

        // ��������
        if (CurrentWitch.CanSeeSisterBrotherView(sisterFoot))
        {
            // ���������ꍇ�͒ǐՏ�ԂɕύX
            CurrentWitch.SetStateToChase(sisterFoot);
            Debug.Log("[BrotherView] ���𔭌��A�ǐՊJ�n");
        }

        // ��ԕ����iPatrol�͌Z���_�Ŗ����j
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

        // �f�o�b�O���O
        if (enableDebugBrotherLog)
        {
            Debug.Log($"[BrotherView Debug] DistanceXZ={distanceXZ:F2}, HeightDiff={heightDiff:F2}, WitchState={CurrentWitch.CurrentState}, WitchChasing={CurrentWitch.IsChasing}");
        }

        if (distanceXZ <= witchCatchDistance && heightDiff < witchCatchHeight)
        {
            Debug.Log("[BrotherView Debug] ���������ɕ߂܂����I");
            GameManager.Instance.TriggerGameOver("���������ɕ߂܂����c�i�Z���_�j");
            DeactivateWitch();
        }

        // �ԃt���[����\��
        if (redFrame != null)
        {
            Color c = redFrame.color;
            c.a = Mathf.Lerp(c.a, 0f, Time.deltaTime * 5f);
            redFrame.color = c;
        }

        // BGM�ቹ�ʍĐ�
        if (witchBGM != null)
        {
            witchBGM.volume = Mathf.MoveTowards(witchBGM.volume, 0.25f, Time.deltaTime * 0.5f);
            if (!witchBGM.isPlaying) witchBGM.Play();
        }
    }

    // �I�v�V�����FGizmos�Ŗ����Ɩ��̍Ō�ʒu��\��
    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos || CurrentWitch == null) return;

        Vector3 witchPos = (WitchFootPoint != null) ? WitchFootPoint.position : CurrentWitch.transform.position;
        Vector3 sisterPos = sisterPosition;

        // ���̐F�������ŕς���
        float distXZ = Vector2.Distance(new Vector2(witchPos.x, witchPos.z), new Vector2(sisterPos.x, sisterPos.z));
        Gizmos.color = distXZ <= witchCatchDistance ? Color.yellow : Color.cyan;

        Gizmos.DrawLine(witchPos + Vector3.up * 0.5f, sisterPos + Vector3.up * 0.5f);
        Gizmos.DrawSphere(witchPos + Vector3.up * 0.5f, 0.2f);   // ����
        Gizmos.DrawSphere(sisterPos + Vector3.up * 0.5f, 0.2f);  // ��
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
                Debug.Log("[WitchManager] CurrentWitch�������o�^���܂����B");
            }
        }

        if (CurrentWitch == null)
        {
            Debug.LogWarning("�������o�^����Ă��܂���B");
            return;
        }

        // ��FootPoint���S��
        if (GameManager.Instance != null && GameManager.Instance.sisterFootPoint != null)
            sisterPosition = GameManager.Instance.sisterFootPoint.position;

        // �������o��
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

        // --- �ԃt���[�����X�V ---
        if (redFrame != null)
        {
            float t = Mathf.InverseLerp(minSoundDistance, maxSoundDistance, distanceXZ);
            float targetAlpha = Mathf.Lerp(maxRedAlpha, 0f, t);
            Color c = redFrame.color;
            c.a = targetAlpha;
            redFrame.color = c;
        }

        // --- BGM���X�V ---
        if (witchBGM != null)
        {
            float soundT = Mathf.InverseLerp(maxSoundDistance, minSoundDistance, distanceXZ);
            witchBGM.volume = Mathf.Lerp(0.2f, 1f, 1f - soundT);
        }

        Debug.Log("[WitchManager] ForceImmediateUpdate() �ɂ�菉�񓯊�����");
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

        // �܂��o�^��������Γo�^����B���ɕʂ̃I�u�W�F�N�g���o�^�ς݂ł���Ί�{�͏㏑�����Ȃ��i���O�̂݁j
        if (CurrentWitch == null)
        {
            CurrentWitch = witch;
            Debug.Log($"[WitchManager] Registered witch: {CurrentWitch.name}");
        }
        else if (CurrentWitch != witch)
        {
            // ���ɕʂ̃C���X�^���X���o�^�ς� �� �V�[���R�s�[���o�^���悤�Ƃ��Ă���\��
            Debug.LogWarning($"[WitchManager] RegisterWitch ignored: another witch already registered ({CurrentWitch.name}). New: {witch.name}");
        }
    }



    public void SetVisible(bool visible)
    {
        if (CurrentWitch == null) return;
        CurrentWitch.SetVisible(visible);
    }
}
