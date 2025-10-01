using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
[ExecuteAlways]
public class QuizUIManager : MonoBehaviour
{
    [Header("問題データ")]
    [SerializeField] QuizDataBase quizDataBase;
    [Header("出題画面")]
    [SerializeField] TextMeshProUGUI questionText;
    [Header("回答ボタンプレハブ")]
    [SerializeField] GameObject ButtonParent;
    [SerializeField] GameObject ButtonPrefab;
    [Header("リザルト画面")]
    [SerializeField] GameObject resultPanel;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] TextMeshProUGUI explainText;
    [SerializeField] Button nextButton;
    [Header("出題設定")]
    [SerializeField] GameMode gameMode = GameMode.Random;
    [SerializeField] int quizID = 0;
    [SerializeField] string quiztag = "";
    private List<QuizData> quizData = new List<QuizData>();
    private QuizData currentQuiz;
    private List<GameObject> prefabsList = new List<GameObject>();

    private enum GameMode
    {
        Random,
        ID,
        Tag
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        quizData = quizDataBase.quizDatas;
        SetUpUI();
    }

    public void StartQuiz()
    {
        switch (gameMode)
        {
            case GameMode.Random:
                ShowRandomQuiz();
                break;
            case GameMode.ID:
                break;
            case GameMode.Tag:
                break;
        }
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
    private void SetUpUI()
    {
        if (ButtonPrefab == null)
        {
            Debug.LogError("Button Prefab is not Exist");
        }
        resultPanel.SetActive(false);
        nextButton.onClick.AddListener(() => OnNextButtonClick());
    }
    private void ShowQuiz(QuizData quizdata)
    {
        currentQuiz = quizdata;
        questionText.text = quizdata.questionText;
        
        for(int i=0;i<currentQuiz.choices.Length;i++)
        {
            Color32[] mycolor = 
            {
                new Color32(168,255,0,255),
                new Color32(0,255,168,255),
                new Color32(255,168,0,255),
                new Color32(0,168,168,255),
                new Color32(168,168,0,255),
                new Color32(168,0,168,255),
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


        resultPanel.SetActive(false);
    }
    private void ShowRandomQuiz()
    {
        if (quizData.Count == 0)
        {
            Debug.LogError("クイズデータがありません");
            return;
        }

        int randomIndex = Random.Range(0, quizData.Count);
        ShowQuiz(quizData[randomIndex]);
    }
    private void ShowResult(bool is_correct)
    {
        if (is_correct) 
        {
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
        resultPanel.SetActive(true);
    }

    private void OnNextButtonClick()
    {
        StartQuiz();
    }
    
}


//Inspectorに実行ボタンを追加するクラス
//拡張するクラス
[CustomEditor(typeof(QuizUIManager))]
public class InspecterStartButton : Editor//Editorを継承
{
    //このクラスがインスペクターに表示される場合のオーバーライド
    public override void OnInspectorGUI()
    {
        //元のインスペクターを表示
        //Base ->基底クラス(Editor)のメンバーのアクセサ
        base.OnInspectorGUI();

        //Editorの変数target(インスペクターに表示される対象を上書き)
        //targetを拡張するクラスに変換する
        QuizUIManager _quizUIManager = this.target as QuizUIManager;//(QuizUIManager)this.target;と同義(キャストできない場合にnullが出る)

        if (GUILayout.Button("Start Quiz"))
        {
            //public関数の場合
            _quizUIManager.StartQuiz();
            //private関数の場合
            //関数にメッセージを送信->発火
            //SendMessage("関数名",引数,返り値があるか)
            //元のクラスで[ExecuteAlway]宣言が必要(Editor上で動くメソッドという意味)
            //_quizUIManager.SendMessage("StartQuiz",null,SendMessageOptions.DontRequireReceiver);
        }
    }
}
