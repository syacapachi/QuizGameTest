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
    [Header("対象クイズデータベース")]
    [SerializeField] public QuizDataWrapperSO defaultDatabase; // 内部デフォルト
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
        UnityEngine.Debug.Log($"MargeQuizzes is {external.Count}");
    }
    //新たなクイズデータベースを作成
    private void ExportCSV(/*List<QuizData> external*/)
    {
        List<QuizData> external = defaultDatabase.quizDatas;
        //CSV書き出し
        // 新しくcsvファイルを作成して、{}の中の要素分csvに追記をする
        StreamWriter sw = new StreamWriter(Application.streamingAssetsPath +"/"+"AddData.csv", false, Encoding.GetEncoding("UTF-8"));
        sw.NewLine = "\n"; //   改行をLFに統一(余分な改行を防ぐ)
        string[] s1 = { "問題番号", "問題文", "選択肢A", "選択肢B", "選択肢C", "選択肢D", "解答(A～D)", "解説", "タグ", "備考" };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
        
        foreach (QuizData q in external)
        {
            string[] quiz = { q.questionNumber.ToString(), q.questionText,string.Join(",",q.choices),q.correctAnswer.ToString(),q.explanation,q.tag };
            string csvdata = string.Join(",", quiz);
            sw.WriteLine(csvdata);
        }
        //この方法は手動で閉じる
        sw.Close();
        UnityEngine.Debug.Log("Export CSV file");
        //新しい情報を持ったアセットの分割(アセットにしなくても良い).
        var asset = ScriptableObject.CreateInstance<QuizDataWrapperSO>();
        foreach (var q in external)
        {   
            // 新規追加
            asset.quizDatas.Add(q);
            
        }
        UnityEngine.Debug.Log("Export Quiz Asset");
        //アセットの作成.
        AssetDatabase.CreateAsset(asset, $"Assets/Quizdata/AddData.asset");
        //アセットの即時保存(CreateAssetでも保存されるが、キャッシュがある場合されない)
        AssetDatabase.SaveAssets();
        //ProjectWindowを再読み込み,変更を適応
        AssetDatabase.Refresh();
        //ProjectWindowを表示
        EditorUtility.FocusProjectWindow();
        //ProjectWindowのインスペクターに表示するモノをこれにする.
        Selection.activeObject = asset;
        UnityEngine.Debug.Log($"Quizzes is Added of  {external.Count}");
    
}
}

//エディターを変更宣言.
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
