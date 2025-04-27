using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class CupCakeGimmick : MonoBehaviour
{
    public GameObject CupCakeA, CupCakeB, CupCakeC;
    List<GameObject> targets_pos = new List<GameObject>(); // すべてのカップ
    List<bool> isOccupied;  //カップケーキが生成されていればTrue、されていないのならFalse
    private int targetCnt;

    void Start()
    {
        //カップケーキの出現場所（オブジェクト）を取得
        GetTargetPos();

        //生成場所が重ならないようにする
        isOccupied = new List<bool>();

        //生成場所に既にカップケーキがあるかの判定
        for (int i = 0; i < targets_pos.Count; i++)
        {
            isOccupied.Add(false);  //最初はすべてFalseを登録
        }


        //ランダムに5つ生成
        for (int i = 0; i < 5; i++)
        {
            //カップケーキを生成
            ComeOutTarget();

            //カップケーキの数を記録
            targetCnt++;
            Debug.Log("カップケーキの数：" + targetCnt);
        }
        

        targetCnt = 0;
    }

    void Update()
    {

    }

    void GetTargetPos()
    {
        targets_pos = GameObject.FindGameObjectsWithTag("TargetPos").ToList();

        //出現場所確認用
        //foreach(GameObject target in targets_pos)
        //{
        //    Vector3 potition = target.transform.position;
        //    potition.y += 0.25f;
        //    GameObject obj = Instantiate(CupCakeA, potition, Quaternion.identity);
        //}
    }

    void ComeOutTarget()
    {
        // 空きの場所をリストアップ
        List<int> emptyIndices = new List<int>();
        for (int i = 0; i < isOccupied.Count; i++)
        {
            if (!isOccupied[i]) emptyIndices.Add(i);
        }

        // 空いてる場所がないならreturn
        if (emptyIndices.Count == 0) return;

        // 空いてる場所からランダムで選ぶ
        int randomIndex = Random.Range(0, emptyIndices.Count);
        int selectedPos = emptyIndices[randomIndex];

        // 生成
        GameObject target = targets_pos[selectedPos];
        Vector3 position = target.transform.position;
        position.y += 0.25f;
        Instantiate(CupCakeA, position, Quaternion.identity);

        // 使用中マーク
        isOccupied[selectedPos] = true;
    }
}
