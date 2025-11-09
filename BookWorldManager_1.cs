using UnityEngine;

public class BookWorldManager_1 : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            var witchMgr = WitchManager.Instance;
            if (witchMgr != null && witchMgr.CurrentWitch != null)
            {
                // 魔女が非アクティブなら出現させる
                if (!witchMgr.isWitchActive || !witchMgr.CurrentWitch.gameObject.activeInHierarchy)
                {
                    witchMgr.ActivateWitch();
                    Debug.Log("魔女を出現させました（絵本世界・Eキー押下）");
                }
                else
                {
                    Debug.Log("魔女はすでに出現しています");
                }
            }
            else
            {
                Debug.LogWarning("魔女オブジェクトが WitchManager に登録されていません");
            }
        }
    }
}
