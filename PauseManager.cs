using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("���[���h�͈͐ݒ�")]
    public GameObject PauseCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PauseCanvas = Instantiate(Resources.Load<GameObject>("Objects/PauseCanvas"));
        PauseCanvas.SetActive(false); // ������ԂŔ�\���ɂ���

        if (PauseCanvas == null)
        {
            Debug.Log("�L�����o�X��������Ȃ�");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            //�|�[�Y��ʂ�\��
            TogglePauseMenu();
        }
    }

    void TogglePauseMenu()
    {
        if (GameManager.isPaused)
        {
            PauseCanvas.SetActive(false);

            //�Q�[�����ĊJ
            GameManager.isPaused = false;
            GameManager.isSceneMove = true;
            CameraController.isCameraMove = true;

            //�{���J���Ă��邩����
            if (!ItemManager.BookCanvasAvtive)
            {
                //�v���C���[���ړ��\��
                PlayerController.isPlayerMove = true;
            }

            if (CupCakeGimmick.isPlayCupCakeGame)
            {
                GameManager.isSceneMove = false;
            }

            // �J�[�\�����\�����Œ�
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            PauseCanvas.SetActive(true);
            //�Q�[�����~
            GameManager.isPaused = true;
            GameManager.isSceneMove = false;
            CameraController.isCameraMove = false;

            //�v���C���[��s�ړ��\��
            PlayerController.isPlayerMove = false;

            // �J�[�\�����\�����Œ������
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
