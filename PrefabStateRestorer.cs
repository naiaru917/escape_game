using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PrefabStateRestorer : MonoBehaviour
{
    public ObjectState objectState;                // �ۑ�����Ă��� ScriptableObject
    public List<GameObject> prefabList;            // �����Ώۂ̃v���n�u���X�g

    void Start()
    {
        RestorePrefabsFromState();
    }

    private void RestorePrefabsFromState()
    {
        // �f�[�^�𕡐����Ĉ��S�ɃC�e���[�g
        var dataList = objectState.objectDataList.ToList();

        foreach (var data in dataList)
        {
            // �v���n�u�̂ݏ���
            if (!data.isPrefab)
                continue;

            // �v���n�u�𖼑O�Ō���
            GameObject prefabToSpawn = prefabList.Find(p => p.name == data.objectName);
            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"Prefab '{data.objectName}' ��������܂���ł����B");
                continue;
            }

            // �v���n�u���V�[����ɕ���
            GameObject spawned = Instantiate(prefabToSpawn, data.position, data.rotation);
            spawned.name = prefabToSpawn.name;

            // IceCreamGimmick �ɓo�^�i���݂���΁j
            IceCreamGimmick gimmick = FindFirstObjectByType<IceCreamGimmick>();
            if (gimmick != null)
            {
                gimmick.RegisterRestoredIce(spawned);
            }

            // ObjectState ���畜���ς݃f�[�^���폜
            objectState.objectDataList.Remove(data);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(objectState);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
}
