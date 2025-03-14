using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject mainCamera;
    private Rigidbody rb;
    public Vector3 cameraForward; //�J�������猩������
    public Vector3 moveForward; //���ʕ����ւ̈ړ���
    public float speed; //���݂̈ړ����x
    public float walkspeed = 3.0f; //�������̈ړ����x
    public float runSpeed = 9.0f; //���鎞�̈ړ����x
    public float moveX; // X�����̈ړ�����
    public float moveZ; // Z�����̈ړ�����


    //�W�����v
    public float jumpForce = 4.0f; //�W�����v��
    bool isJump, isJumpWait; //�W�����v���s���邩�̔���
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
        //�W�����v
        if (Input.GetKeyUp("space"))
        {
            if (!isJump && !isJumpWait) //isJump��isJumpWait�̗�����false�̂Ƃ�
            {
                isJumpWait = true;
                JumpWaitTimer = 0.2f;
            }
        }
        //�����o���Ă���W�����v
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

        // �J�����̕�������AX-Z���ʂ̒P�ʃx�N�g�����擾
        cameraForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        // �����L�[�̓��͒l�ƃJ�����̌�������A�ړ�����������
        moveForward = cameraForward * moveZ + mainCamera.transform.right * moveX;

        // �ړ������ɃX�s�[�h���|����B�W�����v�◎��������ꍇ�́A�ʓrY�������̑��x�x�N�g���𑫂��B
        rb.velocity = moveForward * speed + new Vector3(0, rb.velocity.y, 0);

    }

    //�����蔻�肪���������Ƃ��ɌĂ΂��
    private void OnCollisionEnter(Collision collision)
    {
        isJump = false;
    }
}
