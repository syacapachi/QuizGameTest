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
    [Header("初期化用クイズデータベース")]
    [SerializeField] public QuizDataBase defaultDatabase; // 内部デフォルト
    private List<QuizData> loadedQuizzes = new List<QuizData>();
    [Header("更新用設定")]
    [SerializeField] bool isOverWrite = false;
    [SerializeField] string csvFilePath = "quiz_data.csv";
   
    public List<QuizData> QuizDataList => defaultDatabase.quizDatas;

    // 外部CSVロード
    private void UpdateCSV() 
    {
        //上書きしない場合デフォをロード.
        if (!isOverWrite)
        {
            // 内部デフォルトロード
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
        // WebGLはUnityWebRequestを使う
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("CSVロード失敗: " + www.error);
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
        string[] lines = text.Split('\n');//行で分割.
        //Debug.Log($"Lines={lines[0]}");
        for (int i = 1; i < lines.Length; i++)
        {
            // , "" 空白 で分割
            string[] cols = SplitCSVLine(lines[i]);
            if (cols.Length < 9) continue;
            //コンストラクタのオーバーロード.
            QuizData q = new QuizData
            {
                questionNumber = int.Parse(cols[0]),
                questionText = cols[1],
                choices = cols[2..^3],//Pythonでいう cols[2:-3]
                correctAnswer = Int32.Parse(cols[cols.Length -3]),
                explanation = cols[cols.Length -2],
                tag = cols[cols.Length -1]
            };
            list.Add(q);
        }
        return list;
    }
    // CSV行を分割（カンマ区切り、ダブルクォート対応）
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

    //データベースに登録、上書き.
    private void MergeQuizzes(List<QuizData> external)
    {
        foreach (var q in external)
        {
            var existing = loadedQuizzes.Find(x => x.questionNumber == q.questionNumber);
            if (existing != null)
            {
                // 上書き
                existing.questionText = q.questionText;
                existing.choices = q.choices;
                existing.correctAnswer = q.correctAnswer;
                existing.explanation = q.explanation;
            }
            else
            {
                // 新規追加
                loadedQuizzes.Add(q);
            }
        }
        //以下の方法は、ビルド後には使えない手法,初期設定には使える.
        //データベース更新
        defaultDatabase.quizDatas = loadedQuizzes;
        Debug.Log($"MargeQuizzes is {external.Count}");
    }
    private void AddQuizes(List<QuizData> external)
    {
        //新しい情報を持ったアセットの分割(アセットにしなくても良い).
        var asset = ScriptableObject.CreateInstance<QuizDataBase>();
        foreach (var q in external)
        {   
            // 新規追加
            asset.quizDatas.Add(q);
            
        }

        //アセットの作成.
        AssetDatabase.CreateAsset(asset, $"Assets/Scripts/YOKOYAMAScripts/AddData.asset");
        //アセットの即時保存(CreateAssetでも保存されるが、キャッシュがある場合されない)
        AssetDatabase.SaveAssets();
        //ProjectWindowを再読み込み,変更を適応
        AssetDatabase.Refresh();
        //ProjectWindowを表示
        EditorUtility.FocusProjectWindow();
        //ProjectWindowのインスペクターに表示するモノをこれにする.
        Selection.activeObject = asset;
        Debug.Log($"Quizzes is Added of  {external.Count}");
    
}
}

//エディターを変更宣言.
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