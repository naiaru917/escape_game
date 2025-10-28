using System.Collections.Generic;
using UnityEngine;

public class WorldMapUpdater : MonoBehaviour
{
    public GameObject BookCanvas;
    public List<string> bookWorldScenes = new List<string> { "Stage1", "Stage2", "Stage3"};    //本世界のシーン

    [Header("UI設定")]
    private RectTransform mapImage, playerImage;
    private List<RectTransform> imageList = new List<RectTransform>();
    private List<string> imageNames = new List<string> { "CupA", "CupB", "CupC", "CupD" };

    [Header("ワールド範囲設定")]
    [SerializeField] private Vector2 worldCenter = Vector2.zero;

    //カップの位置取得
    [SerializeField] private ObjectState objectState;
    private Dictionary<string, Vector3> CupPositions = new Dictionary<string, Vector3>();

    //本世界の部屋サイズ
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
        // ObjectStateからオブジェクトの情報を取得
        InitializeDictionary();

        // マップ上のカップやプレイヤーの位置を更新
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
        //キャンバスや画像を取得
        FindCanvasElements();

        // 本のマップにあるプレイヤーの位置情報を更新
        UpdatePlayerPosition(GetPlayerPosition(bookWorldScenes[GameManager.currentBookWorldIndex]));

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

    Vector3 GetPlayerPosition(string worldKey)
    {
        // worldKeyをもとに、プレイヤーの位置情報を返す
        if (GameManager.playerPositions.ContainsKey(worldKey))
            return GameManager.playerPositions[worldKey];

        // 初期位置
        return new Vector3(0, 2f, 0);
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
        string currentScene = bookWorldScenes[GameManager.currentBookWorldIndex];
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
        string currentScene = bookWorldScenes[GameManager.currentBookWorldIndex];
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
}
