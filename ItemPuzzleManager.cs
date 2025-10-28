using System.Collections.Generic;
using UnityEngine;

public class ItemPuzzleManager : MonoBehaviour
{
    public static ItemPuzzleManager Instance { get; private set; }
    public static int currentStageIndex = 0;    // パズルをクリアした数

    [Header("段階別の正解パターン")]
    public List<PuzzleStage> puzzleStages = new();

    // 設置したアイテムリスト
    private static List<Dictionary<string, string>> placedItemNamesPerStage = new();

    void Awake()
    {
        if (Instance == null) Instance = this;

        // static データが未初期化なら初期化する
        if (placedItemNamesPerStage.Count == 0)
        {
            foreach (var stage in puzzleStages)
            {
                placedItemNamesPerStage.Add(new Dictionary<string, string>());
            }
        }

        Debug.Log("現在のパズルステージ：" + currentStageIndex);
    }

    // アイテムが設置されたときに呼ばれる
    public void ReportPlacement(GameObject location, GameObject item)
    {
        string itemName = item.name.Replace("(Clone)", "").Trim();
        string locationName = location.name.Trim();

        if (currentStageIndex >= placedItemNamesPerStage.Count) return;

        var stagePlacedItems = placedItemNamesPerStage[currentStageIndex];
        stagePlacedItems[locationName] = itemName;


        CheckPuzzleCompletion();
    }

    // 正解順と一致するかチェック
    private void CheckPuzzleCompletion()
    {
        if (currentStageIndex >= puzzleStages.Count) return;
        // 全部設置されていないなら何もしない

        var stage = puzzleStages[currentStageIndex];
        var stagePlacedItems = placedItemNamesPerStage[currentStageIndex];

        if (stagePlacedItems.Count < stage.installationLocations.Count) return;

        for (int i = 0; i < stage.installationLocations.Count; i++)
        {
            string locationName = stage.installationLocations[i].name.Trim();
            string expectedName = stage.correctItemNames[i];

            if (!stagePlacedItems.TryGetValue(locationName, out string actualName) || actualName != expectedName)
            {
                return;
            }
        }

        // 全部一致 → 正解
        OnPuzzleStageClear();
    }

    private void OnPuzzleStageClear()
    {
        Debug.Log($"パズル {currentStageIndex} をクリア");

        currentStageIndex++;

        if (currentStageIndex >= puzzleStages.Count)
        {
            Debug.Log("すべてのパズルをクリアしました！");
            // 最終クリア処理（例：ドアを開ける、アイテムを出現させる等）
        }
        else
        {
            // 次のステージの演出・アクティブ処理などがあればここで
            Debug.Log($"次のパズル {currentStageIndex} に進みます");
        }
    }

    // 謎解きをリセットしたい場合（任意）
    public void ResetPuzzle()
    {
        currentStageIndex = 0;

        // 各ステージの設置情報を初期化
        for (int i = 0; i < placedItemNamesPerStage.Count; i++)
        {
            placedItemNamesPerStage[i].Clear();
        }

        Debug.Log("パズル全体をリセットしました");
    }
}