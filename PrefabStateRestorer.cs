using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PrefabStateRestorer : MonoBehaviour
{
    public ObjectState objectState;                // 保存されている ScriptableObject
    public List<GameObject> prefabList;            // 生成対象のプレハブリスト

    void Start()
    {
        RestorePrefabsFromState();
    }

    private void RestorePrefabsFromState()
    {
        // データを複製して安全にイテレート
        var dataList = objectState.objectDataList.ToList();

        foreach (var data in dataList)
        {
            // プレハブのみ処理
            if (!data.isPrefab)
                continue;

            // プレハブを名前で検索
            GameObject prefabToSpawn = prefabList.Find(p => p.name == data.objectName);
            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"Prefab '{data.objectName}' が見つかりませんでした。");
                continue;
            }

            // プレハブをシーン上に復元
            GameObject spawned = Instantiate(prefabToSpawn, data.position, data.rotation);
            spawned.name = prefabToSpawn.name;

            // IceCreamGimmick に登録（存在すれば）
            IceCreamGimmick gimmick = FindFirstObjectByType<IceCreamGimmick>();
            if (gimmick != null)
            {
                gimmick.RegisterRestoredIce(spawned);
            }

            // ObjectState から復元済みデータを削除
            objectState.objectDataList.Remove(data);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(objectState);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
}
