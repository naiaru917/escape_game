using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("ワールド範囲設定")]
    public GameObject PauseCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PauseCanvas = Instantiate(Resources.Load<GameObject>("Objects/PauseCanvas"));
        PauseCanvas.SetActive(false); // 初期状態で非表示にする

        if (PauseCanvas == null)
        {
            Debug.Log("キャンバスが見つからない");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            //ポーズ画面を表示
            TogglePauseMenu();
        }
    }

    void TogglePauseMenu()
    {
        if (GameManager.isPaused)
        {
            PauseCanvas.SetActive(false);

            //ゲームを再開
            GameManager.isPaused = false;
            GameManager.isSceneMove = true;
            CameraController.isCameraMove = true;

            //本を開いているか判定
            if (!ItemManager.BookCanvasAvtive)
            {
                //プレイヤーを移動可能に
                PlayerController.isPlayerMove = true;
            }

            if (CupCakeGimmick.isPlayCupCakeGame)
            {
                GameManager.isSceneMove = false;
            }

            // カーソルを非表示＆固定
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            PauseCanvas.SetActive(true);
            //ゲームを停止
            GameManager.isPaused = true;
            GameManager.isSceneMove = false;
            CameraController.isCameraMove = false;

            //プレイヤーを不移動可能に
            PlayerController.isPlayerMove = false;

            // カーソルを非表示＆固定を解除
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
