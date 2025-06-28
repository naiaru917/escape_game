using UnityEngine;

public class ObjectStateManager : MonoBehaviour
{
    public ObjectState objectState;
    public string objectName;
    public int objectID;
    public bool isPrefab;
    public WorldName worldName;


    private void Start()
    {
        // ゲーム開始時にデータをリセット（初回のみ）
        if (GameManager.isFirstRunCup)
        {
            objectState.ResetAllData();
            GameManager.isFirstRunCup = false;
        }

        // 初期位置が保存されていれば復元
        if (objectState.HasInitialPosition(objectName, objectID))
        {
            RestorePosition();
        }
    }

    private void Update()
    {
        if (GameManager.isSceneMove == true)
        {
            // 本の世界にいる間、Vキーで位置を保存
            if (GameManager.isInBookWorld && Input.GetKeyDown(KeyCode.V))
            {
                SaveCurrentPosition();
            }
        }
    }

    private void SaveCurrentPosition()
    {
        // 自動採番処理（isPrefab時）
        if (isPrefab && objectID <= 0)
        {
            objectID = 1;
            while (objectState.objectDataList.Exists(d =>
                d.isPrefab &&
                d.objectName == objectName &&
                d.objectID == objectID))
            {
                objectID++;
            }
        }

        objectState.SaveState(objectName, objectID, isPrefab, worldName, transform.position, transform.rotation);
    }

    private void RestorePosition()
    {
        if (objectState.TryGetState(objectName, objectID, out Vector3 savedPosition, out Quaternion savedRotation))
        {
            transform.position = savedPosition;     //位置の反映
            transform.rotation = savedRotation;     //回転の反映
        }
    }
}
