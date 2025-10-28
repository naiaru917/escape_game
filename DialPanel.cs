using TMPro;
using UnityEngine;

public class DialPanel : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private int currentValue = 0;
    [SerializeField] private int panelIndex = 0; // ���̃p�l�������Ԗڂ��i0,1,2�j���C���X�y�N�^�Őݒ�
    public int CurrentValue => currentValue; // �O������ǂݎ��p

    private void Start()
    {
        UpdateDisplay();
    }

    public void Increment()
    {
        currentValue = (currentValue + 1) % 10; // 0��9���[�v
        UpdateDisplay();

        // static�ϐ��ɕۑ��i�V�[�����ׂ��ł��ێ��j
        DialLock.dialValues[panelIndex] = currentValue;
    }

    // DialLock.Start() �ŕ�������Ƃ��p
    public void SetValue(int value)
    {
        currentValue = value;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (textMesh != null)
        {
            textMesh.text = currentValue.ToString();
        }
    }
}
