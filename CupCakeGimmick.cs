using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class CupCakeGimmick : MonoBehaviour
{
    public GameObject CupCakeA, CupCakeB, CupCakeC;
    List<GameObject> targets_pos = new List<GameObject>(); // ���ׂẴJ�b�v
    List<bool> isOccupied;  //�J�b�v�P�[�L����������Ă����True�A����Ă��Ȃ��̂Ȃ�False
    private int targetCnt;

    void Start()
    {
        //�J�b�v�P�[�L�̏o���ꏊ�i�I�u�W�F�N�g�j���擾
        GetTargetPos();

        //�����ꏊ���d�Ȃ�Ȃ��悤�ɂ���
        isOccupied = new List<bool>();

        //�����ꏊ�Ɋ��ɃJ�b�v�P�[�L�����邩�̔���
        for (int i = 0; i < targets_pos.Count; i++)
        {
            isOccupied.Add(false);  //�ŏ��͂��ׂ�False��o�^
        }


        //�����_����5����
        for (int i = 0; i < 5; i++)
        {
            //�J�b�v�P�[�L�𐶐�
            ComeOutTarget();

            //�J�b�v�P�[�L�̐����L�^
            targetCnt++;
            Debug.Log("�J�b�v�P�[�L�̐��F" + targetCnt);
        }
        

        targetCnt = 0;
    }

    void Update()
    {

    }

    void GetTargetPos()
    {
        targets_pos = GameObject.FindGameObjectsWithTag("TargetPos").ToList();

        //�o���ꏊ�m�F�p
        //foreach(GameObject target in targets_pos)
        //{
        //    Vector3 potition = target.transform.position;
        //    potition.y += 0.25f;
        //    GameObject obj = Instantiate(CupCakeA, potition, Quaternion.identity);
        //}
    }

    void ComeOutTarget()
    {
        // �󂫂̏ꏊ�����X�g�A�b�v
        List<int> emptyIndices = new List<int>();
        for (int i = 0; i < isOccupied.Count; i++)
        {
            if (!isOccupied[i]) emptyIndices.Add(i);
        }

        // �󂢂Ă�ꏊ���Ȃ��Ȃ�return
        if (emptyIndices.Count == 0) return;

        // �󂢂Ă�ꏊ���烉���_���őI��
        int randomIndex = Random.Range(0, emptyIndices.Count);
        int selectedPos = emptyIndices[randomIndex];

        // ����
        GameObject target = targets_pos[selectedPos];
        Vector3 position = target.transform.position;
        position.y += 0.25f;
        Instantiate(CupCakeA, position, Quaternion.identity);

        // �g�p���}�[�N
        isOccupied[selectedPos] = true;
    }
}
