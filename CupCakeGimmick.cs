using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CupCakeGimmick : MonoBehaviour
{
    public GameObject CupCakeA, CupCakeB, CupCakeC;
    List<GameObject> targets_pos = new List<GameObject>(); // カップケーキの出現場所
    List<bool> isOccupied;  //カップケーキが生成されていればTrue、されていないのならFalse

    // 出現数の管理
    public static int totalCnt;
    public static int cntA;
    public static int cntB;
    public static int cntC;

    // 生成タイマー
    float spawnTimer = 0f;
    float nextSpawnDelay = 0f;

    // ゲーム進行用
    public float gameTime = 20f; // 制限時間
    private float currentTime;
    public Text timeText;  // Canvas 内の TimeText
    public Text scoreText; // Canvas 内の ScoreText
    private int score = 0;
    private bool isGameOver = false;
    public static bool isPlayCupCakeGame;

    public GameObject Gate; //次のステージに行くためのゲート

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

        totalCnt = cntA = cntB = cntC = 0;

        // 最初のディレイをランダムに設定（0～1秒の間）
        nextSpawnDelay = Random.Range(0f, 0.2f);

        // 時間初期化
        currentTime = gameTime;
        UpdateUI();

        // ミニゲームが終わるまでシーン移動を不可に
        GameManager.isSceneMove = false;

        // プレイヤーの移動を不可に
        PlayerController.isPlayerMove = false;

        isPlayCupCakeGame = true;

        //次の部屋へ移動するためのゲートを非表示に
        if (GameManager.isGate == true)
        {
            Gate.gameObject.SetActive(true);
        }
        else
        {
            Gate.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isGameOver) return;

        if (!GameManager.isPaused)
        {
            // 時間を進める
            spawnTimer += Time.deltaTime;

            // --- カウントダウン ---
            currentTime -= Time.deltaTime;
            if (currentTime < 0) currentTime = 0;
            UpdateUI();

            if (currentTime <= 0)
            {
                EndMiniGame();
                return;
            }

            // --- 出現処理 ---
            if (totalCnt < 40 && spawnTimer >= nextSpawnDelay)  //カップケーキが20個以内の場合に生成
            {
                //カップケーキを生成
                ComeOutTarget();

                // タイマーリセット + 次回の待ち時間をランダムに
                spawnTimer = 0f;
                nextSpawnDelay = Random.Range(0.3f, 1.0f);
            }
        }
    }

    void GetTargetPos()
    {
        targets_pos = GameObject.FindGameObjectsWithTag("TargetPos").ToList();
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

        // 出す種類を選ぶ
        GameObject prefab = SelectCupCakeType(out CupCakeType type);

        if (prefab == null) return; // 制限で出せないとき

        Vector3 position = targets_pos[selectedPos].transform.position;
        Quaternion rotarion = targets_pos[selectedPos].transform.rotation;
        position.y -= 0.15f;
        GameObject obj = Instantiate(prefab, position, rotarion);

        // CupCakeManagerに情報を渡す
        CupCakeManager manager = obj.GetComponent<CupCakeManager>();
        manager.gimmick = this;
        manager.myIndex = selectedPos;
        manager.myType = type;

        // カウント更新
        totalCnt++;
        switch (type)
        {
            case CupCakeType.A: cntA++; break;
            case CupCakeType.B: cntB++; break;
            case CupCakeType.C: cntC++; break;
        }

        // 使用中マーク
        isOccupied[selectedPos] = true;
    }

    // 種類を選ぶ処理
    GameObject SelectCupCakeType(out CupCakeType type)
    {
        type = CupCakeType.A;

        // ランダム選択（優先度や確率は後で調整可）
        int r = Random.Range(0, 3); // 0=A,1=B,2=C

        if (r == 1 && cntB < 3)
        {
            type = CupCakeType.B;
            return CupCakeB;
        }
        else if (r == 2 && cntC < 1)
        {
            type = CupCakeType.C;
            return CupCakeC;
        }
        else
        {
            type = CupCakeType.A;
            return CupCakeA;
        }
    }

    // 消滅時に呼ばれる
    public void TargetDestroyed(int index, CupCakeType type)
    {
        isOccupied[index] = false;
        totalCnt--;

        switch (type)
        {
            case CupCakeType.A: cntA--; break;
            case CupCakeType.B: cntB--; break;
            case CupCakeType.C: cntC--; break;
        }
    }

    // ▼ スコア加算用 ▼
    public void AddScore(int value)
    {
        score += value;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (timeText != null)
            timeText.text = "Time: " + Mathf.CeilToInt(currentTime).ToString();
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
    }

    void EndMiniGame()
    {
        isGameOver = true;
        timeText.enabled = false;

        ClearAllCupCakes(); // 残っているカップケーキをすべて削除

        if (score >= 200)
        {
            Debug.Log("成功！ SCORE: " + score);
            GameManager.isGate = true;
            Gate.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("失敗... SCORE: " + score);
        }

        // シーン移動を可能に
        GameManager.isSceneMove = true;

        isPlayCupCakeGame = false;
    }

    void ClearAllCupCakes()
    {
        // シーン内にある全ての CupCake タグのオブジェクトを取得
        GameObject[] allCupCakes = GameObject.FindGameObjectsWithTag("CupCake");

        foreach (GameObject cake in allCupCakes)
        {
            // 出現元にも通知してカウントを減らす
            CupCakeManager manager = cake.GetComponent<CupCakeManager>();
            if (manager != null)
            {
                TargetDestroyed(manager.myIndex, manager.myType);
            }

            // 自分自身を削除
            Destroy(cake);
        }
    }

    // 種類を表すEnum
    public enum CupCakeType { A, B, C }
}
