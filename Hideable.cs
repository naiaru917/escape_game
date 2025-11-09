using UnityEngine;
using UnityEngine.Video;

public class Hideable : MonoBehaviour
{
    [SerializeField] private Transform hidingPoint; // 箱の中の位置
    [SerializeField] private Transform exitPoint;   // 箱から出る位置
    [SerializeField] private VideoClip caughtByWitchClip; // 魔女に捕まるムービー

    private GameObject player;
    private Rigidbody playerRb;
    private Collider playerCol;

    public void Enter(GameObject playerObj)
    {
        Debug.Log($"[Hideable] 呼び出し確認: gameObject={gameObject.name}, scene={gameObject.scene.name}, caughtByWitchClip={(caughtByWitchClip ? caughtByWitchClip.name : "null")}");

        // 魔女が存在していれば参照
        var witch = WitchManager.Instance?.CurrentWitch;

        // 魔女が追跡中なら捕獲イベントを即発火
        if (witch != null)
        {
            bool canSeeNow = witch.CanSeeSisterBrotherView(GameManager.Instance.sisterFootPoint.position);
            bool wasSeeingRecently = witch.WasSeeingPlayerRecently;
            Debug.Log($"[Hideable] 魔女状態: {witch.CurrentState}, 視界判定: {canSeeNow}, 最近見てた: {wasSeeingRecently}");

            // 魔女が追跡中か確認
            if (witch.CurrentState == WitchState.Chase)
            {
                Debug.Log("[Hideable] 魔女はChase状態です");

                // 「今見てる or 最近まで見てた」なら捕獲
                if (canSeeNow || wasSeeingRecently)
                {
                    Debug.Log("[Hideable] 魔女の視界内（または直前まで視界内）で隠れた → 捕獲ムービー再生処理開始");

                    // Witch AI停止（多重イベント防止）
                    WitchManager.Instance.DeactivateWitch();

                    // 捕獲ムービー設定を確認
                    if (caughtByWitchClip == null)
                    {
                        Debug.LogWarning("[Hideable] caughtByWitchClip が未設定。直接GameOverへ移行します。");
                        GameManager.Instance.TriggerGameOver("魔女に見られた状態で隠れた…");
                        return;
                    }

                    // MoviePlayer取得
                    var moviePlayer = MoviePlayer.Instance;
                    Debug.Log($"[Hideable] MoviePlayer.Instance={(moviePlayer != null ? "あり" : "なし")}");

                    if (moviePlayer != null)
                    {
                        moviePlayer.PlayMovie(caughtByWitchClip, () =>
                        {
                            Debug.Log("[Hideable] ムービー終了 → GameOver呼び出し");
                            GameManager.Instance.TriggerGameOver("魔女に見られた状態で隠れた…");
                        });
                    }
                    else
                    {
                        Debug.LogWarning("[Hideable] MoviePlayer.Instanceが見つからないため直接GameOver");
                        GameManager.Instance.TriggerGameOver("魔女に見られた状態で隠れた…");
                    }

                    return; // 捕獲時はこの先に進まない
                }
            }
            else
            {
                Debug.Log("[Hideable] 魔女はChase状態ではないため、通常の隠れ動作を実行");
            }
        }
        else
        {
            Debug.LogWarning("[Hideable] WitchManager.Instance?.CurrentWitch が null です");
        }

        // 通常の隠れ処理（捕獲されなかった場合）
        player = playerObj;
        playerRb = player.GetComponent<Rigidbody>();
        playerCol = player.GetComponent<Collider>();

        // 移動禁止
        PlayerController.isPlayerMove = false;
        GameManager.isSceneMove = false;

        // 物理を無効化
        if (playerRb != null) playerRb.isKinematic = true;
        if (playerCol != null) playerCol.enabled = false;

        // プレイヤーを隠れる位置へ移動
        player.transform.position = hidingPoint.position;
        player.transform.rotation = Quaternion.LookRotation(hidingPoint.forward);

        // 隠れ状態ON
        GameManager.isHiding = true;

        Debug.Log($"[Hideable] {name} に隠れた（通常処理）");
    }


    public void Exit()
    {
        if (player == null) return;

        // プレイヤーを出る位置へ移動
        player.transform.position = exitPoint.position;

        // 移動可能に戻す
        PlayerController.isPlayerMove = true;
        GameManager.isHiding = false;
        GameManager.isSceneMove = true;

        // コライダー復活
        if (playerRb != null) playerRb.isKinematic = false;
        if (playerCol != null) playerCol.enabled = true;

        // プレイヤーの向きを ExitPoint の forward に合わせる
        player.transform.rotation = Quaternion.LookRotation(exitPoint.forward);


        Debug.Log($"{name} から出た");

        player = null;
    }
}
