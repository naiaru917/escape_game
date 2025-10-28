using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class DialLock : MonoBehaviour
{
    [SerializeField] private DialPanel[] panels; // 3�̃p�l�����C���X�y�N�^�ŃZ�b�g
    [SerializeField] private int[] correctCode = { 3, 7, 5 }; // �����̐���

    public TextMeshPro[] dialTexts;   // 3�̃_�C������TextMeshPro�Q��
    public static int[] dialValues = new int[3];  // �V�[�����܂����ŕێ�����l

    public static  bool isUnlocked = false; // �J������true�ɂȂ�
    public Animator openBox;
    private void Start()
    {
        // �������ɊJ���Ă�����A�����ڂ��J������Ԃɂ���
        if (isUnlocked)
        {
            SetOpenedState();
        }

        // �����̏�Ԃ𕜌�
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetValue(dialValues[i]); // DialPanel ��������
        }
    }

    public void CheckAnswer()
    {
        if (isUnlocked) return; // ���ɉ����ς݂Ȃ�X�L�b�v

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i].CurrentValue != correctCode[i])
                return; // 1�ł��s��v�Ȃ�I��
        }

        // �S����v�������������
        isUnlocked = true;
        OpenBox();
    }

    private void OpenBox()
    {
        Debug.Log("�����J�����I");
        openBox.SetTrigger("OpenBox");
    }

    private void SetOpenedState()
    {
        // �A�j���[�V�����̍Ō�̃t���[���ŕ\���i�W���󂢂Ă����ԁj
        openBox.Play("OpenBox", 0, 1f);
    }

    public bool IsUnlocked => isUnlocked;
}
