using UnityEngine;
using UnityEngine.UI;

public class OutlineController : MonoBehaviour
{
    private GameObject hitCup;
    public Text EventTxt;   //テキスト「クリックでカップを移動」

    // Start is called before the first frame update
    void Start()
    {
        EventTxt.enabled = false;
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
        if (hitCup != null)
        {
            Outline outline = hitCup.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false; // Outline を無効化
            }
            hitCup = null; // hitItem を空にする
            EventTxt.enabled = false;
        }

        // レイがオブジェクトに当たった場合のみ処理を実行
        if (Physics.Raycast(ray, out hit))
        {
            // hit.collider が null でないかチェック
            if (hit.collider != null && hit.collider.CompareTag("teaCup"))
            {
                  // クリックしたカップのGameObjectを保存
                  hitCup = hit.collider.gameObject;
                  //イベントテキストを表示
                  EventTxt.enabled = true;

                // 既に Outline コンポーネントがあるか確認
                Outline outline = hitCup.GetComponent<Outline>();
                    if (outline == null)
                    {
                        // Outline コンポーネントがない場合は追加
                        outline = hitCup.AddComponent<Outline>();
                    }

                    // Outline を有効化
                    outline.enabled = true;
            }
        }
    }
}
