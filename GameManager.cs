using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("プレイヤーPrefab")]
    public GameObject brotherPrefab;
    public GameObject sisterPrefab;

    [Header("シーン設定")]
    public string realWorldScene = "Stage4";
    public List<string> bookWorldScenes = new List<string> { "Stage1", "Stage2", "Stage3" };
    public static int currentBookWorldIndex = 0;

    private GameObject currentPlayer;
    private Dictionary<string, Vector3> playerPositions = new Dictionary<string, Vector3>();
    //private Dictionary<string, List<string>> playerItems = new Dictionary<string, List<string>>();
    public static bool isInBookWorld = false;

    [Header("UI設定")]
    public GameObject canvasPrefab;
    private GameObject canvasInstance;
    private RectTransform mapImage;
    private RectTransform playerImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePlayerStates();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初回プレイヤー生成
        SpawnPlayer(isInBookWorld);
    }

    void InitializePlayerStates()
    {
        // 位置の初期化
        playerPositions["BookWorld"] = new Vector3(0, 2f, 0);

        foreach(string scene in bookWorldScenes)
        {
            playerPositions[scene] = new Vector3(0, 2f, 0);
        }
        

        // アイテムの初期化
        //playerItems["BookWorld"] = new List<string>();
        //foreach (string scene in bookWorldScenes)
        //{
        //    playerItems["RealWorld"] = new List<string>();
        //}
    }

    void Update()
    {
        // Vキーで視点切り替え
        if (Input.GetKeyDown(KeyCode.V))
        {
            SwitchWorld();
        }
    }

    void SpawnPlayer(bool inBookWorld)
    {
        // 現在のプレイヤーを削除
        if (currentPlayer != null)
        {
            SavePlayerState();
            Destroy(currentPlayer);
        }

        // プレイヤー生成
        if (inBookWorld)
        {
            string currentScene = bookWorldScenes[currentBookWorldIndex];

            currentPlayer = Instantiate(brotherPrefab, GetPlayerPosition(currentScene), Quaternion.identity);
        }
        else
        {
            currentPlayer = Instantiate(sisterPrefab, GetPlayerPosition(realWorldScene), Quaternion.identity);
        }

        // アイテム復元（必要に応じて）
        //RestorePlayerItems();
    }

    void SwitchWorld()
    {
        // シーン切り替え
        SavePlayerState();

        if (isInBookWorld)
        {
            //本の世界から現実世界へ
            isInBookWorld = false;
            SceneManager.LoadScene("Stage4");
            Debug.Log(playerPositions[bookWorldScenes[currentBookWorldIndex]]);
        }
        else
        {
            //現実世界から本の世界へ
            isInBookWorld = true;
            string nextScene = bookWorldScenes[currentBookWorldIndex];
            SceneManager.LoadScene(nextScene);
        }

        PlayerController.isPlayerMove = true;
        // カーソルを非表示＆固定
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
            Debug.Log("本の世界をクリア");
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
        // シーンが読み込まれたらプレイヤーをスポーン
        SpawnPlayer(isInBookWorld);

        // マップとプレイヤー画像を再取得
        ReattachCanvas();

        // プレイヤー位置の反映
        string currentScene = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;
        UpdatePlayerPosition(GetPlayerPosition(currentScene));
    }

    void SavePlayerState()
    {
        string worldKey = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;
        playerPositions[worldKey] = currentPlayer.transform.position;
        // ここでアイテムも保存（アイテム取得処理で追加）
    }

    Vector3 GetPlayerPosition(string worldKey)
    {
        if (playerPositions.ContainsKey(worldKey))
            return playerPositions[worldKey];

        // 初期位置
        return new Vector3(0, 2f, 0);
    }

    //void RestorePlayerItems()
    //{
    //    string worldKey = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;
    //List<string> items = playerItems[worldKey];

    //// アイテム復元（表示や装備などに応じて実装）
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
}
