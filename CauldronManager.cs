using System.Collections.Generic;
using UnityEngine;

public class CauldronManager : MonoBehaviour
{
    // 正解の順番（インスペクターで設定）
    [SerializeField] private List<string> correctOrder = new List<string>();

    // 投入されたアイテム名の履歴
    private List<string> currentOrder = new List<string>();

    // 鍋に残っているアイテムの参照
    private List<GameObject> ingredientsInCauldron = new List<GameObject>();

    /// <summary>
    /// 鍋にアイテムを投入したときに呼ばれる
    /// </summary>
    public void AddIngredient(GameObject ingredient)
    {
        // アイテム名を記録（Prefab名やオブジェクト名を利用）
        string ingredientName = ingredient.name.Replace("(Clone)", "");
        currentOrder.Add(ingredientName);

        // Destroyせず鍋に残す
        ingredientsInCauldron.Add(ingredient);

        Debug.Log("鍋に投入: " + ingredientName);

        // 判定チェック（3つ揃ったら）
        if (currentOrder.Count == correctOrder.Count)
        {
            CheckOrder();
        }
    }

    private void CheckOrder()
    {
        bool isCorrect = true;

        for (int i = 0; i < correctOrder.Count; i++)
        {
            if (currentOrder[i] != correctOrder[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            Debug.Log("クリア！");
            // TODO: クリア演出やアイテム変身処理
        }
        else
        {
            Debug.Log("失敗…リセット");
            ResetCauldron();
        }
    }

    private void ResetCauldron()
    {
        Debug.Log("失敗！リセットします");
        foreach (var obj in ingredientsInCauldron)
        {
            if (obj != null) Destroy(obj);
        }
        ingredientsInCauldron.Clear();
        currentOrder.Clear();
    }
}
