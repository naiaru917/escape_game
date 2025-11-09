using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public GameObject CupA, CupB, CupC, CupD, CupE, CupF; // 各カップのゲームオブジェクト
    private Vector3 start_posA, start_posB, start_posC, start_posD, start_posE, start_posF;
    [SerializeField] private teaCupGimmick gimmick;

    void Start()
    {
        start_posA = gimmick.StartPosA.transform.position;
        start_posB = gimmick.StartPosB.transform.position;
        start_posC = gimmick.StartPosC.transform.position;
        start_posD = gimmick.StartPosD.transform.position;
        start_posE = gimmick.StartPosE.transform.position;
        start_posF = gimmick.StartPosF.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CupA.transform.position = start_posA;
            CupB.transform.position = start_posB;
            CupC.transform.position = start_posC;
            CupD.transform.position = start_posD;
            CupE.transform.position = start_posE;
            CupF.transform.position = start_posF;
            Debug.Log("Reset");
        }
    }
}
