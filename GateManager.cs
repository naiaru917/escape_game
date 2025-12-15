using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GateManager : MonoBehaviour
{
    //ゲートに触れた時の処理
    private void OnTriggerEnter(Collider other)
    {
        ItemManager.BookCanvasAvtive = false;
        PlayerController.isPlayerMove = true;
        GameManager.isInBookWorld = false;
        

        // カーソルを非表示＆固定
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (GameManager.isLastStage)
        {
            GameManager.isInBookWorld = false;
            SceneManager.LoadScene("LastPassage");
            return;
        }
        
        if (GameManager.currentBookWorldIndex < 2)
        {
            //一度現実世界（妹視点）に戻る
            SceneManager.LoadScene("RealWorld");
            GameManager.currentBookWorldIndex++;
            GameManager.isGate = false;
        }
        else if (GameManager.currentBookWorldIndex==2)
        {
            SceneManager.LoadScene("RealWorld");
            GameManager.isLastStage = true;
            GameManager.isGate = true;
        }
    }
}

