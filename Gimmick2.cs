using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Gimmick2 : MonoBehaviour
{
    public GameObject hitItem;  //��ʒ����ɂ���I�u�W�F�N�g
    public GameObject selectIce;    //�I�������A�C�X
    public Text EventTxt;
    public GameObject IceA, IceB, IceC, IceD;   //�A�C�X�̃I�u�W�F�N�g
    public GameObject SwichA, SwichB, SwichC, SwichD;   //�A�C�X�̃I�u�W�F�N�g
    private int iceCnt;
    private List<GameObject> answerList,selectIceList;

    // Start is called before the first frame update
    void Start()
    {
        EventTxt.enabled = false;
        iceCnt = 0;
        answerList = new List<GameObject> { IceA, IceB, IceC, IceD };
        selectIceList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // ��ʒ����̃X�N���[�����W���擾
        Vector3 centerScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // ��ʒ����̃X�N���[�����W�����[���h���W�ɕϊ�
        Ray ray = Camera.main.ScreenPointToRay(centerScreenPosition);
        RaycastHit hit;


        // �O��� hitItem �� Outline �𖳌����i�O��̃I�u�W�F�N�g���� Outline ���폜�j
        if (hitItem != null)
        {
            Outline outline = hitItem.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false; // Outline �𖳌���
            }
            hitItem = null; // hitItem ����ɂ���
            EventTxt.enabled = false;
        }

        // ���C���I�u�W�F�N�g�ɓ��������ꍇ�̂ݏ��������s
        if (Physics.Raycast(ray, out hit))
        {
            // hit.collider �� null �łȂ����`�F�b�N
            if (hit.collider != null && hit.collider.CompareTag("Item"))
            {
                // �N���b�N�����A�C�e����GameObject��ۑ�
                hitItem = hit.collider.gameObject;

                Outline outline = hitItem.GetComponent<Outline>();

                // Outline ��L����
                outline.enabled = true;

                //�C�x���g�e�L�X�g��\��
                EventTxt.text = "�{�^�����N���b�N";
                EventTxt.enabled = true;
            }
        }

        if (hitItem != null)
        {
            if (iceCnt < 4) //�A�C�X��4�܂�
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //�I�������{�^�������ƂɑI�������A�C�X��o�^
                    SwichToIce(hitItem);
                    
                    //�I�������A�C�X���o��
                    AddIce(selectIce);
                }
            }
            else
            {
                //�������A�C�X��I��ł��邩����
                CheckIce();
            }

        }

    }

    void SwichToIce(GameObject hitItem)
    {
        if (hitItem == SwichA)
        {
            selectIce = IceA;
        }
        if (hitItem == SwichB)
        {
            selectIce = IceB;
        }
        if (hitItem == SwichC)
        {
            selectIce = IceC;
        }
        if (hitItem == SwichD)
        {
            selectIce = IceD;
        }
    }

    void AddIce(GameObject selectIce)
    {
        Instantiate(selectIce);   //�A�C�X�I�u�W�F�N�g���o��
        selectIceList.Add(selectIce);
        iceCnt++;
    }

    void CheckIce()
    {
        if (answerList.SequenceEqual(selectIceList))
        {
            Debug.Log("GameClear!!");
        }
    }
}
