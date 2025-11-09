using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IceCreamGimmick : MonoBehaviour
{
    public int stageNumber;
    public GameObject hitItem;  //画面中央にあるオブジェクト
    public GameObject selectIce;    //選択したアイス

    public GameObject IceA, IceB, IceC, IceD;   //アイスのオブジェクト
    public GameObject SwichA, SwichB, SwichC, SwichD;   //スイッチのオブジェクト

    private List<GameObject> selectIceList; //選択したアイスを記録
    private List<string> answerList;        //正解のアイスを記録

    public Text EventTxt;
    public GameObject Gate; //次の部屋へのゲート
    private int iceCnt = 0;     //現在重ねているアイスの個数

    public ObjectState objectState;
    [SerializeField] private float rayDistance = 2.5f; // レイを飛ばす最大距離

    // Start is called before the first frame update
    void Start()
    {
        EventTxt.enabled = false;

        //正しいアイスの順番を登録
        answerList = new List<string> { "IceA", "IceB", "IceC", "IceD" };

        if (selectIceList == null)
        {
            selectIceList = new List<GameObject>();
        }
        

        //次の部屋へのゲートを非表示
        Gate.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // 画面中央のスクリーン座標を取得
        Vector3 centerScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // 画面中央のスクリーン座標をワールド座標に変換
        Ray ray = Camera.main.ScreenPointToRay(centerScreenPosition);
        RaycastHit hit;


        // 前回の hitItem の Outline を無効化（前回のオブジェクトから Outline を削除）
        if (hitItem != null)
        {
            hitItem = null; // hitItem を空にする
            EventTxt.enabled = false;
        }

        // レイがオブジェクトに当たった場合のみ処理を実行
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            // hit.collider が null でないかチェック
            if (hit.collider != null && hit.collider.CompareTag("Swich"))
            {
                // クリックしたアイテムのGameObjectを保存
                hitItem = hit.collider.gameObject;

                //イベントテキストを表示
                EventTxt.text = "ボタンをクリック";
                EventTxt.enabled = true;
            }
        }

        if (hitItem != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //選択したボタンをもとに選択したアイスを登録
                SwichToIce(hitItem);

                //選択したアイスを出現
                AddIce(selectIce);
            }

            if (iceCnt == 4)
            {
                //アイスの順番が合っているか判定
                if (CheckIce())
                {
                    Debug.Log("GameClear!!");
                    Gate.SetActive(true);
                    Destroy(this);
                }
            }
            else if (iceCnt >= 5)
            {
                //アイスを削除
                ResetGimmick();

                //選択したボタンをもとに選択したアイスを登録
                SwichToIce(hitItem);

                //選択したアイスを出現
                AddIce(selectIce);
            }

        }

    }

    void SwichToIce(GameObject hitItem)
    {
        if (hitItem == SwichA)
        {
            selectIce = IceA;
        }
        if (hitItem == SwichB)
        {
            selectIce = IceB;
        }
        if (hitItem == SwichC)
        {
            selectIce = IceC;
        }
        if (hitItem == SwichD)
        {
            selectIce = IceD;
        }
    }

    void AddIce(GameObject selectIce)
    {
        GameObject instance = Instantiate(selectIce);  //アイス（インスタンス）を生成
        instance.name = selectIce.name;
        selectIceList.Add(instance);
        iceCnt++;
    }

    bool CheckIce()
    {
        for (int i = 0; i < answerList.Count; i++)
        {
            string correctName = answerList[i];
            string selectedName = selectIceList[i].name.Trim();

            if (correctName != selectedName)
                return false;
        }
        return true;
    }

    void ResetGimmick()
    {
        foreach (GameObject ice in selectIceList)
        {
            Destroy(ice);   //アイスの削除
        }
        selectIceList.Clear(); // リストも空に
        iceCnt = 0; //アイスの個数を空に
    }

    public void RegisterRestoredIce(GameObject ice)
    {
        if (selectIceList == null)
            selectIceList = new List<GameObject>();

        selectIceList.Add(ice);
        iceCnt++;
    }
}
