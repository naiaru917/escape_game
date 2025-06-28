using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ObjectState", menuName = "ScriptableObjects/ObjectState")]
public class ObjectState : ScriptableObject
{
    [System.Serializable]
    public class ObjectData
    {
        public string objectName;           //オブジェクト名
        public int objectID;                //同じオブジェクト名のプレハブを生成したときに判別するためのID
        public bool isPrefab;               //プレハブから生成されたオブジェクトか
        public WorldName worldName;         //どのシーンに存在しているオブジェクトか
        public Vector3 position;            //オブジェクトの位置情報
        public Quaternion rotation;         //オブジェクトの回転情報
        public bool initialPositionSaved;   //初期位置を保存したか（初回のみ初期位置を保存）
    }

    public List<ObjectData> objectDataList = new List<ObjectData>();    //データをリスト形式で登録

    public void SaveState(string objectName, int objectID, bool isPrefab, WorldName worldName, Vector3 position, Quaternion rotation)
    {
        // ID重複チェック処理（isPrefabがtrueのときのみ）
        if (isPrefab)
        {
            bool idConflict = objectDataList.Any(d =>
                d.isPrefab &&
                d.objectName == objectName &&
                d.objectID == objectID);

            if (idConflict)
            {
                // 被ってないIDを探して上書き
                int newId = objectID;
                while (objectDataList.Any(d =>
                    d.isPrefab &&
                    d.objectName == objectName &&
                    d.objectID == newId))
                {
                    newId++;
                }

                objectID = newId;
            }
        }

        //オブジェクトのデータを保存
        var data = objectDataList.Find(d => d.objectName == objectName && d.objectID == objectID);

        if (data == null)
        {
            //データが空の場合
            data = new ObjectData
            {
                objectName = objectName,
                objectID = objectID,
                isPrefab = isPrefab,
                worldName = worldName,
                position = position,
                rotation = rotation,
                initialPositionSaved = true
            };
            objectDataList.Add(data);
        }
        else
        {
            //既にデータがある場合（位置・回転情報を更新）
            data.position = position;
            data.rotation = rotation;
            data.initialPositionSaved = true;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

    public bool TryGetState(string objectName, int objectID, out Vector3 position, out Quaternion rotation)
    {
        var data = objectDataList.Find(d => d.objectName == objectName && d.objectID == objectID);
        if (data != null)
        {
            position = data.position;
            rotation = data.rotation;
            return true;
        }

        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;
    }

    public bool HasInitialPosition(string objectName, int objectID)
    {
        var data = objectDataList.Find(d => d.objectName == objectName && d.objectID == objectID);
        return data != null && data.initialPositionSaved;
    }

    public void ResetAllData()
    {
        //Debug.Log("ObjectStateのデータをリセットします");
        objectDataList.Clear();
    }
}
