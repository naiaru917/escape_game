using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 200f; // �}�E�X���x�i��Őݒ��ʂŕύX�\�j
    private float xRotation = 0f;
    private GameObject hitItem;

    void Start()
    {
        // �J�[�\�����\�����Œ�
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        // �}�E�X�̓��͂��擾
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // �c�����̉�]�i�J�����̏㉺�j
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // �㉺90�x�܂Ő���

        // ��]��K�p
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.parent.Rotate(Vector3.up * mouseX); // �e�I�u�W�F�N�g�i�v���C���[�j�����E�ɉ�]



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

    // �ݒ��ʂŊ��x��ύX���邽�߂̊֐�
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
}
