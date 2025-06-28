using UnityEngine;
using UnityEngine.UI;

public class ItemUsePoint : MonoBehaviour
{
    public Text EventTxt;
    private bool isPlayerInUsePoint = false;    //アイテム使用可能地点にプレイヤーがいるか

    void Start()
    {
        EventTxt = GameObject.Find("EventText").GetComponent<Text>();
    }

    void Update()
    {
        if (isPlayerInUsePoint && ItemManager.pickedItem != null)
        {
            EventTxt.text = "クリックでアイテムを使用";
            EventTxt.enabled = true;

            if (Input.GetMouseButtonDown(0))
            {
                Destroy(ItemManager.pickedItem);    //手に持っているアイテムを削除
                ItemManager.pickedItem = null;
                EventTxt.text = "";
                EventTxt.enabled = false;
                Debug.Log("アイテムを使用："+ItemManager.pickedItem);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //プレイヤーがアイテム使用可能地点と接しているとき
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInUsePoint = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //プレイヤーがアイテム使用可能地点から離れたとき
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInUsePoint = false;
            EventTxt.text = "";
            EventTxt.enabled = false;
        }
    }
}
