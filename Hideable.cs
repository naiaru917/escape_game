using UnityEngine;

public class Hideable : MonoBehaviour
{
    [SerializeField] private Transform hidingPoint; // ���̒��̈ʒu
    [SerializeField] private Transform exitPoint;   // ������o��ʒu

    private GameObject player;
    private Rigidbody playerRb;
    private Collider playerCol;

    public void Enter(GameObject playerObj)
    {
        player = playerObj;
        playerRb = player.GetComponent<Rigidbody>();
        playerCol = player.GetComponent<Collider>();

        // �ړ��֎~
        PlayerController.isPlayerMove = false;
        GameManager.isSceneMove = false;

        // �����𖳌���
        if (playerRb != null) playerRb.isKinematic = true;
        if (playerCol != null) playerCol.enabled = false;

        // �v���C���[���B���ʒu�ֈړ�
        player.transform.position = hidingPoint.position;
        player.transform.rotation = Quaternion.LookRotation(transform.forward);

        // �ړ��֎~�E�G���猩���Ȃ�����
        PlayerController.isPlayerMove = false;
        GameManager.isHiding = true;

        // �v���C���[�̌����� HidingPoint �� forward �ɍ��킹��
        player.transform.rotation = Quaternion.LookRotation(hidingPoint.forward);

        Debug.Log($"{name} �ɉB�ꂽ");
    }

    public void Exit()
    {
        if (player == null) return;

        // �v���C���[���o��ʒu�ֈړ�
        player.transform.position = exitPoint.position;

        // �ړ��\�ɖ߂�
        PlayerController.isPlayerMove = true;
        GameManager.isHiding = false;
        GameManager.isSceneMove = true;

        // �R���C�_�[����
        if (playerRb != null) playerRb.isKinematic = false;
        if (playerCol != null) playerCol.enabled = true;

        // �v���C���[�̌����� ExitPoint �� forward �ɍ��킹��
        player.transform.rotation = Quaternion.LookRotation(exitPoint.forward);


        Debug.Log($"{name} ����o��");

        player = null;
    }
}
