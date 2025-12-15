using UnityEngine;
using UnityEngine.SceneManagement;

public class GateManager : MonoBehaviour
{
    //ゲートに触れた時の処理
    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene("LastStage");
    }
}
