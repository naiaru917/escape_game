using System.Collections.Generic;
using UnityEngine;

public class CauldronManager : MonoBehaviour
{
    // �����̏��ԁi�C���X�y�N�^�[�Őݒ�j
    [SerializeField] private List<string> correctOrder = new List<string>();

    // �������ꂽ�A�C�e�����̗���
    private List<string> currentOrder = new List<string>();

    // ��Ɏc���Ă���A�C�e���̎Q��
    private List<GameObject> ingredientsInCauldron = new List<GameObject>();

    /// <summary>
    /// ��ɃA�C�e���𓊓������Ƃ��ɌĂ΂��
    /// </summary>
    public void AddIngredient(GameObject ingredient)
    {
        // �A�C�e�������L�^�iPrefab����I�u�W�F�N�g���𗘗p�j
        string ingredientName = ingredient.name.Replace("(Clone)", "");
        currentOrder.Add(ingredientName);

        // Destroy������Ɏc��
        ingredientsInCauldron.Add(ingredient);

        Debug.Log("��ɓ���: " + ingredientName);

        // ����`�F�b�N�i3��������j
        if (currentOrder.Count == correctOrder.Count)
        {
            CheckOrder();
        }
    }

    private void CheckOrder()
    {
        bool isCorrect = true;

        for (int i = 0; i < correctOrder.Count; i++)
        {
            if (currentOrder[i] != correctOrder[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            Debug.Log("�N���A�I");
            // TODO: �N���A���o��A�C�e���ϐg����
        }
        else
        {
            Debug.Log("���s�c���Z�b�g");
            ResetCauldron();
        }
    }

    private void ResetCauldron()
    {
        Debug.Log("���s�I���Z�b�g���܂�");
        foreach (var obj in ingredientsInCauldron)
        {
            if (obj != null) Destroy(obj);
        }
        ingredientsInCauldron.Clear();
        currentOrder.Clear();
    }
}
