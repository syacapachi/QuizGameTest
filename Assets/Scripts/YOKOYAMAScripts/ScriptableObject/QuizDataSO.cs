using UnityEditor;
using UnityEngine;

[System.Serializable]
public class QuizDataSO : ScriptableObject
{
    public string quiztype;         //�N�C�Y�̃^�C�v
    public int questionNumber;      // ���ԍ�
    public string questionText;     // ��蕶
    public string[] choices;        // �g����������������悤�ɔz��
    public int correctAnswer;    // �𓚁iA,B,C,D�j
    public string explanation;      // ���
    public string tag;             // �^�O

    public void CreateAsset()
    {
        QuizDataSO asset = ScriptableObject.CreateInstance<QuizDataSO>();
        asset = this;
        UnityEngine.Debug.Log("Export Quiz Asset");
        //�A�Z�b�g�̍쐬.
        AssetDatabase.CreateAsset(asset, $"Assets/Quizdata/AddData{questionNumber}.asset");
        //�A�Z�b�g�̑����ۑ�(CreateAsset�ł��ۑ�����邪�A�L���b�V��������ꍇ����Ȃ�)
        AssetDatabase.SaveAssets();
    }    
}
