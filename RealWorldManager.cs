using UnityEngine;

public class RealWorldManager : MonoBehaviour
{
    public GameObject book1, book2, book3;
    public GameObject Gate; //次のステージに行くためのゲート

    public GameObject Witch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.currentBookWorldIndex == 0)
        {
            book1.SetActive(true);
            book2.SetActive(false);
            book3.SetActive(false);
        }
        else if (GameManager.currentBookWorldIndex == 1)
        {
            book1.SetActive(true);
            book2.SetActive(true);
            book3.SetActive(false);
        }
        else if (GameManager.currentBookWorldIndex == 2)
        {
            book1.SetActive(true);
            book2.SetActive(true);
            book3.SetActive(true);
        }
        else if (GameManager.currentBookWorldIndex == 3)
        {
            book1.SetActive(true);
            book2.SetActive(true);
            book3.SetActive(true);
            Gate.gameObject.SetActive(true);
        }

        //次の部屋へ移動するためのゲートを非表示に
        if (GameManager.isGate == true)
        {
            Gate.gameObject.SetActive(true);
        }
        else
        {
            Gate.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
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
