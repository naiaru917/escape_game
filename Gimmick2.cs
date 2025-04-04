using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gimmick2 : MonoBehaviour
{
    public GameObject hitItem;  //画面中央にあるオブジェクト
    public GameObject selectIce;    //選択したアイス
    public Text EventTxt;
    public GameObject IceA, IceB, IceC, IceD;   //アイスのオブジェクト
    public GameObject SwichA, SwichB, SwichC, SwichD;   //アイスのオブジェクト
    private int iceCnt;
    private List<GameObject> selectIceList;
    private List<string> answerList;
    public string iceId;

    // Start is called before the first frame update
    void Start()
    {
        EventTxt.enabled = false;
        iceCnt = 0;

        //正しいアイスの順番を登録
        answerList = new List<string> { "IceA", "IceB", "IceC", "IceD" };
        selectIceList = new List<GameObject>();
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
            Outline outline = hitItem.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false; // Outline を無効化
            }
            hitItem = null; // hitItem を空にする
            EventTxt.enabled = false;
        }

        // レイがオブジェクトに当たった場合のみ処理を実行
        if (Physics.Raycast(ray, out hit))
        {
            // hit.collider が null でないかチェック
            if (hit.collider != null && hit.collider.CompareTag("Swich"))
            {
                // クリックしたアイテムのGameObjectを保存
                hitItem = hit.collider.gameObject;

                Outline outline = hitItem.GetComponent<Outline>();

                // Outline を有効化
                outline.enabled = true;

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
        selectIceList.Add(instance);
        iceCnt++;
    }

    bool CheckIce()
    {
        for (int i = 0; i < answerList.Count; i++)
        {
            string correctName = answerList[i];
            string selectedName = selectIceList[i].name.Replace("(Clone)", "").Trim();

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
}
