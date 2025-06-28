
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    private GameObject hitBook, hitItem;
    public Outline hitOutlineCS;    //�I�������I�u�W�F�N�g�ɕt�^����Outline�X�N���v�g
    public Text EventTxt;
    public GameObject BookCanvas;
    public static bool canvasAvtive;
    public GameObject ItemUsePoint;
    public static GameObject pickedItem;
    public Camera heldItemCamera;


    // Start is called before the first frame update
    void Start()
    {
        if (!GameManager.isInBookWorld)
        {
            BookCanvas = GameObject.Find("BookCanvas");
            BookCanvas.SetActive(false);
            canvasAvtive = false;
        }

        EventTxt = GameObject.Find("EventText").GetComponent<Text>();
        EventTxt.enabled = false;
        
        PlayerController.isPlayerMove = true;

        if (GameObject.Find("testItemPoint")!=null)
        {
            ItemUsePoint = GameObject.Find("testItemPoint");
            ItemUsePoint.SetActive(false);
        }
        

        // MainCamera �̎q���� "HeldItemCamera" ��T��
        heldItemCamera = Camera.main.GetComponentInChildren<Camera>(true);

    }

    // Update is called once per frame
    void Update()
    {
        // ��ʒ����̃X�N���[�����W���擾
        Vector3 centerScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // ��ʒ����̃X�N���[�����W�����[���h���W�ɕϊ�
        Ray ray = Camera.main.ScreenPointToRay(centerScreenPosition);
        RaycastHit hit;


        // �O��� hitBook �� Outline �𖳌����i�O��̃I�u�W�F�N�g���� Outline �𖳌����j
        if (hitBook != null)
        {
            if (hitOutlineCS != null)
            {
                hitOutlineCS.enabled = false;
            }

            hitBook = null; // hitBook ����ɂ���
            EventTxt.enabled = false;
        }

        // ���C���I�u�W�F�N�g�ɓ��������ꍇ�̂ݏ��������s
        if (Physics.Raycast(ray, out hit))
        {
            // hit.collider �� null �łȂ����`�F�b�N
            if (hit.collider != null && hit.collider.CompareTag("Book"))
            {
                // �N���b�N�����J�b�v��GameObject��ۑ�
                hitBook = hit.collider.gameObject;
                //�C�x���g�e�L�X�g��\��
                EventTxt.text = "�N���b�N�Ŗ{���J��";
                EventTxt.enabled = true;
                if (Input.GetMouseButtonDown(0))
                {
                    BookCanvas.SetActive(true);
                    canvasAvtive = true;
                    PlayerController.isPlayerMove = false;

                    // �J�[�\����\�����Œ������
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                //�I�u�W�F�N�g�̃A�E�g���C���X�N���v�g���擾
                hitOutlineCS = hitBook.GetComponent<Outline>();
                hitOutlineCS.enabled = true;
            }

            if (hit.collider != null && hit.collider.CompareTag("testItem"))
            {
                hitItem = hit.collider.gameObject;

                // �C�x���g�e�L�X�g��\��
                EventTxt.text = "�N���b�N�ŏE��";
                EventTxt.enabled = true;

                if (Input.GetMouseButtonDown(0))
                {
                    // �A�C�e�����C���X�^���X��
                    pickedItem = Instantiate(hitItem);

                    // Layer �� HeldItemLayer �ɕύX
                    SetLayerRecursively(pickedItem, LayerMask.NameToLayer("HeldItemLayer"));

                    // �J�����̎q�ɂ��ď�ɕ\�������悤��
                    pickedItem.transform.SetParent(heldItemCamera.transform); // ���ϐ��ǉ����Ă���
                    pickedItem.transform.localPosition = new Vector3(0.7f, -0.4f, 1.5f);
                    pickedItem.transform.localRotation = Quaternion.identity;
                    pickedItem.transform.localScale = Vector3.one * 0.4f;

                    // �R���C�_�[�E���W�b�h�{�f�B�𖳌����i�������h�~�j
                    Collider col = pickedItem.GetComponent<Collider>();
                    if (col != null) col.enabled = false;

                    Rigidbody rb = pickedItem.GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = true;

                    Debug.Log("�A�C�e�����l��: " + hitItem.name);
                    hitItem.SetActive(false); // ���̃I�u�W�F�N�g���\��
                    ItemUsePoint.SetActive(true);
                }
            }

        }

        if (canvasAvtive==true)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CloseBook();
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void CloseBook()
    {
        BookCanvas.SetActive(false);
        canvasAvtive = false;
        PlayerController.isPlayerMove = true;

        // �J�[�\�����\�����Œ�
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
