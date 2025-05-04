using UnityEngine;
using UnityEngine.EventSystems;

public class BookController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform;      // �h���b�N����e�L�X�g��RectTransform
    private Canvas canvas;                    // �X�P�[���␳�p��Canvas
    public RectTransform keyZoneRect;         // �e�L�X�g��z�u�������ʒu��RectTransform

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    // UI���}�E�X�ɒǏ]������
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        //�@�e�L�X�g�̈ʒu(�A���J�[����̑��Έʒu) += �}�E�X�̈ړ��� �� Canvas�̃X�P�[�����O�W��
    }

    //�h���b�O�J�n����
    public void OnPointerDown(PointerEventData eventData) 
    {
        //���ɂȂ�
    }

    // �h���b�O�I������
    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsOverlappingKeyZone())
        {
            Debug.Log("�J�M����肵��");
            SnapToKeyZoneCenter();
        }
        else
        {
            Debug.Log("�������ʒu�ł͂���܂���");
        }
    }

    // �e�L�X�g��KeyZone�Ɂu�ꕔ�ł��v�d�Ȃ��Ă��邩�ǂ����𔻒�
    private bool IsOverlappingKeyZone()
    {
        Rect rectA = GetWorldRect(rectTransform);   // �e�L�X�g�̋�`
        Rect rectB = GetWorldRect(keyZoneRect);     // �L�[�]�[���̋�`

        // Unity��Rect�\���̂� Overlaps() ���g�p���āA�����𔻒�
        return rectA.Overlaps(rectB);
    }

    // RectTransform���烏�[���h��Ԃ�Rect���擾
    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];     //UI �I�u�W�F�N�g��4�̊p�̈ʒu���i�[

        rt.GetWorldCorners(corners);    // ���������と�E�と�E�� �̏��Ɋp���擾�i���[���h���W�j

        Vector3 bottomLeft = corners[0];   // ����
        Vector3 topRight = corners[2];     // �E��

        // �����ƃT�C�Y��Rect���쐬
        return new Rect(bottomLeft, topRight - bottomLeft);
    }

    // �e�L�X�g��KeyZone�̒��S�Ɉړ��i�X�i�b�v�j
    private void SnapToKeyZoneCenter()
    {
        rectTransform.position = keyZoneRect.position;
    }
}
