using UnityEngine;
using UnityEngine.EventSystems;

public class BookController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform;      // ドラックするテキストのRectTransform
    private Canvas canvas;                    // スケール補正用のCanvas
    public RectTransform keyZoneRect;         // テキストを配置したい位置のRectTransform

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    // UIをマウスに追従させる
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        //　テキストの位置(アンカーからの相対位置) += マウスの移動量 ÷ Canvasのスケーリング係数
    }

    //ドラッグ開始処理
    public void OnPointerDown(PointerEventData eventData) 
    {
        //特になし
    }

    // ドラッグ終了処理
    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsOverlappingKeyZone())
        {
            Debug.Log("カギを入手した");
            SnapToKeyZoneCenter();
        }
        else
        {
            Debug.Log("正しい位置ではありません");
        }
    }

    // テキストがKeyZoneに「一部でも」重なっているかどうかを判定
    private bool IsOverlappingKeyZone()
    {
        Rect rectA = GetWorldRect(rectTransform);   // テキストの矩形
        Rect rectB = GetWorldRect(keyZoneRect);     // キーゾーンの矩形

        // UnityのRect構造体の Overlaps() を使用して、交差を判定
        return rectA.Overlaps(rectB);
    }

    // RectTransformからワールド空間のRectを取得
    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];     //UI オブジェクトの4つの角の位置を格納

        rt.GetWorldCorners(corners);    // 左下→左上→右上→右下 の順に角を取得（ワールド座標）

        Vector3 bottomLeft = corners[0];   // 左下
        Vector3 topRight = corners[2];     // 右上

        // 左下とサイズでRectを作成
        return new Rect(bottomLeft, topRight - bottomLeft);
    }

    // テキストをKeyZoneの中心に移動（スナップ）
    private void SnapToKeyZoneCenter()
    {
        rectTransform.position = keyZoneRect.position;
    }
}
