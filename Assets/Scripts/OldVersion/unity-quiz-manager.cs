using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEditor;
using System;
using Unity.VisualScripting;
using System.Text;
using System.Globalization;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting.FullSerializer;

public class QuizManager : MonoBehaviour
{
    [Header("CSV設定")]
    [SerializeField] private string csvFileName = "quiz_data.csv";
    [Header("SetupUI")]
    [SerializeField] GameObject setupPanel;
    [SerializeField] TextMeshProUGUI tagField;
    [SerializeField] TextMeshProUGUI IdField;
    [SerializeField] TextMeshProUGUI errorMessage;
    [Header("クイズUI要素")]
    [SerializeField] private TextMeshProUGUI questionText;        // 問題文表示用Text
    [SerializeField] private TextMeshProUGUI progressText;      // 1/10 みたいなやつ
    [SerializeField] private UnityEngine.UI.Button[] choiceButtons;   // 選択肢ボタン（4つ）
    [SerializeField] private TextMeshProUGUI[] choiceTexts;       // 選択肢テキスト（4つ）
    [Header("Result画面UI")]
    [SerializeField] private GameObject resultPanel;   // 結果表示パネル
    [SerializeField] private TextMeshProUGUI resultText;          // 正誤表示Text
    [SerializeField] private TextMeshProUGUI explanationText;     // 解説表示Text
    [SerializeField] private TextMeshProUGUI nextText; 
    [SerializeField] private UnityEngine.UI.Button nextButton;        // 次へボタン
    [Header("EnddingUI")]
    [SerializeField] GameObject enddingPanel;
    [Header("テスト設定")]
    [SerializeField] private bool testMode = false;           // テストモード
    [SerializeField] private GameModeType gameModeType = GameModeType.Random;
    [SerializeField] private int testQuestionNumber = 1;      // テスト用問題番号
    [SerializeField] private string testTag = "";             // テスト用タグ
    
    private List<QuizData> allQuizData = new List<QuizData>();
    private List<QuizData> sortQuizData = new List<QuizData>();
    private int quizindex = 0;
    private QuizData currentQuiz;

    private bool isAnswered = false;
    private bool lastResult = false;
    
    public enum GameModeType
    {
        Setup,
        EditorRandom,
        Random,
        SpecificNumber,
        Tag,
        Endding
    }
    void Start()
    {
        LoadCSVData();
        SetupUI();
        
        // テストモードの場合、自動的に問題を開始
        if (testMode)
        {
            StartTestMode();
        }
        else
        {
            ShowSetup();
        }
    }
    
    // CSVデータの読み込み
    void LoadCSVData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        
        if (File.Exists(filePath))
        {
            string dataString = File.ReadAllText(filePath);
            string[] lines = dataString.Split('\n');
            
            // ヘッダー行をスキップして、各行を処理
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) continue;
                
                string[] values = SplitCSVLine(lines[i]);
                if (values.Length >= 9)
                {
                    QuizData quiz = new TextQuizData()
                    {
                        questionNumber = Int32.Parse(values[0]),
                        questionText = values[1],
                        choices = values[2..^3],
                        correctAnswer = Int32.Parse(values[^3]),
                       explanation = values[^2],
                        tag = values[^1]
                        
                    };
                    allQuizData.Add(quiz);
                }
            }
            
            Debug.Log($"読み込み完了: {allQuizData.Count}問の問題");
        }
        else
        {
            Debug.LogError($"CSVファイルが見つかりません: {filePath}");
        }
    }
    
    // CSV行を分割（カンマ区切り、ダブルクォート対応）
    string[] SplitCSVLine(string line)
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
    
    // UI初期設定
    void SetupUI()
    {
        // 選択肢ボタンにイベントを設定
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() => OnChoiceClick(index));
        }
        
        // 次へボタンのイベント設定
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClick);
        }
        
    }

    
    // テストモード開始
    void StartTestMode()
    {
        switch (gameModeType)
        {
            case GameModeType.Setup:
                ShowSetup();
                break;
            case GameModeType.EditorRandom:
                ShowEditorRandomQuiz();
                break;
            case GameModeType.Random:
                ShowRandomQuiz();
                break;
            case GameModeType.SpecificNumber:
                ShowQuizByNumber(testQuestionNumber);
                break;
            case GameModeType.Tag:
                ShowQuizByTag(testTag);
                break;
            case GameModeType.Endding:
                break;
        }
    }
    public void SelectGameMode(GameModeType _type)
    {
        gameModeType = _type;
    }
    
    public void ShowSetup()
    {
        // 問題パネル以外を非表示
        setupPanel.SetActive(false);
        resultPanel.SetActive(false);
        enddingPanel.SetActive(false);
        errorMessage.text = "";
        nextText.text = "Next";
        setupPanel.SetActive(true);   
    }

    // 外部から呼び出されるメソッド：ランダム出題
    public void ShowRandomQuiz()
    {
        if (allQuizData.Count == 0)
        {
            Debug.LogError("クイズデータがありません");
            return;
        }
        
        if(sortQuizData.Count == 0)
        {
            sortQuizData = allQuizData.OrderBy(i => Guid.NewGuid()).ToList();
        }
        ShowQuiz(sortQuizData[quizindex]);
    }
    public void ShowEditorRandomQuiz()
    {
        if (allQuizData.Count == 0)
        {
            Debug.LogError("クイズデータがありません");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, allQuizData.Count);
        ShowQuiz(allQuizData[randomIndex]);
    }
    public void ReadID()
    {
        int id = TMPTextUtils.ParseInt(IdField.text,-1);
        Debug.Log($"input={id}");
        if (id == -1)
        {
            errorMessage.text = $"入力に問題があります";
            UnityEngine.Debug.LogWarning($"Input Format Exception");
            return;
        }
        gameModeType = GameModeType.SpecificNumber;
        ShowQuizByNumber(id);
    }


    // 外部から呼び出されるメソッド：番号指定出題
    public void ShowQuizByNumber(int questionNumber)
    {
        QuizData quiz = allQuizData.FirstOrDefault(q => q.questionNumber == questionNumber);
        if (quiz != null)
        {
            ShowQuiz(quiz);
        }
        else
        {
            errorMessage.text = $"問題番号 {questionNumber} が見つかりません";
            Debug.LogWarning($"問題番号 {questionNumber} が見つかりません");
            return;
        }
    }
    public void ReadTag()
    {

        string tag = TMPTextUtils.NormalizeText(tagField.text);
        Debug.Log($"Input = {tag}");
        foreach (char c in tag)
        {
            Debug.Log($"'{c}' -> U+{(int)c:X4}");
        }
        if (string.IsNullOrEmpty(tag))
        {
            errorMessage.text = $"入力に問題があります入力={tag}";
            UnityEngine.Debug.LogWarning($"Input Format Exception,入力={tag}");
            return;
        }
        gameModeType = GameModeType.Tag;
        ShowQuizByTag(tag);
    }

    // 外部から呼び出されるメソッド：タグ指定出題
    public void ShowQuizByTag(string tag)
    {
        if(sortQuizData.Count == 0)
        {
            List<QuizData> taggedQuizzes = allQuizData.Where(q => q.tag == tag).ToList();
            if (taggedQuizzes.Count > 0)
            {
                sortQuizData = allQuizData.OrderBy(q => Guid.NewGuid()).ToList();
            }
            else
            {
                errorMessage.text = $"タグ '{tag}' の問題が見つかりません";
                Debug.LogWarning($"タグ '{tag}' の問題が見つかりません");
                return;
            }
        }
        ShowQuiz(sortQuizData[quizindex]);
        
        
    }
    
    // クイズ表示
    void ShowQuiz(QuizData quiz)
    {
        quizindex++;
        currentQuiz = quiz;
        isAnswered = false;
        
        if(progressText != null)
        {
            progressText.text = quizindex.ToString()+"/"+sortQuizData.Count;
        }

        // 問題文を表示
        if (questionText != null)
        {
            questionText.text = quiz.questionText;
        }
        
        // 選択肢を表示
        string[] choices = quiz.choices;
        for (int i = 0; i < choiceButtons.Length && i < choices.Length; i++)
        {
            choiceButtons[i].interactable = true;
            if (choiceTexts[i] != null)
            {
                choiceTexts[i].text = choices[i];
            }
        }
        
        resultPanel.SetActive(false);
        setupPanel.SetActive(false);
        enddingPanel.SetActive(false);
    }
    
    // 選択肢クリック処理
    void OnChoiceClick(int choiceIndex)
    {
        if (isAnswered) return;
        
        isAnswered = true;
        
        // 正誤判定
        bool isCorrect = currentQuiz.correctAnswer.Equals(choiceIndex);
        lastResult = isCorrect;
        
        // ボタンを無効化
        foreach (var button in choiceButtons)
        {
            button.interactable = false;
        }
        
        // 結果表示
        ShowResult(isCorrect, choiceIndex);
    }
    
    private string ToAlphabet(int index)
    {
        return ((char)('A' + index)).ToString();
    }
    // 結果表示
    void ShowResult(bool isCorrect, int selectedAnswer)
    {
        
        if (resultText != null)
        {
            if (isCorrect)
            {
                resultText.text = "正解！";
                resultText.color = Color.green;
            }
            else
            {
                resultText.text = $"不正解...\n正解は「{ToAlphabet(currentQuiz.correctAnswer)}」です";
                resultText.color = Color.red;
            }
        }
        
        if (explanationText != null)
        {
            explanationText.text = $"解説:\n{currentQuiz.explanation}";
        }
        
        if (sortQuizData.Count == quizindex)
        {
            nextText.text = "Result";
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
    }
    
    // 次へボタンクリック処理
    void OnNextButtonClick()
    {
        if(sortQuizData.Count == quizindex)
        {
            gameModeType = GameModeType.Endding;
            enddingPanel.SetActive(true);
            return;
        }
        StartTestMode();         
    }
    
    // 正誤結果を取得（外部から呼び出し可能）
    public bool GetLastResult()
    {
        return lastResult;
    }
    
    // 現在の問題データを取得
    public QuizData GetCurrentQuiz()
    {
        return currentQuiz;
    }
}

[CustomEditor(typeof(QuizManager))]
public class QuizStartButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        QuizManager _manager = target as QuizManager;

        GUILayout.Label("Button");
        GUILayout.BeginHorizontal();
        GUILayout.SelectionGrid(12, new string[] { "A", "B", "C" },3);
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("A"))
        {

        }
    }
}