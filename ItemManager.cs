
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    private GameObject hitBook, hitItem;
    public Outline hitOutlineCS;    //選択したオブジェクトに付与したOutlineスクリプト
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
        

        // MainCamera の子から "HeldItemCamera" を探す
        heldItemCamera = Camera.main.GetComponentInChildren<Camera>(true);

    }

    // Update is called once per frame
    void Update()
    {
        // 画面中央のスクリーン座標を取得
        Vector3 centerScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // 画面中央のスクリーン座標をワールド座標に変換
        Ray ray = Camera.main.ScreenPointToRay(centerScreenPosition);
        RaycastHit hit;


        // 前回の hitBook の Outline を無効化（前回のオブジェクトから Outline を無効化）
        if (hitBook != null)
        {
            if (hitOutlineCS != null)
            {
                hitOutlineCS.enabled = false;
            }

            hitBook = null; // hitBook を空にする
            EventTxt.enabled = false;
        }

        // レイがオブジェクトに当たった場合のみ処理を実行
        if (Physics.Raycast(ray, out hit))
        {
            // hit.collider が null でないかチェック
            if (hit.collider != null && hit.collider.CompareTag("Book"))
            {
                // クリックしたカップのGameObjectを保存
                hitBook = hit.collider.gameObject;
                //イベントテキストを表示
                EventTxt.text = "クリックで本を開く";
                EventTxt.enabled = true;
                if (Input.GetMouseButtonDown(0))
                {
                    BookCanvas.SetActive(true);
                    canvasAvtive = true;
                    PlayerController.isPlayerMove = false;

                    // カーソルを表示＆固定を解除
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                //オブジェクトのアウトラインスクリプトを取得
                hitOutlineCS = hitBook.GetComponent<Outline>();
                hitOutlineCS.enabled = true;
            }

            if (hit.collider != null && hit.collider.CompareTag("testItem"))
            {
                hitItem = hit.collider.gameObject;

                // イベントテキストを表示
                EventTxt.text = "クリックで拾う";
                EventTxt.enabled = true;

                if (Input.GetMouseButtonDown(0))
                {
                    // アイテムをインスタンス化
                    pickedItem = Instantiate(hitItem);

                    // Layer を HeldItemLayer に変更
                    SetLayerRecursively(pickedItem, LayerMask.NameToLayer("HeldItemLayer"));

                    // カメラの子にして常に表示されるように
                    pickedItem.transform.SetParent(heldItemCamera.transform); // ←変数追加しておく
                    pickedItem.transform.localPosition = new Vector3(0.7f, -0.4f, 1.5f);
                    pickedItem.transform.localRotation = Quaternion.identity;
                    pickedItem.transform.localScale = Vector3.one * 0.4f;

                    // コライダー・リジッドボディを無効化（物理干渉防止）
                    Collider col = pickedItem.GetComponent<Collider>();
                    if (col != null) col.enabled = false;

                    Rigidbody rb = pickedItem.GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = true;

                    Debug.Log("アイテムを獲得: " + hitItem.name);
                    hitItem.SetActive(false); // 元のオブジェクトを非表示
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

        // カーソルを非表示＆固定
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
