using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class QuizManager : MonoBehaviour
{
    [Header("CSV設定")]
    [SerializeField] private string csvFileName = "quiz_data.csv";
    
    [Header("UI要素")]
    [SerializeField] private TextMeshProUGUI questionText;        // 問題文表示用Text
    [SerializeField] private Button[] choiceButtons;   // 選択肢ボタン（4つ）
    [SerializeField] private TextMeshProUGUI[] choiceTexts;       // 選択肢テキスト（4つ）
    [SerializeField] private GameObject resultPanel;   // 結果表示パネル
    [SerializeField] private TextMeshProUGUI resultText;          // 正誤表示Text
    [SerializeField] private TextMeshProUGUI explanationText;     // 解説表示Text
    [SerializeField] private Button nextButton;        // 次へボタン
    
    [Header("テスト設定")]
    [SerializeField] private bool testMode = false;           // テストモード
    [SerializeField] private TestModeType testModeType = TestModeType.Random;
    [SerializeField] private int testQuestionNumber = 1;      // テスト用問題番号
    [SerializeField] private string testTag = "";             // テスト用タグ
    
    private List<QuizData> allQuizData = new List<QuizData>();
    private QuizData currentQuiz;
    private bool isAnswered = false;
    private bool lastResult = false;
    
    public enum TestModeType
    {
        Random,
        SpecificNumber,
        Tag
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
        //今のとこ分岐処理ないので混乱を防ぐため同じことをする.
        else
        {
            StartTestMode();
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
                QuizData quiz = new QuizData(values);
                allQuizData.Add(quiz);
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
        
        // 結果パネルを非表示
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
    
    // テストモード開始
    void StartTestMode()
    {
        switch (testModeType)
        {
            case TestModeType.Random:
                ShowRandomQuiz();
                break;
            case TestModeType.SpecificNumber:
                ShowQuizByNumber(testQuestionNumber);
                break;
            case TestModeType.Tag:
                ShowQuizByTag(testTag);
                break;
        }
    }
    
    // 外部から呼び出されるメソッド：ランダム出題
    public void ShowRandomQuiz()
    {
        if (allQuizData.Count == 0)
        {
            Debug.LogError("クイズデータがありません");
            return;
        }
        
        int randomIndex = Random.Range(0, allQuizData.Count);
        ShowQuiz(allQuizData[randomIndex]);
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
            Debug.LogError($"問題番号 {questionNumber} が見つかりません");
        }
    }
    
    // 外部から呼び出されるメソッド：タグ指定出題
    public void ShowQuizByTag(string tag)
    {
        List<QuizData> taggedQuizzes = allQuizData.Where(q => q.tag == tag).ToList();
        if (taggedQuizzes.Count > 0)
        {
            int randomIndex = Random.Range(0, taggedQuizzes.Count);
            ShowQuiz(taggedQuizzes[randomIndex]);
        }
        else
        {
            Debug.LogError($"タグ '{tag}' の問題が見つかりません");
        }
    }
    
    // クイズ表示
    void ShowQuiz(QuizData quiz)
    {
        currentQuiz = quiz;
        isAnswered = false;
        
        // 問題文を表示
        if (questionText != null)
        {
            questionText.text = quiz.questionText;
        }
        
        // 選択肢を表示
        string[] choices = quiz.GetChoices();
        for (int i = 0; i < choiceButtons.Length && i < choices.Length; i++)
        {
            choiceButtons[i].interactable = true;
            if (choiceTexts[i] != null)
            {
                choiceTexts[i].text = choices[i];
            }
        }
        
        // 結果パネルを非表示
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
    
    // 選択肢クリック処理
    void OnChoiceClick(int choiceIndex)
    {
        if (isAnswered) return;
        
        isAnswered = true;
        
        // 正誤判定
        bool isCorrect = currentQuiz.IsCorrect(choiceIndex);
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
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        
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
    }
    
    // 次へボタンクリック処理
    void OnNextButtonClick()
    {
        if (testMode)
        {
            StartTestMode();
        }
        else
        {
            StartTestMode();
        }
                
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