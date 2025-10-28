using System.Collections.Generic;
using UnityEngine;

public class ItemPuzzleManager : MonoBehaviour
{
    public static ItemPuzzleManager Instance { get; private set; }
    public static int currentStageIndex = 0;    // �p�Y�����N���A������

    [Header("�i�K�ʂ̐����p�^�[��")]
    public List<PuzzleStage> puzzleStages = new();

    // �ݒu�����A�C�e�����X�g
    private static List<Dictionary<string, string>> placedItemNamesPerStage = new();

    void Awake()
    {
        if (Instance == null) Instance = this;

        // static �f�[�^�����������Ȃ珉��������
        if (placedItemNamesPerStage.Count == 0)
        {
            foreach (var stage in puzzleStages)
            {
                placedItemNamesPerStage.Add(new Dictionary<string, string>());
            }
        }

        Debug.Log("���݂̃p�Y���X�e�[�W�F" + currentStageIndex);
    }

    // �A�C�e�����ݒu���ꂽ�Ƃ��ɌĂ΂��
    public void ReportPlacement(GameObject location, GameObject item)
    {
        string itemName = item.name.Replace("(Clone)", "").Trim();
        string locationName = location.name.Trim();

        if (currentStageIndex >= placedItemNamesPerStage.Count) return;

        var stagePlacedItems = placedItemNamesPerStage[currentStageIndex];
        stagePlacedItems[locationName] = itemName;


        CheckPuzzleCompletion();
    }

    // �������ƈ�v���邩�`�F�b�N
    private void CheckPuzzleCompletion()
    {
        if (currentStageIndex >= puzzleStages.Count) return;
        // �S���ݒu����Ă��Ȃ��Ȃ牽�����Ȃ�

        var stage = puzzleStages[currentStageIndex];
        var stagePlacedItems = placedItemNamesPerStage[currentStageIndex];

        if (stagePlacedItems.Count < stage.installationLocations.Count) return;

        for (int i = 0; i < stage.installationLocations.Count; i++)
        {
            string locationName = stage.installationLocations[i].name.Trim();
            string expectedName = stage.correctItemNames[i];

            if (!stagePlacedItems.TryGetValue(locationName, out string actualName) || actualName != expectedName)
            {
                return;
            }
        }

        // �S����v �� ����
        OnPuzzleStageClear();
    }

    private void OnPuzzleStageClear()
    {
        Debug.Log($"�p�Y�� {currentStageIndex} ���N���A");

        currentStageIndex++;

        if (currentStageIndex >= puzzleStages.Count)
        {
            Debug.Log("���ׂẴp�Y�����N���A���܂����I");
            // �ŏI�N���A�����i��F�h�A���J����A�A�C�e�����o�������铙�j
        }
        else
        {
            // ���̃X�e�[�W�̉��o�E�A�N�e�B�u�����Ȃǂ�����΂�����
            Debug.Log($"���̃p�Y�� {currentStageIndex} �ɐi�݂܂�");
        }
    }

    // ����������Z�b�g�������ꍇ�i�C�Ӂj
    public void ResetPuzzle()
    {
        currentStageIndex = 0;

        // �e�X�e�[�W�̐ݒu����������
        for (int i = 0; i < placedItemNamesPerStage.Count; i++)
        {
            placedItemNamesPerStage[i].Clear();
        }

        Debug.Log("�p�Y���S�̂����Z�b�g���܂���");
    }
}