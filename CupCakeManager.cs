using UnityEngine;
using static CupCakeGimmick;

public class CupCakeManager : MonoBehaviour
{
    public CupCakeGimmick gimmick; // 呼び出し元を登録する
    public float lifeTime;  // 出現後の寿命（秒）
    private float remainingLifeTime; // 残り時間を管理する変数

    public int myIndex;             // 自分が占領してた出現ポイント番号
    public CupCakeType myType;      // 自分の種類を記録

    private Animator cupCakeAnim;
    private bool isDestroyed = false; // 削除処理開始済みフラグ

    void Start()
    {
        remainingLifeTime = lifeTime;

        cupCakeAnim = GetComponent<Animator>();

        // 出てくるアニメーション再生
        cupCakeAnim.SetTrigger("UpTrigger");
    }

    void Update()
    {
        // ポーズ中は処理を止める
        if (GameManager.isPaused) return;
        if (isDestroyed) return;

        // 時間を減らす
        remainingLifeTime -= Time.deltaTime;

        // 0以下になったら削除
        if (remainingLifeTime <= 0f)
        {
            BeginGoBack();
        }
    }

    void BeginGoBack()
    {
        // クリックされた場合はすでにDestroyされるので何もしない
        if (this == null) return;

        // Downトリガーを送る（Destroyはアニメーションイベントで実行）
        cupCakeAnim.SetTrigger("DownTrigger");
    }

    public void OnMouseDown()
    {
        if (isDestroyed) return;  // すでに削除中なら二重処理しない

        isDestroyed = true;       // フラグ立て

        switch (myType)
        {
            case CupCakeType.A: gimmick.AddScore(10); break;
            case CupCakeType.B: gimmick.AddScore(30); break;
            case CupCakeType.C: gimmick.AddScore(50); break;
        }

        // 出現元に自分が消えることを伝える
        gimmick.TargetDestroyed(myIndex, myType);

        // クリック時は即削除
        Destroy(gameObject);
    }
    public void OnDownAnimEnd()
    {
        // 出現元に自分が消えることを伝える
        gimmick.TargetDestroyed(myIndex, myType);

        // 自分を削除
        Destroy(gameObject);
    }

}
