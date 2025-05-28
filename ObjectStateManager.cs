using UnityEngine;

public class ObjectStateManager : MonoBehaviour
{
    public ObjectState objectState;
    public string objectName;  // オブジェクトの識別名

    private void Start()
    {
        // ゲーム開始時にデータをリセット（初回のみ）
        if (GameManager.isFirstRunCup)
        {
            objectState.ResetAllData();
            GameManager.isFirstRunCup = false;
        }

        // 初期位置が保存されていれば復元
        if (objectState.HasInitialPosition(objectName))
        {
            RestorePosition();
        }
    }

    private void Update()
    {
        if(GameManager.isSceneMove==true)
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
        objectState.SaveState(objectName, transform.position, transform.rotation);
        //Debug.Log($"[{objectName}] の位置を保存: {transform.position}");
    }

    private void RestorePosition()
    {
        if (objectState.TryGetState(objectName, out Vector3 savedPosition, out Quaternion savedRotation))
        {
            transform.position = savedPosition;
            transform.rotation = savedRotation;
            //Debug.Log($"[{objectName}] の位置を復元: {transform.position}");
        }
        else
        {
            //Debug.LogWarning($"[{objectName}] の保存された位置が見つかりません");
        }
    }
}
