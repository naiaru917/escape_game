using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 300f; // マウス感度（後で設定画面で変更可能）
    private float xRotation = 0;
    public static bool isCameraMove = true;    //視点移動可能か


    void Start()
    {
        // カーソルを非表示＆固定
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetInitialXRotation(float value)
    {
        // -90～90の範囲に正規化
        if (value > 180f) value -= 360f;
        xRotation = Mathf.Clamp(value, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void Update()
    {
        if (isCameraMove == true)
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
        }
    }

    // 設定画面で感度を変更するための関数
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
}
