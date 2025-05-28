using UnityEngine;

public class ObjectStateManager : MonoBehaviour
{
    public ObjectState objectState;
    public string objectName;  // �I�u�W�F�N�g�̎��ʖ�

    private void Start()
    {
        // �Q�[���J�n���Ƀf�[�^�����Z�b�g�i����̂݁j
        if (GameManager.isFirstRunCup)
        {
            objectState.ResetAllData();
            GameManager.isFirstRunCup = false;
        }

        // �����ʒu���ۑ�����Ă���Ε���
        if (objectState.HasInitialPosition(objectName))
        {
            RestorePosition();
        }
    }

    private void Update()
    {
        if(GameManager.isSceneMove==true)
        {
            // �{�̐��E�ɂ���ԁAV�L�[�ňʒu��ۑ�
            if (GameManager.isInBookWorld && Input.GetKeyDown(KeyCode.V))
            {
                SaveCurrentPosition();
            }
        }
    }

    private void SaveCurrentPosition()
    {
        objectState.SaveState(objectName, transform.position, transform.rotation);
        //Debug.Log($"[{objectName}] �̈ʒu��ۑ�: {transform.position}");
    }

    private void RestorePosition()
    {
        if (objectState.TryGetState(objectName, out Vector3 savedPosition, out Quaternion savedRotation))
        {
            transform.position = savedPosition;
            transform.rotation = savedRotation;
            //Debug.Log($"[{objectName}] �̈ʒu�𕜌�: {transform.position}");
        }
        else
        {
            //Debug.LogWarning($"[{objectName}] �̕ۑ����ꂽ�ʒu��������܂���");
        }
    }
}
