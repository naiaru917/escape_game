using System.Collections.Generic;
using UnityEngine;

public class WorldMapUpdater : MonoBehaviour
{
    public GameObject BookCanvas;
    public List<string> bookWorldScenes = new List<string> { "Stage1", "Stage2", "Stage3"};    //�{���E�̃V�[��

    [Header("UI�ݒ�")]
    private RectTransform mapImage, playerImage;
    private List<RectTransform> imageList = new List<RectTransform>();
    private List<string> imageNames = new List<string> { "CupA", "CupB", "CupC", "CupD" };

    [Header("���[���h�͈͐ݒ�")]
    [SerializeField] private Vector2 worldCenter = Vector2.zero;

    //�J�b�v�̈ʒu�擾
    [SerializeField] private ObjectState objectState;
    private Dictionary<string, Vector3> CupPositions = new Dictionary<string, Vector3>();

    //�{���E�̕����T�C�Y
    [SerializeField]
    private Dictionary<string, Vector2> roomSizes = new Dictionary<string, Vector2>()
    {
        { "Stage1", new Vector2(9.5f, 9.5f) },
        { "Stage2", new Vector2(9.5f, 9.5f) },
        { "Stage3", new Vector2(9.5f, 9.5f) }
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ObjectState����I�u�W�F�N�g�̏����擾
        InitializeDictionary();

        // �}�b�v��̃J�b�v��v���C���[�̈ʒu���X�V
        MapUpDate();
    }

    void InitializeDictionary()
    {
        foreach (var data in objectState.objectDataList)
        {
            CupPositions[data.objectName] = data.position;
        }
    }

    void MapUpDate()
    {
        //�L�����o�X��摜���擾
        FindCanvasElements();

        // �{�̃}�b�v�ɂ���v���C���[�̈ʒu�����X�V
        UpdatePlayerPosition(GetPlayerPosition(bookWorldScenes[GameManager.currentBookWorldIndex]));

        // �{�̃}�b�v�ɂ���e�B�[�J�b�v�̈ʒu�����X�V
        for (int i = 0; i < imageList.Count; i++)
        {
            string imageName = imageNames[i];

            // �J�b�v�̈ʒu��CupPositions�ɑ��݂��邩�`�F�b�N
            if (CupPositions.ContainsKey(imageName))
            {
                UpdateCupPosition(CupPositions[imageName], imageList[i]);
            }
        }
    }

    Vector3 GetPlayerPosition(string worldKey)
    {
        // worldKey�����ƂɁA�v���C���[�̈ʒu����Ԃ�
        if (GameManager.playerPositions.ContainsKey(worldKey))
            return GameManager.playerPositions[worldKey];

        // �����ʒu
        return new Vector3(0, 2f, 0);
    }

    void FindCanvasElements()
    {
        // �L�����o�X���擾
        GameObject canvasObject = GameObject.Find("BookCanvas");

        if (canvasObject != null)
        {
            // �L�����o�X���ɂ���}�b�v�摜�ƃv���C���[�摜���擾
            mapImage = canvasObject.transform.Find("MapImage").GetComponent<RectTransform>();
            playerImage = canvasObject.transform.Find("PlayerImage").GetComponent<RectTransform>();


            imageList.Clear();
            foreach (string name in imageNames)
            {
                string ObjectName = name + "Image";
                imageList.Add(canvasObject.transform.Find(ObjectName).GetComponent<RectTransform>());
            }
        }

    }

    public void UpdatePlayerPosition(Vector3 playerWorldPos)
    {
        if (mapImage == null || playerImage == null) return;

        // ���݂̕����T�C�Y���擾
        string currentScene = bookWorldScenes[GameManager.currentBookWorldIndex];
        if (!roomSizes.ContainsKey(currentScene)) return;
        Vector2 roomSize = roomSizes[currentScene];

        // ���[���h���W���瑊�΍��W�ɕϊ�
        float relativeX = (playerWorldPos.x - worldCenter.x) / (roomSize.x / 2f);
        float relativeY = (playerWorldPos.z - worldCenter.y) / (roomSize.y / 2f);

        // �}�b�v�̃T�C�Y
        Vector2 mapSize = mapImage.rect.size;
        Vector2 mapCenterOffset = (Vector2)mapImage.localPosition;

        // �}�b�v��̍��W�ɕϊ�
        float posX = relativeX * (mapSize.x / 2f);
        float posY = relativeY * (mapSize.y / 2f);

        // ���S��Ɉʒu��ݒ�
        playerImage.anchoredPosition = mapCenterOffset + new Vector2(posX, posY);
    }

    public void UpdateCupPosition(Vector3 CupWorldPos, RectTransform imagePos)
    {
        if (mapImage == null || imagePos == null) return;

        // ���݂̕����T�C�Y���擾
        string currentScene = bookWorldScenes[GameManager.currentBookWorldIndex];
        if (!roomSizes.ContainsKey(currentScene)) return;
        Vector2 roomSize = roomSizes[currentScene];

        // ���[���h���W���瑊�΍��W�ɕϊ�
        float relativeX = (CupWorldPos.x - worldCenter.x) / (roomSize.x / 2f);
        float relativeY = (CupWorldPos.z - worldCenter.y) / (roomSize.y / 2f);

        // �}�b�v�̃T�C�Y
        Vector2 mapSize = mapImage.rect.size;
        Vector2 mapCenterOffset = (Vector2)mapImage.localPosition;

        // �}�b�v��̍��W�ɕϊ�
        float posX = relativeX * (mapSize.x / 2f);
        float posY = relativeY * (mapSize.y / 2f);

        // ���S��Ɉʒu��ݒ�
        imagePos.anchoredPosition = mapCenterOffset + new Vector2(posX, posY);
    }
}
