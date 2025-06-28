using UnityEngine;
using UnityEngine.UI;

public class ItemUsePoint : MonoBehaviour
{
    public Text EventTxt;
    private bool isPlayerInUsePoint = false;    //�A�C�e���g�p�\�n�_�Ƀv���C���[�����邩

    void Start()
    {
        EventTxt = GameObject.Find("EventText").GetComponent<Text>();
    }

    void Update()
    {
        if (isPlayerInUsePoint && ItemManager.pickedItem != null)
        {
            EventTxt.text = "�N���b�N�ŃA�C�e�����g�p";
            EventTxt.enabled = true;

            if (Input.GetMouseButtonDown(0))
            {
                Destroy(ItemManager.pickedItem);    //��Ɏ����Ă���A�C�e�����폜
                ItemManager.pickedItem = null;
                EventTxt.text = "";
                EventTxt.enabled = false;
                Debug.Log("�A�C�e�����g�p�F"+ItemManager.pickedItem);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //�v���C���[���A�C�e���g�p�\�n�_�Ɛڂ��Ă���Ƃ�
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInUsePoint = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //�v���C���[���A�C�e���g�p�\�n�_���痣�ꂽ�Ƃ�
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInUsePoint = false;
            EventTxt.text = "";
            EventTxt.enabled = false;
        }
    }
}
