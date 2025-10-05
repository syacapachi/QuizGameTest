using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System;
using System.Text;
using System.Diagnostics;

[ExecuteAlways]
public class QuizLoadFromCSV : MonoBehaviour
{
    [Header("�ΏۃN�C�Y�f�[�^�x�[�X")]
    [SerializeField] public QuizDataWrapperSO defaultDatabase; // �����f�t�H���g
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
            UnityEngine.Debug.Log("CSV file Load is Success");
        }
        else
        {
            UnityEngine.Debug.LogError("CSV file is no Exsit");
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
        UnityEngine.Debug.Log($"MargeQuizzes is {external.Count}");
    }
    //�V���ȃN�C�Y�f�[�^�x�[�X���쐬
    private void ExportCSV(/*List<QuizData> external*/)
    {
        List<QuizData> external = defaultDatabase.quizDatas;
        //CSV�����o��
        // �V����csv�t�@�C�����쐬���āA{}�̒��̗v�f��csv�ɒǋL������
        StreamWriter sw = new StreamWriter(Application.streamingAssetsPath +"/"+"AddData.csv", false, Encoding.GetEncoding("UTF-8"));
        sw.NewLine = "\n"; //   ���s��LF�ɓ���(�]���ȉ��s��h��)
        string[] s1 = { "���ԍ�", "��蕶", "�I����A", "�I����B", "�I����C", "�I����D", "��(A�`D)", "���", "�^�O", "���l" };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
        
        foreach (QuizData q in external)
        {
            string[] quiz = { q.questionNumber.ToString(), q.questionText,string.Join(",",q.choices),q.correctAnswer.ToString(),q.explanation,q.tag };
            string csvdata = string.Join(",", quiz);
            sw.WriteLine(csvdata);
        }
        //���̕��@�͎蓮�ŕ���
        sw.Close();
        UnityEngine.Debug.Log("Export CSV file");
        //�V���������������A�Z�b�g�̕���(�A�Z�b�g�ɂ��Ȃ��Ă��ǂ�).
        var asset = ScriptableObject.CreateInstance<QuizDataWrapperSO>();
        foreach (var q in external)
        {   
            // �V�K�ǉ�
            asset.quizDatas.Add(q);
            
        }
        UnityEngine.Debug.Log("Export Quiz Asset");
        //�A�Z�b�g�̍쐬.
        AssetDatabase.CreateAsset(asset, $"Assets/Quizdata/AddData.asset");
        //�A�Z�b�g�̑����ۑ�(CreateAsset�ł��ۑ�����邪�A�L���b�V��������ꍇ����Ȃ�)
        AssetDatabase.SaveAssets();
        //ProjectWindow���ēǂݍ���,�ύX��K��
        AssetDatabase.Refresh();
        //ProjectWindow��\��
        EditorUtility.FocusProjectWindow();
        //ProjectWindow�̃C���X�y�N�^�[�ɕ\�����郂�m������ɂ���.
        Selection.activeObject = asset;
        UnityEngine.Debug.Log($"Quizzes is Added of  {external.Count}");
    
}
}

//�G�f�B�^�[��ύX�錾.
[CustomEditor(typeof(QuizLoadFromCSV))]
public class QuizDataBaseInspector : Editor
{
    [SerializeField] QuizDataWrapperSO quizList;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        QuizLoadFromCSV _manager = target as QuizLoadFromCSV;

        if (GUILayout.Button("Load From CSV file"))
        {
            
            _manager.SendMessage("UpdateCSV", null, SendMessageOptions.DontRequireReceiver);
            UnityEngine.Debug.Log("Call Update CSV from Editor");
        }
        if (GUILayout.Button("Export CSV file"))
        {
            _manager.SendMessage("ExportCSV", null,SendMessageOptions.DontRequireReceiver);
            UnityEngine.Debug.Log("Call Export CSV from Editor");
        }
    }
}
