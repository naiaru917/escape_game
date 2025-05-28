using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ObjectState", menuName = "ScriptableObjects/ObjectState")]
public class ObjectState : ScriptableObject
{
    [System.Serializable]
    public class ObjectData
    {
        public string objectName;
        public Vector3 position;
        public Quaternion rotation;
        public bool initialPositionSaved;
    }

    public List<ObjectData> objectDataList = new List<ObjectData>();

    public void SaveState(string objectName, Vector3 position, Quaternion rotation)
    {
        var data = objectDataList.Find(d => d.objectName == objectName);
        if (data == null)
        {
            data = new ObjectData
            {
                objectName = objectName,
                position = position,
                rotation = rotation,
                initialPositionSaved = true
            };
            objectDataList.Add(data);
        }
        else
        {
            data.position = position;
            data.rotation = rotation;
            data.initialPositionSaved = true;
        }

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        #endif

        //Debug.Log($"[{objectName}]の状態を保存しました: {position}");
    }

    public bool TryGetState(string objectName, out Vector3 position, out Quaternion rotation)
    {
        var data = objectDataList.Find(d => d.objectName == objectName);
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

    public bool HasInitialPosition(string objectName)
    {
        var data = objectDataList.Find(d => d.objectName == objectName);
        return data != null && data.initialPositionSaved;
    }

    public void ResetAllData()
    {
        //Debug.Log("ObjectStateのデータをリセットします");
        objectDataList.Clear();
    }
}
