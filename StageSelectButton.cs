using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour
{
    [SerializeField] private int StageNumber;  //�ړ��������V�[�������C���X�y�N�^�[����擾

    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClickStageSelect);
        }
    }

    private void OnClickStageSelect()
    {
        

        if (StageNumber == 3)
        {
            GameManager.isLastStage = true;
            GameManager.isGate = true;
        }
        else
        {
            GameManager.currentBookWorldIndex = StageNumber;
        }
        SceneManager.LoadScene("RealWorld");
    }
}
