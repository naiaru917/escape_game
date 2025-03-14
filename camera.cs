using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 200f; // マウス感度（後で設定画面で変更可能）
    private float xRotation = 0f;
    private GameObject hitItem;

    void Start()
    {
        // カーソルを非表示＆固定
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        // マウスの入力を取得
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 縦方向の回転（カメラの上下）
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 上下90度まで制限

        // 回転を適用
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.parent.Rotate(Vector3.up * mouseX); // 親オブジェクト（プレイヤー）を左右に回転



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

    // 設定画面で感度を変更するための関数
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
}
