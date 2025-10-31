using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum GameModeType {Setup,Play,End}
public enum QuizType { Random = 0,Order = 1}
public class GameManager : MonoBehaviour
{
    [Header("問題json")]
    [SerializeField] string jsonName = "quiz_data";
    [Header("セットアップUI")]
    [SerializeField] GameObject setupPanel;
    [SerializeField] GameObject quizTitleButton;
    [SerializeField] Transform prefabParent;
    [SerializeField] TextMeshProUGUI errorMessage;
    private string[] QuizTitiles;
    [Header("出題画面")]
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] TextMeshProUGUI imageQuizText;
    [SerializeField] Image QuizImage;
    [SerializeField] GridLayoutGroup QuizGrid;
    [Header("回答ボタンプレハブ")]
    [SerializeField] GameObject ButtonParent;
    [SerializeField] GameObject ButtonPrefab;
    [SerializeField] TextMeshProUGUI progressText;
    [Header("リザルト画面")]
    [SerializeField] GameObject resultPanel;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] TextMeshProUGUI explainText;
    [SerializeField] TextMeshProUGUI nextText;
    [SerializeField] Button nextButton;
    [Header("EnddingUI")]
    [SerializeField] GameObject enddingPanel;
    [SerializeField] TextMeshProUGUI collectCountText;
    [SerializeField] Button backButton;
    [Header("出題設定")]
    [SerializeField] GameModeType gameModeType = GameModeType.Setup;
    [SerializeField] QuizType quizType = QuizType.Order;
    [SerializeField] int quizID = 0;
    [SerializeField] string quiztag = "";

    private bool isQuizTitileSet = false;
    private List<QuizData> quizData = new List<QuizData>();
    private QuizData[] quizArray;
    private List<QuizData> sortQuizData = new List<QuizData>();
    private int quizindex = 0;
    private int collectCount;
    private QuizData currentQuiz;
    private List<GameObject> prefabsList = new List<GameObject>();
    private QuizDataWrapper wrapper = new QuizDataWrapper();
    private Vector2 defaultGridSize = new Vector2(800,600);
    private Vector2 expandGridSize = new Vector2(600,500);


    
    public void TitleSetUp(string json)
    {
        StartCoroutine(ManagerLocater.Instance.loader.LoadJsonCoroutine(json, result =>
        {
            if (result != null)
            {
                Debug.Log($"Loaded {result.quizDatas.Length} quizzes");
                quizData = result.quizDatas.ToList();
                quizArray = result.quizDatas;
                quizType = result.quizType;

                //終わってから呼ばれる安心安全設計
                gameModeType = GameModeType.Play;
                StartQuiz();
            }

            else
                Debug.LogWarning("Failed to load quiz data");
                errorMessage.text = json.ToString()+"のクイズデータがありません";
        }));
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeSetUpUI();
        ShowTitle();
    }
    private void ShowTitle()
    {
        quizindex = 0;
        sortQuizData.Clear();
        gameModeType = GameModeType.Setup;
        QuizTitiles = GetQuizTitile();
        ShowTitleMenu();
    }
    [OnInspectorButton]
    public void StartQuiz()
    {
        switch (gameModeType)
        {
            case GameModeType.Setup:
                ShowTitleMenu();
                break;
            case GameModeType.Play:
                SwitchQuizType();
                break;
            case GameModeType.End:
                break;
        }
    }
    private void SwitchQuizType()
    {
        switch (quizType)
        {
            case QuizType.Random:
                ShowRandomQuiz();
                break;
            case QuizType.Order:
                ShowQuizByOrder(); 
                break;
        }
    }
    private string[] GetQuizTitile()
    {
        string streamingPath = Application.streamingAssetsPath;
        string usersPath = Application.persistentDataPath;
        string[] defaultData = ManagerLocater.Instance.loader.LoadJsonFileName(streamingPath);
        string[] userData = ManagerLocater.Instance.loader.LoadJsonFileName(usersPath);
        string[] result = defaultData.Concat(userData).ToArray();
        Debug.Log(string.Join(",", result));
        return result;
    }
    private string ToAlphabet(int index)
    {
        return ((char)('A' + index)).ToString();
    }
    private void OnChoiceButtonClick(int id)
    {
        //Debug.Log($"Answer={currentQuiz.correctAnswer}");
        //Debug.Log($"Input={id},=>{((char)('A' + id)).ToString()}");
        ShowResult
            ((
                id == currentQuiz.correctAnswer
            ));
    }
    private void InitializeSetUpUI()
    {
        if (ButtonPrefab == null)
        {
            Debug.LogError("Button Prefab is not Exist");
        }
        nextButton.onClick.AddListener(() => OnNextButtonClick());
        backButton.onClick.AddListener(() => ShowTitle());
    }
    public void ShowTitleMenu()
    {
        setupPanel.SetActive(false);
        resultPanel.SetActive(false);
        enddingPanel.SetActive(false);
        errorMessage.text = "";
        nextText.text = "Next";
        setupPanel.SetActive(true);
        if (!isQuizTitileSet)
        {
            CreateQuizTitleButton();
            isQuizTitileSet = true;
        }
    }
    private void CreateQuizTitleButton()
    {
        for (int i = 0; i < QuizTitiles.Length; i++) 
        {
            int index = i;
            GameObject clone = Instantiate(quizTitleButton,prefabParent);
            Button button = clone.GetComponent<Button>();
            if (button == null) button = clone.AddComponent<Button>();
            button.onClick.AddListener(() => TitleSetUp(QuizTitiles[index]));
            TextMeshProUGUI text = clone.GetComponentInChildren<TextMeshProUGUI>();
            if (text == null) text = clone.AddComponent<TextMeshProUGUI>();
            text.text = QuizTitiles[i];
        }
    }
    private void ShowRandomQuiz()
    {
        if (quizData.Count == 0)
        {
            Debug.LogWarning("クイズデータがありません");
            return;
        }

        if (sortQuizData.Count == 0)
        {
            sortQuizData = quizData.OrderBy(i => System.Guid.NewGuid()).ToList();
        }
        setupPanel.SetActive(false);
        ShowQuiz(sortQuizData[quizindex]);
    }
    private void ShowQuizByOrder()
    {
        if (quizData.Count == 0)
        {
            Debug.LogWarning("クイズデータがありません");
            return;
        }
        if (sortQuizData.Count == 0)
        {
            sortQuizData = quizData.ToList();
        }
        setupPanel.SetActive(false);
        ShowQuiz(sortQuizData[quizindex]);
    }
    private void ShowQuiz(QuizData quizdata)
    {
        quizindex++;
        currentQuiz = quizdata;
        questionText.text = "";
        imageQuizText.text = "";
        QuizImage.sprite = null;

        if (quizdata is ImageQuizData image)
        {
            QuizImage.sprite = image.quizImage;
            QuizImage.preserveAspect = true;
            imageQuizText.text = image.questionText;
        }
        else if(quizdata is TextQuizData text)
        {
            questionText.text = text.questionText;
        }
        //GetType()で分岐処理
        progressText.text = quizindex.ToString() + "/" + sortQuizData.Count;
        
        for(int i=0;i<currentQuiz.choices.Length;i++)
        {
            Color32[] mycolor = 
            {
                new (168,255,0,255),
                new (0,255,168,255),
                new (255,168,0,255),
                new (0,168,168,255),
                new (168,168,0,255),
                new (168,0,168,255),
            };
            //iは参照,indexは値
            //iをラムダに渡すと、ループが終わったとき、i = currentQuiz.choices.Length
            //変数キャプチャしないと、ラムダ式で参照されるやつが最後のやつになる。
            int index = i;
            //生成済みなら出現させる
            if (index < prefabsList.Count)
            {
                prefabsList[index].SetActive(true);
            }
            //選択肢生成
            else
            {
                GameObject clone = Instantiate(ButtonPrefab, ButtonParent.transform);
                Button _button = clone.GetComponent<Button>();
                if (_button != null)
                {
                    Debug.Log($"Buttonid={index}");
                    _button.onClick.AddListener(() => OnChoiceButtonClick(index));
                }
                AnswerButtonAccesser button_accesser = clone.GetComponent<AnswerButtonAccesser>();
                if (button_accesser != null)
                {
                    button_accesser.SetTag(((char)('A' + index)).ToString());
                }
                //色変更
                Image _image = clone.GetComponent<Image>();
                if (_image != null) _image.color = mycolor[index % 6];

                //リスト追加
                prefabsList.Add(clone);
            }
            //UpDateQuestion
            AnswerButtonAccesser _accesser = prefabsList[index].GetComponent<AnswerButtonAccesser>();
            if (_accesser != null)
            {
                _accesser.UpdateQuestion(currentQuiz.choices[index]);
            }
            
        }
        //余剰分を消去
        for(int i = currentQuiz.choices.Length; i< prefabsList.Count; i++)
        {
            prefabsList[i].SetActive(false);
        }

        //Girdのサイズ調整
        if(currentQuiz.choices.Length < 5)
        {
            QuizGrid.cellSize = defaultGridSize;
        }
        else
        {
            QuizGrid.cellSize = expandGridSize;
        }

            resultPanel.SetActive(false);
        
    }
    
    private void ShowResult(bool is_correct)
    {
        if (is_correct) 
        {
            AddCount();
            resultText.text = "正解！";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = $"不正解...\n正解は「{ToAlphabet(currentQuiz.correctAnswer)}」です";
            resultText.color = Color.red;
        }
        if (explainText != null)
        {
            explainText.text = $"解説:\n{currentQuiz.explanation}";
        }
        if(quizindex == sortQuizData.Count)
        {
            nextText.text = "Result";
        }

        resultPanel.SetActive(true);
    }
    void ShowEndding()
    {
        collectCountText.text = $"正解割合:{collectCount.ToString()}/{quizData.Count}";
        enddingPanel.SetActive(true);
    }

    private void OnNextButtonClick()
    {
        if (sortQuizData.Count == quizindex)
        {
            gameModeType = GameModeType.End;
            ShowEndding();
            return;
        }
        StartQuiz();
    }
    public void AddCount()
    {
        collectCount++;
    }
}


////Inspectorに実行ボタンを追加するクラス
////拡張するクラス
//[CustomEditor(typeof(GameManager))]
//public class InspecterStartButton : Editor//Editorを継承
//{
//    //このクラスがインスペクターに表示される場合のオーバーライド
//    public override void OnInspectorGUI()
//    {
//        //元のインスペクターを表示
//        //Base ->基底クラス(Editor)のメンバーのアクセサ
//        base.OnInspectorGUI();

//        //Editorの変数target(インスペクターに表示される対象を上書き)
//        //targetを拡張するクラスに変換する
//        GameManager _quizUIManager = this.target as GameManager;//(QuizUIManager)this.target;と同義(キャストできない場合にnullが出る)
        
//        if (GUILayout.Button("Start Quiz"))
//        {
//            //public関数の場合
//            _quizUIManager.StartQuiz();
//            //private関数の場合
//            //関数にメッセージを送信->発火
//            //SendMessage("関数名",引数,返り値があるか)
//            //元のクラスで[ExecuteAlway]宣言が必要(Editor上で動くメソッドという意味)
//            //_quizUIManager.SendMessage("StartQuiz",null,SendMessageOptions.DontRequireReceiver);
//        }
//    }
//}
