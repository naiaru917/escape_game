using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class DialLock : MonoBehaviour
{
    [SerializeField] private DialPanel[] panels; // 3つのパネルをインスペクタでセット
    [SerializeField] private int[] correctCode = { 3, 7, 5 }; // 正解の数字

    public TextMeshPro[] dialTexts;   // 3つのダイヤルのTextMeshPro参照
    public static int[] dialValues = new int[3];  // シーンをまたいで保持する値

    public static  bool isUnlocked = false; // 開いたらtrueになる
    public Animator openBox;
    private void Start()
    {
        // もし既に開いていたら、見た目を開いた状態にする
        if (isUnlocked)
        {
            SetOpenedState();
        }

        // 数字の状態を復元
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetValue(dialValues[i]); // DialPanel 側も同期
        }
    }

    public void CheckAnswer()
    {
        if (isUnlocked) return; // 既に解錠済みならスキップ

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i].CurrentValue != correctCode[i])
                return; // 1つでも不一致なら終了
        }

        // 全部一致したら解錠処理
        isUnlocked = true;
        OpenBox();
    }

    private void OpenBox()
    {
        Debug.Log("箱が開いた！");
        openBox.SetTrigger("OpenBox");
    }

    private void SetOpenedState()
    {
        // アニメーションの最後のフレームで表示（蓋が空いている状態）
        openBox.Play("OpenBox", 0, 1f);
    }

    public bool IsUnlocked => isUnlocked;
}
