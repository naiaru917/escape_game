using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    private GameObject hitItem;

    // Start is called before the first frame update
    void Start()
    {

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
        }

        // レイがオブジェクトに当たった場合のみ処理を実行
        if (Physics.Raycast(ray, out hit))
        {
            // hit.collider が null でないかチェック
            if (hit.collider != null && hit.collider.CompareTag("teaCup"))
            {
                // クリックしたカップのGameObjectを保存
                hitItem = hit.collider.gameObject;

                // 既に Outline コンポーネントがあるか確認
                Outline outline = hitItem.GetComponent<Outline>();
                if (outline == null)
                {
                    // Outline コンポーネントがない場合は追加
                    outline = hitItem.AddComponent<Outline>();
                }

                // Outline を有効化
                outline.enabled = true;
            }
        }
    }
}
