using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BookController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform;      // ドラッグするImageのRectTransform
    private Canvas canvas;                    // スケール補正用のCanvas
    public RectTransform keyZoneRect;         // Imageを配置したい位置のRectTransform
    public static bool isGetKey = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (isGetKey)
        {
            SnapToKeyZoneCenter();
        }
    }

    // UI(Image)をマウスに追従させる
    public void OnDrag(PointerEventData eventData)
    {
        if (!isGetKey)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    // ドラッグ開始処理
    public void OnPointerDown(PointerEventData eventData)
    {
        // 特になし
    }

    // ドラッグ終了処理
    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsOverlappingKeyZone())
        {
            SnapToKeyZoneCenter();
            isGetKey = true;
        }
        else
        {
            Debug.Log("正しい位置ではありません");
        }
    }

    // ImageがKeyZoneに「一部でも」重なっているかどうかを判定
    private bool IsOverlappingKeyZone()
    {
        Rect rectA = GetWorldRect(rectTransform);   // Imageの矩形
        Rect rectB = GetWorldRect(keyZoneRect);     // キーゾーンの矩形

        return rectA.Overlaps(rectB);
    }

    // RectTransformからワールド空間のRectを取得
    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector3 bottomLeft = corners[0];
        Vector3 topRight = corners[2];

        return new Rect(bottomLeft, topRight - bottomLeft);
    }

    // ImageをKeyZoneの中心に移動（スナップ）
    private void SnapToKeyZoneCenter()
    {
        rectTransform.position = keyZoneRect.position;
    }
}
