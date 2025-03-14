using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    //ジャンプ
    public float jumpForce = 4.0f; //ジャンプ力
    bool isJump, isJumpWait; //ジャンプを行えるかの判定
    float JumpWaitTimer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        speed = walkspeed;
    }

    // Update is called once per frame
    void Update()
    {
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");


        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = runSpeed;
        }
        else
        {
            speed = walkspeed;
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
                GetComponent<Rigidbody>().velocity = transform.up * jumpForce;
                isJumpWait = false;
                isJump = true;
            }
        }
    }

    private void FixedUpdate()
    {

        // カメラの方向から、X-Z平面の単位ベクトルを取得
        cameraForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        // 方向キーの入力値とカメラの向きから、移動方向を決定
        moveForward = cameraForward * moveZ + mainCamera.transform.right * moveX;

        // 移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
        rb.velocity = moveForward * speed + new Vector3(0, rb.velocity.y, 0);

    }

    //当たり判定が発生したときに呼ばれる
    private void OnCollisionEnter(Collision collision)
    {
        isJump = false;
    }
}
