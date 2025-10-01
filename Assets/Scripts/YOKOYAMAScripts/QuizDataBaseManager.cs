using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Rendering.LookDev;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.VersionControl;
using System;

[ExecuteAlways]
public class QuizDataBaseManager : MonoBehaviour
{
    [Header("�������p�N�C�Y�f�[�^�x�[�X")]
    [SerializeField] public QuizDataBase defaultDatabase; // �����f�t�H���g
    private List<QuizData> loadedQuizzes = new List<QuizData>();
    [Header("�X�V�p�ݒ�")]
    [SerializeField] bool isOverWrite = false;
    [SerializeField] string csvFilePath = "quiz_data.csv";
   
    public List<QuizData> QuizDataList => defaultDatabase.quizDatas;

    // �O��CSV���[�h
    private void UpdateCSV() 
    {
        //�㏑�����Ȃ��ꍇ�f�t�H�����[�h.
        if (!isOverWrite)
        {
            // �����f�t�H���g���[�h
            loadedQuizzes.AddRange(defaultDatabase.quizDatas);
        }
        string path = Path.Combine(Application.streamingAssetsPath, csvFilePath);
        if (File.Exists(path))
        {
            StartCoroutine(LoadCSV(path));
            Debug.Log("CSV file Load is Success");
        }
        else
        {
            Debug.LogError("CSV file is no Exsit");
        }
        //foreach(var quiz in loadedQuizzes)
        //{
        //    Debug.Log($"{quiz.questionNumber},{quiz.questionText},{quiz.choices[0]},{quiz.correctAnswer},{quiz.explanation},{quiz.tag}");
        //}
    }
    private System.Collections.IEnumerator LoadCSV(string path)
    {
#if UNITY_WEBGL
        // WebGL��UnityWebRequest���g��
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("CSV���[�h���s: " + www.error);
            }
            else
            {
                List<QuizData> external = ParseCSV(www.downloadHandler.text);
                MergeQuizzes(external);
            }
        }
#else
        string quizText = File.ReadAllText(path);
        //Debug.Log($"Text={quizText}");
        List<QuizData> external =  ParseCSV(quizText);
        MergeQuizzes(external);
        yield return null;
#endif
        
    }
    private List<QuizData> ParseCSV(string text)
    {
        List<QuizData> list = new List<QuizData>();
        string[] lines = text.Split('\n');//�s�ŕ���.
        //Debug.Log($"Lines={lines[0]}");
        for (int i = 1; i < lines.Length; i++)
        {
            // , "" �� �ŕ���
            string[] cols = SplitCSVLine(lines[i]);
            if (cols.Length < 9) continue;
            //�R���X�g���N�^�̃I�[�o�[���[�h.
            QuizData q = new QuizData
            {
                questionNumber = int.Parse(cols[0]),
                questionText = cols[1],
                choices = cols[2..^3],//Python�ł��� cols[2:-3]
                correctAnswer = Int32.Parse(cols[cols.Length -3]),
                explanation = cols[cols.Length -2],
                tag = cols[cols.Length -1]
            };
            list.Add(q);
        }
        return list;
    }
    // CSV�s�𕪊��i�J���}��؂�A�_�u���N�H�[�g�Ή��j
    private string[] SplitCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        result.Add(currentField);
        return result.ToArray();
    }

    //�f�[�^�x�[�X�ɓo�^�A�㏑��.
    private void MergeQuizzes(List<QuizData> external)
    {
        foreach (var q in external)
        {
            var existing = loadedQuizzes.Find(x => x.questionNumber == q.questionNumber);
            if (existing != null)
            {
                // �㏑��
                existing.questionText = q.questionText;
                existing.choices = q.choices;
                existing.correctAnswer = q.correctAnswer;
                existing.explanation = q.explanation;
            }
            else
            {
                // �V�K�ǉ�
                loadedQuizzes.Add(q);
            }
        }
        //�ȉ��̕��@�́A�r���h��ɂ͎g���Ȃ���@,�����ݒ�ɂ͎g����.
        //�f�[�^�x�[�X�X�V
        defaultDatabase.quizDatas = loadedQuizzes;
        Debug.Log($"MargeQuizzes is {external.Count}");
    }
    private void AddQuizes(List<QuizData> external)
    {
        //�V���������������A�Z�b�g�̕���(�A�Z�b�g�ɂ��Ȃ��Ă��ǂ�).
        var asset = ScriptableObject.CreateInstance<QuizDataBase>();
        foreach (var q in external)
        {   
            // �V�K�ǉ�
            asset.quizDatas.Add(q);
            
        }

        //�A�Z�b�g�̍쐬.
        AssetDatabase.CreateAsset(asset, $"Assets/Scripts/YOKOYAMAScripts/AddData.asset");
        //�A�Z�b�g�̑����ۑ�(CreateAsset�ł��ۑ�����邪�A�L���b�V��������ꍇ����Ȃ�)
        AssetDatabase.SaveAssets();
        //ProjectWindow���ēǂݍ���,�ύX��K��
        AssetDatabase.Refresh();
        //ProjectWindow��\��
        EditorUtility.FocusProjectWindow();
        //ProjectWindow�̃C���X�y�N�^�[�ɕ\�����郂�m������ɂ���.
        Selection.activeObject = asset;
        Debug.Log($"Quizzes is Added of  {external.Count}");
    
}
}

//�G�f�B�^�[��ύX�錾.
[CustomEditor(typeof(QuizDataBaseManager))]
public class QuizDataBaseInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Updata CSV file"))
        {
            QuizDataBaseManager _manager = target as QuizDataBaseManager;
            _manager.SendMessage("UpdateCSV", null, SendMessageOptions.DontRequireReceiver);
        }
    }
}