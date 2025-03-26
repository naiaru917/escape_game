using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    private GameObject hitItem;

    // Start is called before the first frame update
    void Start()
    {

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
        }

        // ���C���I�u�W�F�N�g�ɓ��������ꍇ�̂ݏ��������s
        if (Physics.Raycast(ray, out hit))
        {
            // hit.collider �� null �łȂ����`�F�b�N
            if (hit.collider != null && hit.collider.CompareTag("teaCup"))
            {
                // �N���b�N�����J�b�v��GameObject��ۑ�
                hitItem = hit.collider.gameObject;

                // ���� Outline �R���|�[�l���g�����邩�m�F
                Outline outline = hitItem.GetComponent<Outline>();
                if (outline == null)
                {
                    // Outline �R���|�[�l���g���Ȃ��ꍇ�͒ǉ�
                    outline = hitItem.AddComponent<Outline>();
                }

                // Outline ��L����
                outline.enabled = true;
            }
        }
    }
}
