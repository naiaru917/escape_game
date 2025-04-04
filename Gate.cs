using UnityEngine;
using UnityEngine.SceneManagement;

public class Gate : MonoBehaviour
{
    int stageNum;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stageNum = Setting.stageNum;

            switch (stageNum)
            {
                case 0:
                    SceneManager.LoadScene("Stage2");
                    Setting.stageNum++;
                    break;
                case 1:
                    SceneManager.LoadScene("Stage1");
                    Setting.stageNum=0;
                    break;

            }
        }
    }
}
