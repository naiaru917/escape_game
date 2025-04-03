using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    private List<GameObject> answerList,selectIceList;

    // Start is called before the first frame update
    void Start()
    {
        EventTxt.enabled = false;
        iceCnt = 0;
        answerList = new List<GameObject> { IceA, IceB, IceC, IceD };
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
            if (iceCnt < 4) //アイスは4個まで
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //選択したボタンをもとに選択したアイスを登録
                    SwichToIce(hitItem);
                    
                    //選択したアイスを出現
                    AddIce(selectIce);
                }
            }
            else
            {
                //正しいアイスを選んでいるか判定
                CheckIce();
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
        Instantiate(selectIce);   //アイスオブジェクトを出現
        selectIceList.Add(selectIce);
        iceCnt++;
    }

    void CheckIce()
    {
        if (answerList.SequenceEqual(selectIceList))
        {
            Debug.Log("GameClear!!");
        }
        else
        {
            Debug.Log("Miss...");
        }
    }
}
