using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public GameObject mainCamera;
    private Rigidbody rb;
    public Vector3 cameraForward; //カメラから見た正面
    public Vector3 moveForward; //正面方向への移動量
    public float speed; //現在の移動速度
    public float walkspeed = 3.0f; //歩く時の移動速度
    public float runSpeed = 9.0f; //走る時の移動速度
    public float moveX; // X方向の移動距離
    public float moveZ; // Z方向の移動距離

    public static bool isPlayerMove;    //プレイヤーが移動可能か

    //ジャンプ
    public float jumpForce = 4.0f; //ジャンプ力
    bool isJump, isJumpWait; //ジャンプを行えるかの判定
    float JumpWaitTimer;

    [Header("足音設定")]
    public AudioClip footstepClip;       // 足音音源（共通）
    private AudioSource footstepSource;  // 足音用AudioSource
    public float stepDistance = 0.5f;    // 足音1回ごとに必要な移動距離

    public bool isMoving { get; private set; } = false;

    private CharacterController controller;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        speed = walkspeed;

        // 足音用AudioSource初期化
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.clip = footstepClip;
        footstepSource.spatialBlend = 1.0f; // 3Dサウンド化
        footstepSource.rolloffMode = AudioRolloffMode.Logarithmic;
        footstepSource.maxDistance = 10f;
        footstepSource.loop = false;
        footstepSource.playOnAwake = false;
        footstepSource.volume = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerMove == true)
        {
            moveX = Input.GetAxis("Horizontal");
            moveZ = Input.GetAxis("Vertical");

            isMoving = (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f);

            // 速度切替
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = runSpeed;
            }
            else
            {
                speed = walkspeed;
            }

            // 足音処理
            if (!isJump)
            {
                HandleFootsteps();
            }
                
            //ジャンプ
            if (Input.GetKeyUp("space"))
            {
                if (!isJump && !isJumpWait) //isJumpとisJumpWaitの両方がfalseのとき
                {
                    isJumpWait = true;
                    JumpWaitTimer = 0.2f;
                }
            }

            //少し経ってからジャンプ
            if (isJumpWait)
            {
                JumpWaitTimer -= Time.deltaTime;
                if (JumpWaitTimer < 0)
                {
                    GetComponent<Rigidbody>().linearVelocity = transform.up * jumpForce;
                    isJumpWait = false;
                    isJump = true;
                }
            }

            // ジャンプ開始時に足音を止める
            if (isJump && footstepSource.isPlaying)
            {
                StopAllCoroutines(); // フェード中なら止める
                footstepSource.Stop();
            }
        }
        else
        {
            Debug.Log("移動不可中");
        }
    }

    private void FixedUpdate()
    {
        if (isPlayerMove == true)
        {
            // カメラの方向から、X-Z平面の単位ベクトルを取得
            cameraForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            // 方向キーの入力値とカメラの向きから、移動方向を決定
            moveForward = cameraForward * moveZ + mainCamera.transform.right * moveX;

            // 移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
            rb.linearVelocity = moveForward * speed + new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    //当たり判定が発生したときに呼ばれる
    private void OnCollisionEnter(Collision collision)
    {
        isJump = false;
    }

    private void HandleFootsteps()
    {
        // ジャンプ中なら即停止（早期リターン）
        if (isJump)
        {
            if (footstepSource.isPlaying)
            {
                StopAllCoroutines();
                footstepSource.Stop();
            }
            return;
        }

        // 現在の移動量を計算
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        isMoving = horizontalVelocity.magnitude > 0.1f && !isJump;

        // 歩行 or 走行に応じて pitch 調整
        float t = Mathf.InverseLerp(walkspeed, runSpeed, speed);
        footstepSource.pitch = Mathf.Lerp(1.3f, 1.6f, t);

        if (isMoving)
        {
            // 動いていてまだ再生していない場合 → 再生開始
            if (!footstepSource.isPlaying)
            {
                footstepSource.loop = true;
                footstepSource.Play();
            }
        }
        else
        {
            // 停止中 → フェードアウトして止める
            if (footstepSource.isPlaying)
            {
                StartCoroutine(FadeOutFootsteps(0.3f));
            }
        }
    }


    private IEnumerator FadeOutFootsteps(float fadeTime)
    {
        float startVolume = footstepSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            footstepSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
            yield return null;
        }

        footstepSource.Stop();
        footstepSource.volume = startVolume; // 次の再生に備えて戻す
    }

    // プレイヤーの足音を外部から止めるためのメソッド
    public void StopFootsteps()
    {
        if (footstepSource != null && footstepSource.isPlaying)
        {
            StopAllCoroutines();
            footstepSource.Stop();
            footstepSource.volume = 1.0f; // 次回再生時に備えて戻す
        }
    }



}
