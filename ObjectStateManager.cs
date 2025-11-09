using UnityEngine;

public class ObjectStateManager : MonoBehaviour
{
    public ObjectState objectState;
    public string objectName;
    public int objectID;
    public bool isPrefab;
    public bool shouldSave = true;
    public WorldName worldName;


    private void Start()
    {
        // 初期位置が保存されていれば復元
        if (shouldSave)
        {
            RestorePosition();
        }
    }

    private void Update()
    {
        if (GameManager.isSceneMove == true)
        {
            // シーン遷移時に位置を保存
            if (Input.GetKeyDown(KeyCode.V))
            {
                SaveCurrentPosition();
            }
        }
    }

    private void SaveCurrentPosition()
    {
        if (!shouldSave) return;

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

        bool isMovable = true;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            isMovable = !rb.isKinematic; // リジッドボディが動けるかどうか保存


        objectState.SaveState(objectName, objectID, isPrefab, worldName, transform.position, transform.rotation, isMovable);
    }

    private void RestorePosition()
    {
        if (objectState.TryGetState(objectName, objectID, out Vector3 savedPosition, out Quaternion savedRotation, out bool isMovable))
        {
            transform.position = savedPosition;     //位置の反映
            transform.rotation = savedRotation;     //回転の反映

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = !isMovable; // ← 保存された isMovable に応じて物理挙動切替

            // 復元後、ObjectStateからデータを削除
             objectState.RemoveState(objectName, objectID);
            
        }
    }
}