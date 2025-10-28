using UnityEngine;

public class Hideable : MonoBehaviour
{
    [SerializeField] private Transform hidingPoint; // 箱の中の位置
    [SerializeField] private Transform exitPoint;   // 箱から出る位置

    private GameObject player;
    private Rigidbody playerRb;
    private Collider playerCol;

    public void Enter(GameObject playerObj)
    {
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
        player.transform.rotation = Quaternion.LookRotation(transform.forward);

        // 移動禁止・敵から見えなくする
        PlayerController.isPlayerMove = false;
        GameManager.isHiding = true;

        // プレイヤーの向きを HidingPoint の forward に合わせる
        player.transform.rotation = Quaternion.LookRotation(hidingPoint.forward);

        Debug.Log($"{name} に隠れた");
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
