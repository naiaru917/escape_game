using TMPro;
using UnityEngine;

public class DialPanel : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private int currentValue = 0;
    [SerializeField] private int panelIndex = 0; // このパネルが何番目か（0,1,2）をインスペクタで設定
    public int CurrentValue => currentValue; // 外部から読み取り用

    private void Start()
    {
        UpdateDisplay();
    }

    public void Increment()
    {
        currentValue = (currentValue + 1) % 10; // 0→9ループ
        UpdateDisplay();

        // static変数に保存（シーンを跨いでも保持）
        DialLock.dialValues[panelIndex] = currentValue;
    }

    // DialLock.Start() で復元するとき用
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
