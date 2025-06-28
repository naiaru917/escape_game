using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("プレイヤーPrefab")]
    public GameObject brotherPrefab;    //本の世界（兄視点）のプレイヤー
    public GameObject sisterPrefab;     //現実世界（妹視点）のプレイヤー

    [Header("シーン設定")]
    public string realWorldScene = "Stage4";    //現実世界のシーン
    public List<string> bookWorldScenes = new List<string> { "Stage1", "Stage2", "Stage3" };    //本世界のシーン
    public static int currentBookWorldIndex = 0;    //何番目の本世界にいるか

    private GameObject currentPlayer;
    private Dictionary<string, Vector3> playerPositions = new Dictionary<string, Vector3>();    //プレイヤーの位置
    private Dictionary<string, Quaternion> playerRotations = new Dictionary<string, Quaternion>();    //プレイヤーの回転
    public static bool isInBookWorld = false;   //現在、本世界にいるか
    public static bool isFirstRunCup = true;
    public static bool isFirstRunIce = true;

    [Header("UI設定")]
    private RectTransform mapImage, playerImage;
    private List<RectTransform> imageList = new List<RectTransform>();
    private List<string> imageNames = new List<string> { "CupA", "CupB", "CupC", "CupD" };

    [Header("ワールド範囲設定")]
    [SerializeField] private Vector2 worldCenter = Vector2.zero;

    [Header("ワールド範囲設定")]
    public GameObject PauseCanvas;
    private GameObject PauseCanvasInstance;
    private bool isPaused = false;

    //カップの位置取得
    [SerializeField] private ObjectState objectState;
    private Dictionary<string, Vector3> CupPositions = new Dictionary<string, Vector3>();

    //本世界の部屋サイズ
    [SerializeField]
    private Dictionary<string, Vector2> roomSizes = new Dictionary<string, Vector2>()
    {
        { "Stage1", new Vector2(14, 14) },
        { "Stage2", new Vector2(14, 14) },
        { "Stage3", new Vector2(14, 14) }
    };

    public static bool isGate = false;    //ゲートが出現しているか
    public static bool isSceneMove = true;  //シーン移動が可能か（ほかの処理中じゃないか）


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
        playerPositions[realWorldScene] = new Vector3(0, 2f, 0);    //現実世界の初期位置
        playerRotations[realWorldScene] = new Quaternion();         //現実世界の初期回転方向

        foreach (string scene in bookWorldScenes)
        {
            playerPositions[scene] = new Vector3(0, 2f, 0);     //本世界の初期位置
            playerRotations[scene] = new Quaternion();          //本世界の初期回転方向
        }
    }

    void InitializeDictionary()
    {
        foreach (var data in objectState.objectDataList)
        {
            if (data.initialPositionSaved)
            {
                CupPositions[data.objectName] = data.position;
            }
        }
    }

    void Update()
    {
        if (isSceneMove == true)
        {
            // Vキーで視点切り替え
            if (Input.GetKeyDown(KeyCode.V))
            {
                SwitchWorld();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                Debug.Log("現在は移動不可能です");
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            //ポーズ画面を表示
            TogglePauseMenu();
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

            currentPlayer = Instantiate(brotherPrefab, GetPlayerPosition(currentScene), GetPlayerRotation(currentScene));
        }
        else
        {
            currentPlayer = Instantiate(sisterPrefab, GetPlayerPosition(realWorldScene), GetPlayerRotation(realWorldScene));
        }
    }

    void SwitchWorld()
    {
        // プレイヤーの位置情報を記録
        SavePlayerState();

        if (isInBookWorld)
        {
            //本の世界から現実世界へ
            isInBookWorld = false;
            SceneManager.LoadScene("Stage4");
            InitializeDictionary();
        }
        else
        {
            //現実世界から本の世界へ
            isInBookWorld = true;
            string nextScene = bookWorldScenes[currentBookWorldIndex];
            SceneManager.LoadScene(nextScene);
        }

        //プレイヤーを移動可能にしておく
        PlayerController.isPlayerMove = true;

        // カーソルを非表示＆固定
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AdvanceBookWorld()
    {
        if (isInBookWorld && currentBookWorldIndex < bookWorldScenes.Count - 1)
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

        // 現実世界に戻った場合のみキャンバス要素を再取得
        if (scene.name == realWorldScene)
        {
            //キャンバスや画像を取得
            FindCanvasElements();

            // 本のマップにあるプレイヤーの位置情報を更新
            UpdatePlayerPosition(GetPlayerPosition(bookWorldScenes[currentBookWorldIndex]));

            // 本のマップにあるティーカップの位置情報を更新
            for (int i = 0; i < imageList.Count; i++)
            {
                string imageName = imageNames[i];

                // カップの位置がCupPositionsに存在するかチェック
                if (CupPositions.ContainsKey(imageName))
                {
                    UpdateCupPosition(CupPositions[imageName], imageList[i]);
                }
            }

        }

        // プレイヤー位置の反映
        string currentScene = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;
    }


    void SavePlayerState()
    {
        //どの世界にいるのかを判定（現実世界か、本世界の何番目か）
        string worldKey = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;

        //プレイヤーの位置情報を記録
        playerPositions[worldKey] = currentPlayer.transform.position;

        //プレイヤーの回転情報を記録
        playerRotations[worldKey] = currentPlayer.transform.rotation;

    }

    Vector3 GetPlayerPosition(string worldKey)
    {
        // worldKeyをもとに、プレイヤーの位置情報を返す
        if (playerPositions.ContainsKey(worldKey))
            return playerPositions[worldKey];

        // 初期位置
        return new Vector3(0, 2f, 0);
    }

    Quaternion GetPlayerRotation(string worldKey)
    {
        // worldKeyをもとに、プレイヤーの回転情報を返す
        if (playerRotations.ContainsKey(worldKey))
            return playerRotations[worldKey];

        // 初期位置
        return new Quaternion();
    }

    void FindCanvasElements()
    {
        // キャンバスを取得
        GameObject canvasObject = GameObject.Find("BookCanvas");

        if (canvasObject != null)
        {
            // キャンバス内にあるマップ画像とプレイヤー画像を取得
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

        // 現在の部屋サイズを取得
        string currentScene = bookWorldScenes[currentBookWorldIndex];
        if (!roomSizes.ContainsKey(currentScene)) return;
        Vector2 roomSize = roomSizes[currentScene];

        // ワールド座標から相対座標に変換
        float relativeX = (playerWorldPos.x - worldCenter.x) / (roomSize.x / 2f);
        float relativeY = (playerWorldPos.z - worldCenter.y) / (roomSize.y / 2f);

        // マップのサイズ
        Vector2 mapSize = mapImage.rect.size;
        Vector2 mapCenterOffset = (Vector2)mapImage.localPosition;

        // マップ上の座標に変換
        float posX = relativeX * (mapSize.x / 2f);
        float posY = relativeY * (mapSize.y / 2f);

        // 中心基準に位置を設定
        playerImage.anchoredPosition = mapCenterOffset + new Vector2(posX, posY);
    }

    public void UpdateCupPosition(Vector3 CupWorldPos, RectTransform imagePos)
    {
        if (mapImage == null || imagePos == null) return;

        // 現在の部屋サイズを取得
        string currentScene = bookWorldScenes[currentBookWorldIndex];
        if (!roomSizes.ContainsKey(currentScene)) return;
        Vector2 roomSize = roomSizes[currentScene];

        // ワールド座標から相対座標に変換
        float relativeX = (CupWorldPos.x - worldCenter.x) / (roomSize.x / 2f);
        float relativeY = (CupWorldPos.z - worldCenter.y) / (roomSize.y / 2f);

        // マップのサイズ
        Vector2 mapSize = mapImage.rect.size;
        Vector2 mapCenterOffset = (Vector2)mapImage.localPosition;

        // マップ上の座標に変換
        float posX = relativeX * (mapSize.x / 2f);
        float posY = relativeY * (mapSize.y / 2f);

        // 中心基準に位置を設定
        imagePos.anchoredPosition = mapCenterOffset + new Vector2(posX, posY);
    }

    void TogglePauseMenu()
    {
        if (isPaused)
        {
            if (PauseCanvasInstance != null)
            {
                Destroy(PauseCanvasInstance);
            }
            //ゲームを再開
            isPaused = false;

            //プレイヤーを移動可能に
            PlayerController.isPlayerMove = true;

            // カーソルを非表示＆固定
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            PauseCanvasInstance = Instantiate(PauseCanvas, transform);
            //ゲームを停止
            isPaused = true;

            //プレイヤーを不移動可能に
            PlayerController.isPlayerMove = false;

            // カーソルを非表示＆固定を解除
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
    }
}
