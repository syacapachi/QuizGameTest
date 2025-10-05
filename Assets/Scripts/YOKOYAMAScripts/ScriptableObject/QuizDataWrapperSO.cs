using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//Editor�ŃN�C�Y�f�[�^����鎞�p
//�A�^�b�`���邽�߂ɂ́A1�t�@�C��,1Unity�֘A�N���X(�t�@�C�����ƈ�v)
//���̂�ƍ\���͓����ɂ��邱��
[CreateAssetMenu(menuName = "QuizDataWrapperSO")]
public class QuizDataWrapperSO : ScriptableObject
{
    public QuizDataSO[] quizDatasSO; 
    public List<QuizData> quizDatas = new List<QuizData>();

    public List<QuizData> LoadJson(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName+".json");
        string json = File.ReadAllText(filePath);
        if (json == null)
        {
            Debug.LogError("Json File is Not Exist");
        }
        // ���ʕ�����ǂݍ���
        QuizDataWrapper wrapper = JsonUtility.FromJson<QuizDataWrapper>(json);
        List<QuizData> result = new List<QuizData>();
        //List<QuizDataSO> resultSO = new List<QuizDataSO>();
        foreach (QuizData q in wrapper.quizDatas)
        {
            //�N���X�ʂ̔h��������I��
            if (q.quiztype == "text")
            {
                TextQuizData questiontext = JsonUtility.FromJson<TextQuizData>(JsonUtility.ToJson(q));
                result.Add(questiontext);
            }
            else if (q.quiztype == "image")
            {
                TextQuizData questionimage = JsonUtility.FromJson<TextQuizData>(JsonUtility.ToJson(q));
                result.Add(questionimage);
            }
        }
        Debug.Log($"Load from Json count = {result.Count}");
        quizDatas = result;
        return result;
    }
    public void ExportJson(string jsonName)
    {
        QuizDataWrapper wrapper = new QuizDataWrapper();
        wrapper.quizDatas = new List<QuizData>();
        for (int i = 0; i < quizDatas.Count; i++)
        {
            if (quizDatasSO[i] is TextQuizDataSO textSO)
            {

                wrapper.quizDatas[i] = new TextQuizData
                {
                    quiztype = "text",
                    questionNumber = textSO.questionNumber,
                    questionText = textSO.questionText,
                    choices = textSO.choices,
                    correctAnswer = textSO.correctAnswer,
                    explanation = textSO.explanation,
                    tag = textSO.tag
                };
            }
            else if (quizDatasSO[i] is ImageQuizDataSO imageSO)
            {
                wrapper.quizDatas[i] = new ImageQuizData
                {
                    quiztype = "image",
                    questionNumber = imageSO.questionNumber,
                    imageurl = imageSO.questionImage.name + ".png", // Sprite �� Addressable Key �ƍ��킹��
                    choices = imageSO.choices,
                    correctAnswer = imageSO.correctAnswer,
                    explanation = imageSO.explanation,
                    tag = imageSO.tag
                };
            }
        }
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(Application.streamingAssetsPath + "/" + jsonName + ".json", json);
        Debug.Log($"ExportedJson:\n{json}");
    }
    
}



[CustomEditor(typeof(QuizDataWrapperSO))]
public class QuizDadaSOInEditot : Editor
{
    //�N���X�̃����o�[���ᖳ���ƍX�V�ł��Ȃ�
    string loadJsonFileName = "loadJsonFileName";
    string exportJsonFileName = "exportJsonFileName";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //�ύX���㏑��������
        serializedObject.Update();
        QuizDataWrapperSO wrapper = target as QuizDataWrapperSO;
        
        loadJsonFileName = GUILayout.TextField(loadJsonFileName);
        if(GUILayout.Button("Load Json"))
        {
            wrapper.LoadJson(loadJsonFileName);
        }
        GUILayout.Space(1);
        exportJsonFileName = GUILayout.TextField(exportJsonFileName);
        if (GUILayout.Button("Export Json"))
        {
            wrapper.ExportJson(exportJsonFileName);
        }
        if(GUILayout.Button("Export Asset file"))
        {
            foreach (var q in wrapper.quizDatasSO)
            {
                q.CreateAsset();
            }
        }
        //�ύX��������݂����Ȃ��
        serializedObject.ApplyModifiedProperties();
    }
}