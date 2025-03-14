using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Gimmick : MonoBehaviour
{
    private Camera mainCamera;

    public Transform stage; //stageBlock���܂Ƃ߂Ă���e�I�u�W�F�N�g
    private Transform[,] stageBlock = new Transform[5, 5]; // 5x5�̃O���b�h
    public LayerMask groundLayer; // 5�~5�̓y��̃��C���[

    public GameObject CupA, CupB, CupC, CupD; // �e�J�b�v�̃Q�[���I�u�W�F�N�g

    private GameObject selectCup; //�I�𒆂̃J�b�v
    private Vector2Int selectCup_pos;�@//�I�������J�b�v�̈ʒu

    private List<GameObject> allCups = new List<GameObject>(); // ���ׂẴJ�b�v
    private List<GameObject> otherCups = new List<GameObject>(); // �I�����Ă��Ȃ��̃J�b�v
    List<Vector2Int> otherCups_pos = new List<Vector2Int>(); // �I�����Ă��Ȃ��̃J�b�v

    private string direction;   //�ړ�����������
    private Vector2Int target_pos;  //�ړ��\��̃}�X
    private int moveCnt;    //�ړ��\�ȃ}�X��

    private Vector3 selectCup_posV3;   //�I�������J�b�v�̍��W
    private Vector3 target_posV3;   //�ړ��������ړI�̍��W

    private float moveSpeed = 3f; //�J�b�v�̈ړ����x
    private bool isMove;    //�J�b�v�̈ړ����s���Ă��邩�̔���
    private bool isDo;      //�����̃J�b�v��I�������Ȃ�����

    //�J�b�v�̏����ʒu
    public static Vector3
        start_posA = new Vector3(-3f, 3f, 0f),
        start_posB = new Vector3(-6f, 3f, 9f),
        start_posC = new Vector3(0f, 3f, 12f),
        start_posD = new Vector3(6f, 3f, 6f);

    //�J�b�v�̃S�[���ʒu
    public static Vector3
        goal_posA = new Vector3(-6f, 1.25f, 12f),
        goal_posB = new Vector3(6f, 1.25f, 6f),
        goal_posC = new Vector3(-6f, 1.25f, 3f),
        goal_posD = new Vector3(3f, 1.25f, 0f);

    void Start()
    {
        mainCamera = Camera.main;

        //�}�X�ڂ̎擾
        InitializeGrid();

        // �J�b�v4���擾�i�^�O "Cup" ����擾�j
        allCups = GameObject.FindGameObjectsWithTag("teaCup").ToList();
        otherCups = allCups;

        isDo = true;

        CupA.transform.position = start_posA;
        CupB.transform.position = start_posB;
        CupC.transform.position = start_posC;
        CupD.transform.position = start_posD;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDo == true)
        {


            if (Input.GetMouseButtonDown(0))
            {
                //�J�b�v��I��
                SelectCup();

                //�J�b�v���I������Ă���Ƃ�
                if (selectCup != null)
                {
                    // otherCups����I�������J�b�v�����O
                    otherCups.Remove(selectCup);

                    //�I�������J�b�v�̈ʒu�����擾
                    selectCup_pos = blockNameToVec2Int(GetBlockName(selectCup));

                    //�I�����Ă��Ȃ��J�b�v�̈ʒu�����擾
                    GetOtherCupsPos();

                    //�ړ��������������擾
                    direction = GetDirection(selectCup.transform.position);

                    //���}�X�ړ��ł��邩�v�Z
                    CntMove(direction);

                    //�ړ���̍��W���擾
                    getTargetPos(target_pos);

                    //�ړ����J�n
                    isMove = true;
                }

                // otherCup�����Z�b�g
                otherCups.Add(selectCup);

                // moveCnt�����Z�b�g
                moveCnt = 0;

            }
        }

        if (isMove == true)
        {
            //�I�������J�b�v���ړ�
            moveCup();
        }

        if (selectCup != null)
        {
            //�J�b�v�̈ړ����I�������
            if (selectCup.transform.position == target_posV3)
            {
                isDo = true;
            }
        }

        if (CupA.transform.position == goal_posA && CupB.transform.position == goal_posB &&
            CupC.transform.position == goal_posC && CupD.transform.position == goal_posD)
        {
            Debug.Log("GameClear!!");
        }
    }

    //�}�X�̏����擾(�}�X���Ƃ���transform)
    void InitializeGrid()
    {
        foreach (Transform child in stage)
        {
            if (child != stage) // �e�I�u�W�F�N�g���g�����O
            {
                char row = child.name[0]; // A, B, C, D, E
                int col = int.Parse(child.name.Substring(1)); // 1, 2, 3...�i1-based�j

                // 0�x�[�X�̃C���f�b�N�X�ɕϊ�
                int rowIndex = row - 'A'; // A �� 0, B �� 1, ..., E �� 4
                int colIndex = col - 1;   // 1 �� 0, 2 �� 1, ..., 5 �� 4

                stageBlock[rowIndex, colIndex] = child;
            }
        }
    }

    // ��ʒ����̃I�u�W�F�N�g�����o���A�J�b�v�̑I��������
    void SelectCup()
    {
        // ��ʒ����̃X�N���[�����W���擾
        Vector3 centerScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // ��ʒ����̃X�N���[�����W�����[���h���W�ɕϊ�
        Ray ray = mainCamera.ScreenPointToRay(centerScreenPosition);
        RaycastHit hit;

        // ���C���I�u�W�F�N�g�ɓ��������ꍇ�̂ݏ��������s
        if (Physics.Raycast(ray, out hit))
        {
            // hit.collider �� null �łȂ����`�F�b�N
            if (hit.collider != null && hit.collider.CompareTag("teaCup"))
            {
                // �N���b�N�����J�b�v��GameObject��ۑ�
                selectCup = hit.collider.gameObject;
                isDo = false;
                selectCup.AddComponent<Outline>();
            }
        }

    }

    // �J��������I�u�W�F�N�g�ւ̕����𔻒�
    private string GetDirection(Vector3 objectPosition)
    {
        // �J��������I�u�W�F�N�g�ւ̃x�N�g�����v�Z
        Vector3 direction = objectPosition - mainCamera.transform.position;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            // X�������������ꍇ
            if (direction.x > 0)
                return "X+";
            else
                return "X-";
        }
        else
        {
            // Z�������������ꍇ
            if (direction.z > 0)
                return "Z+";
            else
                return "Z-";
        }
    }

    // �J�b�v�̈ʒu���牺������Ray���΂��A�I�u�W�F�N�g����Ԃ�
    string GetBlockName(GameObject cup)
    {
        RaycastHit hit;

        // �J�b�v�̈ʒu���牺������Ray�𔭎�
        Ray ray = new Ray(cup.transform.position + Vector3.up * 1.0f, Vector3.down);

        // Ray�������ɓ����������ǂ������m�F
        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            return hit.collider.gameObject.name; // �q�b�g�����I�u�W�F�N�g�̖��O��Ԃ�
        }
        else
        {
            return "No block hit"; // �q�b�g���Ȃ������ꍇ
        }
    }

    // �u���b�N��������W�`���ɕϊ�����
    Vector2Int blockNameToVec2Int(string blockName)
    {
        if (blockName == "No block hit")
        {
            return Vector2Int.zero; // �q�b�g���Ȃ������ꍇ
        }

        // ���O����s�Ɨ���擾
        char row = blockName[0]; // A, B, C, D, E
        int col = int.Parse(blockName.Substring(1)); // 1, 2, 3...�i1-based�j

        // �s�Ɨ��0�x�[�X�ɕϊ�
        int rowIndex = row - 'A'; // A �� 0, B �� 1, ..., E �� 4
        int colIndex = col - 1;   // 1 �� 0, 2 �� 1, ..., 5 �� 4

        // ���W�`���ŕԂ�
        return new Vector2Int(rowIndex, colIndex); // Vector2�`���ŕԂ�
    }

    void GetOtherCupsPos()
    {
        // ���W���X�g���N���A
        otherCups_pos.Clear();

        // �e�J�b�v�̍��W���擾
        foreach (GameObject cup in otherCups)
        {
            if (cup != null) // �O�̂���Null�`�F�b�N
            {
                string blockName = GetBlockName(cup); // �Q�[���I�u�W�F�N�g������u���b�N�����擾
                Vector2Int position = blockNameToVec2Int(blockName); // �u���b�N����Vector2Int�̍��W�ɕϊ�
                otherCups_pos.Add(position); // ���W���X�g�ɒǉ�
            }
        }
    }

    int CntMove(string actionDirection)
    {
        Vector2Int currentPos = selectCup_pos;  // �I�����ꂽ�J�b�v�̌��݂̍��W
        Vector2Int NextPos = currentPos;  // �J�b�v�ׂ̗̍��W

        target_pos = selectCup_pos;
        // �A�N�V���������ɂ���Ĕ���𕪊�
        switch (actionDirection)
        {
            case "X+": // �E�����iX+�j�F���݂�y���W����y+=1�̈ʒu�ɑ��̃J�b�v�����݂��Ă��Ȃ����𔻒�

                for (int i = (int)NextPos.y; i < 4; i++)
                {
                    NextPos.y += 1; // ���݂�y���W + 1

                    for (int j = 0; j < otherCups.Count; j++)
                    {
                        if (NextPos == otherCups_pos[j])
                        {
                            return moveCnt = 0; // �ړ��s��
                        }
                    }
                    moveCnt++;
                    target_pos.y += 1;

                }
                return moveCnt;


            case "X-": // �������iX-�j�F���݂�y���W����y-=1�̈ʒu�ɑ��̃J�b�v�����݂��Ă��Ȃ����𔻒�

                for (int i = (int)NextPos.y; i > 0; i--)
                {
                    NextPos.y -= 1; // ���݂�y���W - 1

                    for (int j = 0; j < otherCups.Count; j++)
                    {
                        if (NextPos == otherCups_pos[j])
                        {
                            return moveCnt = 0; // �ړ��s��
                        }
                    }
                    moveCnt++;
                    target_pos.y -= 1;
                }
                return moveCnt;


            case "Z+": // ��O�����iZ+�j�F���݂�x���W����x-=1�̈ʒu�ɑ��̃J�b�v��x���W�����݂��Ă��Ȃ����𔻒�

                for (int i = (int)NextPos.x; i > 0; i--)
                {
                    NextPos.x -= 1; // ���݂�x���W - 1

                    for (int j = 0; j < otherCups.Count; j++)
                    {
                        if (NextPos == otherCups_pos[j])
                        {
                            return moveCnt = 0; // �ړ��s��
                        }
                    }
                    moveCnt++;
                    target_pos.x -= 1;
                }
                return moveCnt;


            case "Z-": // �������iZ-�j�F���݂�x���W����x+=1�̈ʒu�ɑ��̃J�b�v��x���W�����݂��Ă��Ȃ����𔻒�

                for (int i = (int)NextPos.x; i < 4; i++)
                {
                    NextPos.x += 1; // ���݂�x���W + 1

                    for (int j = 0; j < otherCups.Count; j++)
                    {
                        if (NextPos == otherCups_pos[j])
                        {
                            return moveCnt = 0; // �ړ��s��
                        }
                    }
                    moveCnt++;
                    target_pos.x += 1;
                }
                return moveCnt;

            default:
                return moveCnt = 0; // �s���ȕ����̏ꍇ�͈ړ��s��
        }
    }

    void moveCup()
    {
        selectCup_posV3 = selectCup.transform.position;

        // �V�������W���v�Z
        Vector3 newDirection = target_posV3 - selectCup_posV3;
        // ���̑��x�ňړ�
        if (newDirection.magnitude > 0.1f)  // �ړ����܂��������Ă��Ȃ��ꍇ
        {
            // �J�b�v���X���[�Y�Ɉړ�������
            selectCup.transform.position = Vector3.MoveTowards(selectCup_posV3, target_posV3, moveSpeed * Time.deltaTime);
        }
        else
        {
            // �J�b�v���^�[�Q�b�g�ʒu�ɓ��B������
            selectCup.transform.position = target_posV3; // ���m�Ƀ^�[�Q�b�g�ʒu�Ɉړ�
            isMove = false;
        }
    }
    Vector3 getTargetPos(Vector2Int target_pos)
    {
        target_posV3 = stageBlock[target_pos.x, target_pos.y].transform.position;
        target_posV3.y += 1f;
        return target_posV3;
    }

    
}