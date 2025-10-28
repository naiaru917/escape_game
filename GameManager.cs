using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("プレイヤーPrefab")]
    public GameObject brotherPrefab;    //本の世界（兄視点）のプレイヤー
    public GameObject sisterPrefab;     //現実世界（妹視点）のプレイヤー
    public Transform sisterFootPoint;         //妹の地面の座標

    [Header("シーン設定")]
    public string realWorldScene = "RealWorld";    //現実世界のシーン
    public List<string> bookWorldScenes = new List<string> { "Stage1", "Stage2", "Stage3"};    //本世界のシーン
    public static int currentBookWorldIndex = 0;    //何番目の本世界にいるか

    private GameObject currentPlayer;
    public static Dictionary<string, Vector3> playerPositions = new Dictionary<string, Vector3>();    //プレイヤーの位置
    private Dictionary<string, float> playerRotations_y = new Dictionary<string, float>();    //プレイヤーのY回転（上下）
    private Dictionary<string, float> playerRotations_x = new Dictionary<string, float>();    //プレイヤーのX回転（左右）

    private Dictionary<string, (string itemName, Vector3 scale)> heldItems = new Dictionary<string, (string, Vector3)>();   // 所持アイテム

    // オブジェクトの位置情報
    [SerializeField] private ObjectState objectState;

    public static bool isInBookWorld = false;   //現在、本世界にいるか
    public static bool isGate = false;    //ゲートが出現しているか
    public static bool isSceneMove = true;  //シーン移動が可能か（ほかの処理中じゃないか）
    public static bool isPaused = false;    //ポーズ画面を開いているか
    public Transform heldItemSlot;

    public GameObject testPrefab;
    public static bool isLastStage = false;
    public static bool isHiding = false;

    [SerializeField] private GameObject gameOverCanvasPrefab;
    private GameObject spawnedGameOverCanvas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            objectState.ResetAllData();      // ObjectStateのデータをリセット
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
        playerRotations_y[realWorldScene] = 0f;         //現実世界の初期回転方向
        playerRotations_x[realWorldScene] = 0f;

        foreach (string scene in bookWorldScenes)
        {
            playerPositions[scene] = new Vector3(0, 2f, 0);     //本世界の初期位置
            playerRotations_y[scene] = 0;          //本世界の初期回転方向
            playerRotations_x[scene] = 0;
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

        // WitchManagerに妹の位置を共有
        if (!isInBookWorld && currentPlayer != null)
        {
            WitchManager.Instance.UpdateSisterPosition(currentPlayer.transform.position);
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

            // Y軸の回転（左右視点）を設定
            float playerY = GetPlayerRotation_y(currentScene);
            currentPlayer.transform.rotation = Quaternion.Euler(0f, playerY, 0f);

            // カメラのX軸回転（上下視点）を設定
            float cameraX = GetPlayerRotation_x(currentScene);
            Camera.main.GetComponent<CameraController>().SetInitialXRotation(cameraX);

        }
        else
        {
            currentPlayer = Instantiate(sisterPrefab, GetPlayerPosition(realWorldScene), Quaternion.identity);

            // Y軸の回転（左右視点）を設定
            float playerY = GetPlayerRotation_y(realWorldScene);
            currentPlayer.transform.rotation = Quaternion.Euler(0f, playerY, 0f);

            // カメラのX軸回転（上下視点）を設定
            float cameraX = GetPlayerRotation_x(realWorldScene);
            Camera.main.GetComponent<CameraController>().SetInitialXRotation(cameraX);
        }

        // 妹の足元参照を WitchManager に登録
        var foot = currentPlayer.GetComponentsInChildren<Transform>(true)
    .FirstOrDefault(t => t.name == "FootPoint");

        if (!inBookWorld && foot != null)
        {
            sisterFootPoint = foot;
            WitchManager.Instance.UpdateSisterPosition(foot.position);
            Debug.Log($"[GameManager] 妹のFootPointを WitchManager に登録しました。（path: {foot.name}）");
        }
        else if (!inBookWorld && foot == null)
        {
            Debug.LogWarning("[GameManager] 妹のFootPointが見つかりませんでした（妹視点で生成しているか確認）");
        }
    }

    void SwitchWorld()
    {
        // シーン切り替え直前に魔女を制御
        if (WitchManager.Instance != null)
        {
            EnemyAI witch = WitchManager.Instance.CurrentWitch;
            if (witch != null)
            {
                if (isInBookWorld)
                {
                    // 兄視点へ行く前：魔女を透明化して動作継続
                    witch.SetVisible(false);
                }
                else
                {
                    // 妹視点へ戻る前：魔女を再表示
                    witch.SetVisible(true);
                }
            }
        }

        // プレイヤーの位置情報を記録
        SavePlayerState();

        if (isInBookWorld)
        {
            //本の世界から現実世界へ
            isInBookWorld = false;
            SceneManager.LoadScene("RealWorld");
            //InitializeDictionary();
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
        RestoreHeldItem();

        // 妹視点シーンの場合
        if (!isInBookWorld && WitchManager.Instance != null)
        {
            var witchMgr = WitchManager.Instance;

            // 妹視点かつ魔女が出現状態の場合のみ再表示
            if (witchMgr.CurrentWitch != null && witchMgr.isWitchActive)
            {
                witchMgr.CurrentWitch.SetVisible(true);
                witchMgr.CurrentWitch.EnableAgent(true);
                Debug.Log("Witch: 妹視点に戻ったため再表示しました。");
            }
        }
    }


    void SavePlayerState()
    {
        //どの世界にいるのかを判定（現実世界か、本世界の何番目か）
        string worldKey = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;

        //プレイヤーの位置情報を記録
        playerPositions[worldKey] = currentPlayer.transform.position;

        //プレイヤーの回転情報を記録
        playerRotations_y[worldKey] = currentPlayer.transform.eulerAngles.y;
        
        float cameraX = Camera.main.transform.localEulerAngles.x;
        if (cameraX > 180f) cameraX -= 360f;    // 角度が180度を超えていたら負の角度に変換
        playerRotations_x[worldKey] = cameraX;


        // 所持アイテムの保存
        if (ItemManager.pickedItem != null)     // もしアイテムが取得中なら
        {
            string itemName = ItemManager.pickedItem.name;  // アイテム名を取得
            Vector3 itemScale = ItemManager.originalScale;  // アイテムのスケールを取得
            heldItems[worldKey] = (itemName, itemScale);    // heldItemsに情報を保存
        }
        else
        {
            heldItems.Remove(worldKey); // 所持していない場合は記録から削除
        }

    }

    Vector3 GetPlayerPosition(string worldKey)
    {
        // worldKeyをもとに、プレイヤーの位置情報を返す
        if (playerPositions.ContainsKey(worldKey))
            return playerPositions[worldKey];

        // 初期位置
        return new Vector3(0, 2f, 0);
    }

    float GetPlayerRotation_y(string worldKey)
    {
        // worldKeyをもとに、プレイヤーの回転情報を返す
        if (playerRotations_y.ContainsKey(worldKey))
            return playerRotations_y[worldKey];

        // 初期位置
        return 0;
    }
    float GetPlayerRotation_x(string worldKey)
    {
        // worldKeyをもとに、プレイヤーの回転情報を返す
        if (playerRotations_x.ContainsKey(worldKey))
            return playerRotations_x[worldKey];

        // 初期位置
        return 0;
    }

    void RestoreHeldItem()
    {
        string worldKey = isInBookWorld ? bookWorldScenes[currentBookWorldIndex] : realWorldScene;

        if (heldItems.TryGetValue(worldKey, out var itemData))
        {
            string Name = itemData.itemName;
            Vector3 Scale = itemData.scale;

            GameObject foundItem = GameObject.Find(Name);   // アイテム名を基にアイテムオブジェクトを検索

            if (foundItem != null)
            {
                // Layer を HeldItemLayer に変更
                SetLayerRecursively(foundItem, LayerMask.NameToLayer("HeldItemLayer"));

                heldItemSlot = null;

                // メインカメラから"HeldItemSlot"を検索
                heldItemSlot = Camera.main.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "HeldItemSlot");

                // カメラの子にして常に表示されるように
                foundItem.transform.SetParent(heldItemSlot, true);  // アイテムオブジェクトを
                foundItem.transform.localPosition = Vector3.zero;
                foundItem.transform.localRotation = Quaternion.identity;

                // コライダー・リジッドボディを無効化（物理干渉防止）
                Collider col = foundItem.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                Rigidbody rb = foundItem.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                ItemManager.pickedItem = foundItem;
                ItemManager.originalScale = Scale;
            }
            else
            {
                Debug.LogWarning($"[{worldKey}] シーンに '{Name}' のアイテムオブジェクトが見つかりませんでした。");
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

    // GameOver は WitchManager から呼び出される
    public void TriggerGameOver(string reason)
    {
        Debug.Log($"GAME OVER: {reason}");

        // プレイヤー操作を停止
        PlayerController.isPlayerMove = false;
        CameraController.isCameraMove = false;
        isSceneMove = false;

        // カーソルを非表示＆固定を解除
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        // 魔女BGMの停止（安全策）
        var witchManager = FindFirstObjectByType<WitchManager>();
        if (witchManager != null)
        {
            witchManager.StopWitchBGM();
        }

        // GameOverCanvas を表示
        if (spawnedGameOverCanvas == null)
        {
            // インスペクタに設定されていればそれを使い、なければResourcesからロード
            GameObject prefab = gameOverCanvasPrefab != null
                ? gameOverCanvasPrefab
                : Resources.Load<GameObject>("Objects/GameOverCanvas");

            if (prefab != null)
            {
                spawnedGameOverCanvas = Instantiate(prefab);
                spawnedGameOverCanvas.SetActive(true);
            }
        }
        else
        {
            spawnedGameOverCanvas.SetActive(true);
        }

    }
}
