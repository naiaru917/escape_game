using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public GameObject CupA, CupB, CupC, CupD; // 各カップのゲームオブジェクト

    //カップの初期位置
    public static Vector3
        start_posA = new Vector3(-3f, 3f, 0f),
        start_posB = new Vector3(-6f, 3f, 9f),
        start_posC = new Vector3(0f, 3f, 12f),
        start_posD = new Vector3(6f, 3f, 6f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CupA.transform.position = start_posA;
            CupB.transform.position = start_posB;
            CupC.transform.position = start_posC;
            CupD.transform.position = start_posD;
            Debug.Log("Reset");
        }
    }
}
