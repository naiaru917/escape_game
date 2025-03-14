using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Gimmick : MonoBehaviour
{
    private Camera mainCamera;

    public Transform stage; //stageBlockをまとめている親オブジェクト
    private Transform[,] stageBlock = new Transform[5, 5]; // 5x5のグリッド
    public LayerMask groundLayer; // 5×5の土台のレイヤー

    public GameObject CupA, CupB, CupC, CupD; // 各カップのゲームオブジェクト

    private GameObject selectCup; //選択中のカップ
    private Vector2Int selectCup_pos;　//選択したカップの位置

    private List<GameObject> allCups = new List<GameObject>(); // すべてのカップ
    private List<GameObject> otherCups = new List<GameObject>(); // 選択していないのカップ
    List<Vector2Int> otherCups_pos = new List<Vector2Int>(); // 選択していないのカップ

    private string direction;   //移動したい方向
    private Vector2Int target_pos;  //移動予定のマス
    private int moveCnt;    //移動可能なマス数

    private Vector3 selectCup_posV3;   //選択したカップの座標
    private Vector3 target_posV3;   //移動したい目的の座標

    private float moveSpeed = 3f; //カップの移動速度
    private bool isMove;    //カップの移動を行っているかの判定
    private bool isDo;      //複数のカップを選択させない処理

    //カップの初期位置
    public static Vector3
        start_posA = new Vector3(-3f, 3f, 0f),
        start_posB = new Vector3(-6f, 3f, 9f),
        start_posC = new Vector3(0f, 3f, 12f),
        start_posD = new Vector3(6f, 3f, 6f);

    //カップのゴール位置
    public static Vector3
        goal_posA = new Vector3(-6f, 1.25f, 12f),
        goal_posB = new Vector3(6f, 1.25f, 6f),
        goal_posC = new Vector3(-6f, 1.25f, 3f),
        goal_posD = new Vector3(3f, 1.25f, 0f);

    void Start()
    {
        mainCamera = Camera.main;

        //マス目の取得
        InitializeGrid();

        // カップ4つを取得（タグ "Cup" から取得）
        allCups = GameObject.FindGameObjectsWithTag("teaCup").ToList();
        otherCups = allCups;

        isDo = true;

        CupA.transform.position = start_posA;
        CupB.transform.position = start_posB;
        CupC.transform.position = start_posC;
        CupD.transform.position = start_posD;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDo == true)
        {


            if (Input.GetMouseButtonDown(0))
            {
                //カップを選択
                SelectCup();

                //カップが選択されているとき
                if (selectCup != null)
                {
                    // otherCupsから選択したカップを除外
                    otherCups.Remove(selectCup);

                    //選択したカップの位置情報を取得
                    selectCup_pos = blockNameToVec2Int(GetBlockName(selectCup));

                    //選択していないカップの位置情報を取得
                    GetOtherCupsPos();

                    //移動したい方向を取得
                    direction = GetDirection(selectCup.transform.position);

                    //何マス移動できるか計算
                    CntMove(direction);

                    //移動先の座標を取得
                    getTargetPos(target_pos);

                    //移動を開始
                    isMove = true;
                }

                // otherCupをリセット
                otherCups.Add(selectCup);

                // moveCntをリセット
                moveCnt = 0;

            }
        }

        if (isMove == true)
        {
            //選択したカップを移動
            moveCup();
        }

        if (selectCup != null)
        {
            //カップの移動が終わったら
            if (selectCup.transform.position == target_posV3)
            {
                isDo = true;
            }
        }

        if (CupA.transform.position == goal_posA && CupB.transform.position == goal_posB &&
            CupC.transform.position == goal_posC && CupD.transform.position == goal_posD)
        {
            Debug.Log("GameClear!!");
        }
    }

    //マスの情報を取得(マス名とそのtransform)
    void InitializeGrid()
    {
        foreach (Transform child in stage)
        {
            if (child != stage) // 親オブジェクト自身を除外
            {
                char row = child.name[0]; // A, B, C, D, E
                int col = int.Parse(child.name.Substring(1)); // 1, 2, 3...（1-based）

                // 0ベースのインデックスに変換
                int rowIndex = row - 'A'; // A → 0, B → 1, ..., E → 4
                int colIndex = col - 1;   // 1 → 0, 2 → 1, ..., 5 → 4

                stageBlock[rowIndex, colIndex] = child;
            }
        }
    }

    // 画面中央のオブジェクトを検出し、カップの選択を処理
    void SelectCup()
    {
        // 画面中央のスクリーン座標を取得
        Vector3 centerScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // 画面中央のスクリーン座標をワールド座標に変換
        Ray ray = mainCamera.ScreenPointToRay(centerScreenPosition);
        RaycastHit hit;

        // レイがオブジェクトに当たった場合のみ処理を実行
        if (Physics.Raycast(ray, out hit))
        {
            // hit.collider が null でないかチェック
            if (hit.collider != null && hit.collider.CompareTag("teaCup"))
            {
                // クリックしたカップのGameObjectを保存
                selectCup = hit.collider.gameObject;
                isDo = false;
                selectCup.AddComponent<Outline>();
            }
        }

    }

    // カメラからオブジェクトへの方向を判定
    private string GetDirection(Vector3 objectPosition)
    {
        // カメラからオブジェクトへのベクトルを計算
        Vector3 direction = objectPosition - mainCamera.transform.position;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            // X軸方向が強い場合
            if (direction.x > 0)
                return "X+";
            else
                return "X-";
        }
        else
        {
            // Z軸方向が強い場合
            if (direction.z > 0)
                return "Z+";
            else
                return "Z-";
        }
    }

    // カップの位置から下方向にRayを飛ばし、オブジェクト名を返す
    string GetBlockName(GameObject cup)
    {
        RaycastHit hit;

        // カップの位置から下方向にRayを発射
        Ray ray = new Ray(cup.transform.position + Vector3.up * 1.0f, Vector3.down);

        // Rayが何かに当たったかどうかを確認
        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            return hit.collider.gameObject.name; // ヒットしたオブジェクトの名前を返す
        }
        else
        {
            return "No block hit"; // ヒットしなかった場合
        }
    }

    // ブロック名から座標形式に変換する
    Vector2Int blockNameToVec2Int(string blockName)
    {
        if (blockName == "No block hit")
        {
            return Vector2Int.zero; // ヒットしなかった場合
        }

        // 名前から行と列を取得
        char row = blockName[0]; // A, B, C, D, E
        int col = int.Parse(blockName.Substring(1)); // 1, 2, 3...（1-based）

        // 行と列を0ベースに変換
        int rowIndex = row - 'A'; // A → 0, B → 1, ..., E → 4
        int colIndex = col - 1;   // 1 → 0, 2 → 1, ..., 5 → 4

        // 座標形式で返す
        return new Vector2Int(rowIndex, colIndex); // Vector2形式で返す
    }

    void GetOtherCupsPos()
    {
        // 座標リストをクリア
        otherCups_pos.Clear();

        // 各カップの座標を取得
        foreach (GameObject cup in otherCups)
        {
            if (cup != null) // 念のためNullチェック
            {
                string blockName = GetBlockName(cup); // ゲームオブジェクト名からブロック名を取得
                Vector2Int position = blockNameToVec2Int(blockName); // ブロック名をVector2Intの座標に変換
                otherCups_pos.Add(position); // 座標リストに追加
            }
        }
    }

    int CntMove(string actionDirection)
    {
        Vector2Int currentPos = selectCup_pos;  // 選択されたカップの現在の座標
        Vector2Int NextPos = currentPos;  // カップの隣の座標

        target_pos = selectCup_pos;
        // アクション方向によって判定を分岐
        switch (actionDirection)
        {
            case "X+": // 右方向（X+）：現在のy座標からy+=1の位置に他のカップが存在していないかを判定

                for (int i = (int)NextPos.y; i < 4; i++)
                {
                    NextPos.y += 1; // 現在のy座標 + 1

                    for (int j = 0; j < otherCups.Count; j++)
                    {
                        if (NextPos == otherCups_pos[j])
                        {
                            return moveCnt = 0; // 移動不可
                        }
                    }
                    moveCnt++;
                    target_pos.y += 1;

                }
                return moveCnt;


            case "X-": // 左方向（X-）：現在のy座標からy-=1の位置に他のカップが存在していないかを判定

                for (int i = (int)NextPos.y; i > 0; i--)
                {
                    NextPos.y -= 1; // 現在のy座標 - 1

                    for (int j = 0; j < otherCups.Count; j++)
                    {
                        if (NextPos == otherCups_pos[j])
                        {
                            return moveCnt = 0; // 移動不可
                        }
                    }
                    moveCnt++;
                    target_pos.y -= 1;
                }
                return moveCnt;


            case "Z+": // 手前方向（Z+）：現在のx座標からx-=1の位置に他のカップのx座標が存在していないかを判定

                for (int i = (int)NextPos.x; i > 0; i--)
                {
                    NextPos.x -= 1; // 現在のx座標 - 1

                    for (int j = 0; j < otherCups.Count; j++)
                    {
                        if (NextPos == otherCups_pos[j])
                        {
                            return moveCnt = 0; // 移動不可
                        }
                    }
                    moveCnt++;
                    target_pos.x -= 1;
                }
                return moveCnt;


            case "Z-": // 奥方向（Z-）：現在のx座標からx+=1の位置に他のカップのx座標が存在していないかを判定

                for (int i = (int)NextPos.x; i < 4; i++)
                {
                    NextPos.x += 1; // 現在のx座標 + 1

                    for (int j = 0; j < otherCups.Count; j++)
                    {
                        if (NextPos == otherCups_pos[j])
                        {
                            return moveCnt = 0; // 移動不可
                        }
                    }
                    moveCnt++;
                    target_pos.x += 1;
                }
                return moveCnt;

            default:
                return moveCnt = 0; // 不明な方向の場合は移動不可
        }
    }

    void moveCup()
    {
        selectCup_posV3 = selectCup.transform.position;

        // 新しい座標を計算
        Vector3 newDirection = target_posV3 - selectCup_posV3;
        // 一定の速度で移動
        if (newDirection.magnitude > 0.1f)  // 移動がまだ完了していない場合
        {
            // カップをスムーズに移動させる
            selectCup.transform.position = Vector3.MoveTowards(selectCup_posV3, target_posV3, moveSpeed * Time.deltaTime);
        }
        else
        {
            // カップがターゲット位置に到達したら
            selectCup.transform.position = target_posV3; // 正確にターゲット位置に移動
            isMove = false;
        }
    }
    Vector3 getTargetPos(Vector2Int target_pos)
    {
        target_posV3 = stageBlock[target_pos.x, target_pos.y].transform.position;
        target_posV3.y += 1f;
        return target_posV3;
    }

    
}