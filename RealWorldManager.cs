using UnityEngine;

public class RealWorldManager : MonoBehaviour
{
    public GameObject book1, book2, book3;
    public GameObject Gate; //���̃X�e�[�W�ɍs�����߂̃Q�[�g

    public GameObject Witch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.currentBookWorldIndex == 0)
        {
            book1.SetActive(true);
            book2.SetActive(false);
            book3.SetActive(false);
        }
        else if (GameManager.currentBookWorldIndex == 1)
        {
            book1.SetActive(true);
            book2.SetActive(true);
            book3.SetActive(false);
        }
        else if (GameManager.currentBookWorldIndex == 2)
        {
            book1.SetActive(true);
            book2.SetActive(true);
            book3.SetActive(true);
        }
        else if (GameManager.currentBookWorldIndex == 3)
        {
            book1.SetActive(true);
            book2.SetActive(true);
            book3.SetActive(true);
            Gate.gameObject.SetActive(true);
        }

        //���̕����ֈړ����邽�߂̃Q�[�g���\����
        if (GameManager.isGate == true)
        {
            Gate.gameObject.SetActive(true);
        }
        else
        {
            Gate.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            var witchMgr = WitchManager.Instance;
            if (witchMgr != null && witchMgr.CurrentWitch != null)
            {
                // ��������A�N�e�B�u�Ȃ�o��������
                if (!witchMgr.isWitchActive || !witchMgr.CurrentWitch.gameObject.activeInHierarchy)
                {
                    witchMgr.ActivateWitch();
                    Debug.Log("�������o�������܂����i�G�{���E�EE�L�[�����j");
                }
                else
                {
                    Debug.Log("�����͂��łɏo�����Ă��܂�");
                }
            }
            else
            {
                Debug.LogWarning("�����I�u�W�F�N�g�� WitchManager �ɓo�^����Ă��܂���");
            }
        }
    }
}
