using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//Editorでクイズデータを作る時用
//アタッチするためには、1ファイル,1Unity関連クラス(ファイル名と一致)
//元のやつと構成は同じにすること
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
        // 共通部分を読み込み
        QuizDataWrapper wrapper = JsonUtility.FromJson<QuizDataWrapper>(json);
        List<QuizData> result = new List<QuizData>();
        //List<QuizDataSO> resultSO = new List<QuizDataSO>();
        foreach (QuizData q in wrapper.quizDatas)
        {
            //クラス別の派生部分を選択
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
                    imageurl = imageSO.questionImage.name + ".png", // Sprite の Addressable Key と合わせる
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
    //クラスのメンバーじゃ無いと更新できない
    string loadJsonFileName = "loadJsonFileName";
    string exportJsonFileName = "exportJsonFileName";
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //変更を上書きするやつ
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
        //変更を許可するみたいなやつ
        serializedObject.ApplyModifiedProperties();
    }
}