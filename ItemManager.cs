using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    private GameObject hitItem;
    public Text EventTxt;
    public GameObject BookCanvas;
    public static bool BookCanvasAvtive;
    public GameObject ItemUsePoint;
    public static GameObject pickedItem;
    public Transform heldItemSlot;
    [SerializeField] private float rayDistance = 4.5f; // レイを飛ばす最大距離
    public static Vector3 originalScale;  //アイテムオブジェクトのサイズ

    // 謎解き用：アイテムと設置位置の管理
    private Dictionary<GameObject, GameObject> placedItems = new(); // InstallationLocation -> Item
    public GameObject[] correctOrder = new GameObject[3]; // 正しい順番のアイテム（設置場所順）

    public GameObject testPrefab; // 煙のプレハブ（インスペクターで割り当てる）
    private Hideable currentHideable;

    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "RealWorld" || SceneManager.GetActiveScene().name == "test")
        {
            BookCanvas = GameObject.Find("BookCanvas");
            BookCanvas.SetActive(false);
            BookCanvasAvtive = false;
        }

        EventTxt = GameObject.Find("EventText").GetComponent<Text>();
        EventTxt.enabled = false;

        if (GameObject.Find("testItemPoint") != null)
        {
            ItemUsePoint = GameObject.Find("testItemPoint");
            ItemUsePoint.SetActive(false);
        }

        // メインカメラから"HeldItemSlot"を検索
        heldItemSlot = Camera.main.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "HeldItemSlot");

        testPrefab = Resources.Load<GameObject>("CFXR Magic Poof");
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.isPaused)
        {
            // 手持ちアイテムをQキーで捨てる
            if (pickedItem != null && Input.GetKeyDown(KeyCode.Q))
            {
                    DropItem();
            }

            // 画面中央のスクリーン座標を取得
            Vector3 centerScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);

            // 画面中央のスクリーン座標をワールド座標に変換
            Ray ray = Camera.main.ScreenPointToRay(centerScreenPosition);
            RaycastHit hit;

            // 前回のhitItemをリセット
            if (hitItem != null)
            {
                
                hitItem = null; // hitBook を空にする
                EventTxt.enabled = false;
            }
            if (hitItem != null)
            {
                EventTxt.enabled = false;
            }

            // レイがオブジェクトに当たった場合のみ処理を実行
            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                // ヒットしたのが本の場合
                if (hit.collider != null && hit.collider.CompareTag("Book"))
                {
                    OpenBook(hit);
                }

                

                // ヒットしたのがティーカップの場合
                if (hit.collider != null && hit.collider.CompareTag("teaCup"))
                {
                    HitTeaCup(hit);
                }

                // ヒットしたのがアイテム設置場所の場合
                if (hit.collider != null && hit.collider.CompareTag("InstallationLocation"))
                {
                    if (pickedItem != null)
                    {
                        PlacItem(hit);
                    }
                }

                // ヒットしたのがカップケーキの場合
                if (hit.collider != null && hit.collider.CompareTag("CupCake"))
                {
                    HitCupCake(hit);
                }

                // ヒットしたのがダイヤルパネルの場合
                if (hit.collider != null && hit.collider.CompareTag("DialPanel") && !DialLock.isUnlocked)
                {
                    HitDialPanel(hit);
                }

                

                // ヒットしたのが鍋の場合
                if (hit.collider != null && hit.collider.CompareTag("Cauldron"))
                {
                    HitCauldron(hit);
                }

                // ヒットしたのが隠れられる箱・樽の場合
                if (hit.collider != null && hit.collider.CompareTag("Hideable"))
                {
                    HitHideable(hit);
                }

                if(pickedItem == null)
                {
                    // ヒットしたのがアイテムの場合
                    if (hit.collider != null && hit.collider.CompareTag("Item"))
                    {
                        PickItem(hit);
                    }

                    // ヒットしたのが鍋の材料の場合
                    if (hit.collider != null && hit.collider.CompareTag("IngredientItem"))
                    {
                        PickIngredient(hit);
                    }
                }

                // 本を開いている状態で Tabキーを押したら本を閉じる
                if (BookCanvasAvtive == true)
                {
                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        CloseBook();
                    }
                }
            }

            if (currentHideable != null && GameManager.isHiding)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    currentHideable.Exit();
                    currentHideable = null;
                }
            }
        }
    }

    void OpenBook(RaycastHit hit)
    {
        // クリックしたカップのGameObjectを保存
        hitItem = hit.collider.gameObject;
        //イベントテキストを表示
        EventTxt.text = "クリックで本を開く";
        EventTxt.enabled = true;
        if (Input.GetMouseButtonDown(0))
        {
            BookCanvas.SetActive(true);
            BookCanvasAvtive = true;
            PlayerController.isPlayerMove = false;
            CameraController.isCameraMove = false;

            // カーソルを表示＆固定を解除
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    void HitTeaCup(RaycastHit hit)
    {
        hitItem = hit.collider.gameObject;
        //イベントテキストを表示
        EventTxt.text = "クリックでカップを移動";
        EventTxt.enabled = true;
    }

    void PickItem(RaycastHit hit)
    {
        hitItem = hit.collider.gameObject;

        // イベントテキストを表示
        EventTxt.text = "クリックで拾う";
        EventTxt.enabled = true;

        if (Input.GetMouseButtonDown(0))
        {
            pickedItem = hitItem;
            originalScale = pickedItem.transform.localScale;

            // アイテムのObjectStateManagerを取得
            ObjectStateManager osm = hitItem.GetComponent<ObjectStateManager>();

            if (osm != null)
            {
                // shouldSaveをfalseに設定（手持ち状態＝保存しない）
                osm.shouldSave = false;
            }

            // Layer を HeldItemLayer に変更
            SetLayerRecursively(pickedItem, LayerMask.NameToLayer("HeldItemLayer"));

            // カメラの子にして常に表示されるように
            pickedItem.transform.SetParent(heldItemSlot);
            pickedItem.transform.localPosition = Vector3.zero;
            pickedItem.transform.localRotation = Quaternion.identity;

            // コライダー・リジッドボディを無効化（物理干渉防止）
            Collider col = pickedItem.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Rigidbody rb = pickedItem.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Debug.Log("アイテムを獲得: " + hitItem.name);
            ItemUsePoint.SetActive(true);
        }
    }

    void PlacItem(RaycastHit hit)
    {
        hitItem = hit.collider.gameObject;
        EventTxt.text = "クリックでアイテムを設置";
        EventTxt.enabled = true;

        if (Input.GetMouseButtonDown(0))
        {
            // pickedItem が null でないことを再確認
            if (pickedItem != null)
            {
                // 親から切り離す
                pickedItem.transform.SetParent(null);

                // アイテムのバウンディングボックスを取得
                Bounds bounds = pickedItem.GetComponent<Renderer>().bounds;

                // 高さの半分を取得（設置場所の上にのせるため）
                float itemHalfHeight = bounds.extents.y;

                // 設置位置の高さにアイテム下部が接するように調整
                Vector3 placePosition = hitItem.transform.position;
                //placePosition.y += itemHalfHeight;

                pickedItem.transform.position = placePosition;
                pickedItem.transform.rotation = Quaternion.Euler(0, 0, 0);

                // スケール調整
                pickedItem.transform.localScale = originalScale;

                // コライダー有効化
                Collider col = pickedItem.GetComponent<Collider>();
                if (col != null) col.enabled = true;

                // リジッドボディ有効化
                Rigidbody rb = pickedItem.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;  //動かなくする

                // 保存対象に戻す
                ObjectStateManager osm = pickedItem.GetComponent<ObjectStateManager>();
                if (osm != null)
                {
                    osm.shouldSave = true;
                }

                // レイヤーをデフォルトに戻す
                SetLayerRecursively(pickedItem, LayerMask.NameToLayer("Default"));


                // 設置記録
                if (!placedItems.ContainsKey(hitItem))
                {
                    placedItems[hitItem] = pickedItem;
                }

                Debug.Log("設置: " + pickedItem.name);

                ItemPuzzleManager.Instance.ReportPlacement(hit.collider.gameObject, pickedItem);

                // pickedItem をクリア
                pickedItem = null;

                // 使用ポイント非表示
                if (ItemUsePoint != null) ItemUsePoint.SetActive(false);
            }
        }
    }

    void DropItem()
    {
        // カメラの前にドロップ
        Vector3 dropPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
        pickedItem.transform.SetParent(null);
        pickedItem.transform.position = dropPosition;

        // レイヤー戻す
        SetLayerRecursively(pickedItem, LayerMask.NameToLayer("Default"));

        // コライダー復活
        Collider col = pickedItem.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // Rigidbody復活
        Rigidbody rb = pickedItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        // 保存対象に戻す
        ObjectStateManager osm = pickedItem.GetComponent<ObjectStateManager>();
        if (osm != null)
        {
            osm.shouldSave = true;
        }

        pickedItem = null;
        if (ItemUsePoint != null) ItemUsePoint.SetActive(false);
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

    void HitCupCake(RaycastHit hit)
    {
        hitItem = hit.collider.gameObject;
        //イベントテキストを表示
        EventTxt.text = "クリックで叩く";
        EventTxt.enabled = true;

        if (Input.GetMouseButtonDown(0))
        {
            CupCakeManager cm = hit.collider.GetComponent<CupCakeManager>();
            if (cm != null)
            {
                cm.OnMouseDown();
                Debug.Log(hit.collider.name + " を叩いた");
                EventTxt.enabled = false;

                // 叩いたときのエフェクトを生成
                Vector3 spawnPos = hit.collider.transform.position + hit.normal * 0.2f;
                Instantiate(testPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
    void HitDialPanel(RaycastHit hit)
    {
        hitItem = hit.collider.gameObject;
        EventTxt.text = "クリックで数字を変更";
        EventTxt.enabled = true;

        if (Input.GetMouseButtonDown(0))
        {
            DialPanel panel = hitItem.GetComponent<DialPanel>();
            if (panel != null)
            {
                panel.Increment();

                // 答え合わせを行う場合はここでDialLockに通知する
                DialLock lockController = panel.GetComponentInParent<DialLock>();
                if (lockController != null)
                {
                    lockController.CheckAnswer();
                }
            }
        }
    }

    void PickIngredient(RaycastHit hit)
    {
        hitItem = hit.collider.gameObject;

        // UI表示
        EventTxt.text = "クリックで材料を手に取る";
        EventTxt.enabled = true;

        if (Input.GetMouseButtonDown(0))
        {
            if (pickedItem == null) // 既に何か持っている場合は拾わない
            {
                // オリジナルは残したまま、複製を生成
                pickedItem = Instantiate(hitItem);

                // スケール記録
                originalScale = pickedItem.transform.localScale;

                // Layer を HeldItemLayer に変更
                SetLayerRecursively(pickedItem, LayerMask.NameToLayer("HeldItemLayer"));

                // カメラの子にして常に表示されるように
                pickedItem.transform.SetParent(heldItemSlot);
                pickedItem.transform.localPosition = Vector3.zero;
                pickedItem.transform.localRotation = Quaternion.identity;

                // コライダー・リジッドボディを無効化（物理干渉防止）
                Collider col = pickedItem.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                Rigidbody rb = pickedItem.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                Debug.Log("材料を獲得: " + hitItem.name);

                if (ItemUsePoint != null) ItemUsePoint.SetActive(true);
            }
        }
    }


    void HitCauldron(RaycastHit hit)
    {
        hitItem = hit.collider.gameObject;
        EventTxt.text = "クリックで材料を投入";
        EventTxt.enabled = true;

        if (Input.GetMouseButtonDown(0))
        {
            if (pickedItem != null)
            {
                // 鍋の上にアイテムを移動
                Vector3 spawnPos = hitItem.transform.position + Vector3.up * 2f; // 鍋の少し上
                pickedItem.transform.SetParent(null);
                pickedItem.transform.position = spawnPos;
                pickedItem.transform.rotation = Quaternion.identity;
                pickedItem.transform.localScale = originalScale;

                // コライダーとリジッドボディを有効化（物理で落とす）
                Collider col = pickedItem.GetComponent<Collider>();
                if (col != null) col.enabled = true;

                Rigidbody rb = pickedItem.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
                else rb = pickedItem.AddComponent<Rigidbody>();

                // 鍋に報告
                CauldronManager cauldron = hitItem.GetComponent<CauldronManager>();
                if (cauldron != null)
                {
                    cauldron.AddIngredient(pickedItem);
                }

                // 手持ち解除
                pickedItem = null;
                if (ItemUsePoint != null) ItemUsePoint.SetActive(false);

                Debug.Log("材料を鍋に投入しました");
            }
        }
    }

    void HitHideable(RaycastHit hit)
    {
        hitItem = hit.collider.gameObject;
        EventTxt.text = "クリックで隠れる";
        EventTxt.enabled = true;

        // 左クリックで隠れる
        if (Input.GetMouseButtonDown(0))
        {
            Hideable hideable = hitItem.GetComponent<Hideable>();
            if (hideable != null)
            {
                GameObject player = transform.root.gameObject;
                hideable.Enter(player);
                currentHideable = hideable;
            }
        }
    }



    public void CloseBook()
    {
        BookCanvas.SetActive(false);
        BookCanvasAvtive = false;
        PlayerController.isPlayerMove = true;
        CameraController.isCameraMove = true;

        // カーソルを非表示＆固定
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}


