using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("�v���C���[Prefab")]
    public GameObject brotherPrefab;
    public GameObject sisterPrefab;

    [Header("�V�[���ݒ�")]
    public string realWorldScene = "Stage4";
    public List<string> bookWorldScenes = new List<string> { "Stage1", "Stage2", "Stage3" };
    public static int currentBookWorldIndex = 0;

    private GameObject currentPlayer;
    private Dictionary<string, Vector3> playerPositions = new Dictionary<string, Vector3>();
    //private Dictionary<string, List<string>> playerItems = new Dictionary<string, List<string>>();
    public static bool isInBookWorld = false;

    [Header("UI�ݒ�")]
    public GameObject canvasPrefab;
    private GameObject canvasInstance;
    private RectTransform mapImage;
    private RectTransform playerImage;

    [Header("���[���h�͈͐ݒ�")]
    [SerializeField] private Vector2 worldCenter = Vector2.zero;
    [SerializeField] private Vector2 worldSize = new Vector2(100, 100);



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePlayerStates();
            CreateCanvasInstance();  // �L�����o�X�̐���
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ����v���C���[����
        SpawnPlayer(isInBookWorld);
    }

    void InitializePlayerStates()
    {
        // �ʒu�̏�����
        playerPositions["BookWorld"] = new Vector3(0, 2f, 0);

        foreach(string scene in bookWorldScenes)
        {
            playerPositions[scene] = new Vector3(0, 2f, 0);
        }
        

        // �A�C�e���̏�����
        //playerItems["BookWorld"] = new List<string>();
        //foreach (string scene in bookWorldScenes)
        //{
        //    playerItems["RealWorld"] = new List<string>();
        //}
    }

    void Update()
    {
        // V�L�[�Ŏ��_�؂�ւ�
        if (Input.GetKeyDown(KeyCode.V))
        {
            SwitchWorld();
        }
    }

    void SpawnPlayer(bool inBookWorld)
    {
        // ���݂̃v���C���[���폜
        if (currentPlayer != null)
        {
            SavePlayerState();
            Destroy(currentPlayer);
        }

        // �v���C���[����
        if (inBookWorld)
        {
            string currentScene = bookWorldScenes[currentBookWorldIndex];

            currentPlayer = Instantiate(brotherPrefab, GetPlayerPosition(currentScene), Quaternion.identity);
        }
        else
        {
            currentPlayer = Instantiate(sisterPrefab, GetPlayerPosition(realWorldScene), Quaternion.identity);
        }

        // �A�C�e�������i�K�v�ɉ����āj
        //RestorePlayerItems();
    }

    void SwitchWorld()
    {
        // �V�[���؂�ւ�
        SavePlayerState();

        if (isInBookWorld)
        {
            //�{�̐��E���猻�����E��
            isInBookWorld = false;
            SceneManager.LoadScene("Stage4");
            Debug.Log(playerPositions[bookWorldScenes[currentBookWorldIndex]]);
        }
        else
        {
            //�������E����{�̐��E��
            isInBookWorld = true;
            string nextScene = bookWorldScenes[currentBookWorldIndex];
            SceneManager.LoadScene(nextScene);
        }

        PlayerController.isPlayerMove = true;
        // �J�[�\�����\�����Œ�
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AdvanceBookWorld()
    {
        if(isInBookWorld && currentBookWorldIndex < bookWorldScenes.Count - 1)
        {
            currentBookWorldIndex++;
            SceneManager.LoadScene(realWorldScene);
        }
        else
        {
            Debug.Log("�{�̐��E���N���A");
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �V�[�����ǂݍ��܂ꂽ��v���C���[���X�|�[��
        SpawnPlayer(isInBookWorld);

        // �}�b�v�ƃv���C���[�摜���Ď擾
        ReattachCanvas();

        // �v���C���[�ʒu�̔��f
        string currentScene = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;
        UpdatePlayerPosition(GetPlayerPosition(currentScene));
    }

    void CreateCanvasInstance()
    {
        if (canvasInstance == null)
        {
            canvasInstance = Instantiate(canvasPrefab);
            DontDestroyOnLoad(canvasInstance);

            // �q�v�f���擾
            mapImage = canvasInstance.transform.Find("MapImage")?.GetComponent<RectTransform>();
            playerImage = canvasInstance.transform.Find("PlayerImage")?.GetComponent<RectTransform>();

            if (mapImage == null || playerImage == null)
            {
                Debug.LogWarning("MapImage�܂���PlayerImage���L�����o�X�Ɋ܂܂�Ă��܂���B");
            }
        }
    }

    void ReattachCanvas()
    {
        if (canvasInstance == null)
        {
            CreateCanvasInstance();
        }
    }


    void SavePlayerState()
    {
        string worldKey = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;
        playerPositions[worldKey] = currentPlayer.transform.position;
        // �����ŃA�C�e�����ۑ��i�A�C�e���擾�����Œǉ��j
    }

    Vector3 GetPlayerPosition(string worldKey)
    {
        if (playerPositions.ContainsKey(worldKey))
            return playerPositions[worldKey];

        // �����ʒu
        return new Vector3(0, 2f, 0);
    }

    //void RestorePlayerItems()
    //{
    //    string worldKey = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;
    //List<string> items = playerItems[worldKey];

    //// �A�C�e�������i�\���⑕���Ȃǂɉ����Ď����j
    //foreach (string item in items)
    //{
    //    Debug.Log($"Item Restored: {item}");
    //}
    //}

    //public void AddItem(string itemName)
    //{
    //    string worldKey = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;
    //    playerItems[worldKey].Add(itemName);
    //    Debug.Log($"Item Added: {itemName}");
    //}

    //public List<string> GetCurrentPlayerItems()
    //{
    //    string worldKey = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;
    //    return playerItems[worldKey];
    //}

    
    public void UpdatePlayerPosition(Vector3 playerWorldPos)
    {
        if (mapImage == null || playerImage == null) return;

        // �}�b�v�̒��S�ʒu�ƃT�C�Y
        Vector2 mapCenter = mapImage.rect.size / 2f;
        Vector2 mapSize = mapImage.rect.size;

        // ���[���h���W���瑊�΍��W�ɕϊ�
        float relativeX = (playerWorldPos.x - worldCenter.x) / (worldSize.x / 2f);
        float relativeY = (playerWorldPos.z - worldCenter.y) / (worldSize.y / 2f);

        // �}�b�v��̍��W�ɕϊ�
        float posX = relativeX * (mapSize.x / 2f);
        float posY = relativeY * (mapSize.y / 2f);

        // �}�b�v�摜�̃��[�J�����W�Ɋ�Â��Ĉʒu��ݒ�
        Vector2 mapCenterOffset = (Vector2)mapImage.localPosition;
        playerImage.anchoredPosition = mapCenterOffset + new Vector2(posX, posY);

    }

}
