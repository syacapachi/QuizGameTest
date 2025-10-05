using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static QuizManager;
//[ExecuteAlways]
public class QuizUIManager : MonoBehaviour
{
    [Header("���f�[�^�A�Z�b�g")]
    [SerializeField] QuizDataWrapperSO quizDataBase;
    [Header("���json")]
    [SerializeField] string jsonName = "quiz_data";
    [Header("�Z�b�g�A�b�vUI")]
    [SerializeField] GameObject setupPanel;
    [SerializeField] TextMeshProUGUI errorMessage;
    [Header("�o����")]
    [SerializeField] TextMeshProUGUI questionText;
    [Header("�񓚃{�^���v���n�u")]
    [SerializeField] GameObject ButtonParent;
    [SerializeField] GameObject ButtonPrefab;
    [SerializeField] TextMeshProUGUI progressText;
    [Header("���U���g���")]
    [SerializeField] GameObject resultPanel;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] TextMeshProUGUI explainText;
    [SerializeField] TextMeshProUGUI nextText;
    [SerializeField] Button nextButton;
    [Header("EnddingUI")]
    [SerializeField] GameObject enddingPanel;
    [Header("�o��ݒ�")]
    [SerializeField] GameModeType gameModeType = GameModeType.Setup;
    [SerializeField] int quizID = 0;
    [SerializeField] string quiztag = "";
    private List<QuizData> quizData = new List<QuizData>();
    private List<QuizData> sortQuizData = new List<QuizData>();
    private int quizindex = 0;
    private QuizData currentQuiz;
    private List<GameObject> prefabsList = new List<GameObject>();
    private QuizDataWrapper wrapper = new QuizDataWrapper();


    private enum GameModeType
    {
        Setup,
        Random,
        ID,
        Tag,
        End,
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        quizData = QuizLoadFromJson(jsonName);
        SetUpUI();
        ShowSetup();
        
    }
    public List<QuizData> QuizLoadFromJson(string jsonpath)
    {
        return wrapper.LoadJson(jsonpath);
    }

    public void StartQuiz()
    {
        switch (gameModeType)
        {
            case GameModeType.Setup:
                ShowSetup();
                break;
            case GameModeType.Random:
                ShowRandomQuiz();
                break;
            case GameModeType.ID:
                //ShowQuizByNumber(testQuestionNumber);
                break;
            case GameModeType.Tag:
                //ShowQuizByTag(testTag);
                break;
            case GameModeType.End:
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
    public void ShowSetup()
    {
        setupPanel.SetActive(false);
        resultPanel.SetActive(false);
        enddingPanel.SetActive(false);
        errorMessage.text = "";
        nextText.text = "Next";
        setupPanel.SetActive(true);
    }
    private void ShowRandomQuiz()
    {
        if (quizData.Count == 0)
        {
            Debug.LogError("�N�C�Y�f�[�^������܂���");
            return;
        }
        gameModeType = GameModeType.Random;

        if (sortQuizData.Count == 0)
        {
            sortQuizData = quizData.OrderBy(i => System.Guid.NewGuid()).ToList();
        }
        setupPanel.SetActive(false);
        ShowQuiz(sortQuizData[quizindex]);
    }
    private void ShowQuiz(QuizData quizdata)
    {
        quizindex++;
        currentQuiz = quizdata;
        questionText.text = quizdata.questionText;

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
            //i�͎Q��,index�͒l
            //i�������_�ɓn���ƁA���[�v���I������Ƃ��Ai = currentQuiz.choices.Length
            //�ϐ��L���v�`�����Ȃ��ƁA�����_���ŎQ�Ƃ�������Ō�̂�ɂȂ�B
            int index = i;
            //�����ς݂Ȃ�o��������
            if (index < prefabsList.Count)
            {
                prefabsList[index].SetActive(true);
            }
            //�I��������
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
                //�F�ύX
                Image _image = clone.GetComponent<Image>();
                if (_image != null) _image.color = mycolor[index % 6];

                //���X�g�ǉ�
                prefabsList.Add(clone);
            }
            //UpDateQuestion
            AnswerButtonAccesser _accesser = prefabsList[index].GetComponent<AnswerButtonAccesser>();
            if (_accesser != null)
            {
                _accesser.UpdateQuestion(currentQuiz.choices[index]);
            }
            
        }
        //�]�蕪������
        for(int i = currentQuiz.choices.Length; i< prefabsList.Count; i++)
        {
            prefabsList[i].SetActive(false);
        }


        resultPanel.SetActive(false);
        
    }
    
    private void ShowResult(bool is_correct)
    {
        if (is_correct) 
        {
            resultText.text = "�����I";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = $"�s����...\n�����́u{ToAlphabet(currentQuiz.correctAnswer)}�v�ł�";
            resultText.color = Color.red;
        }
        if (explainText != null)
        {
            explainText.text = $"���:\n{currentQuiz.explanation}";
        }
        if(quizindex == sortQuizData.Count)
        {
            nextText.text = "Result";
        }

        resultPanel.SetActive(true);
    }
    void ShowEndding()
    {

    }

    private void OnNextButtonClick()
    {
        if (sortQuizData.Count == quizindex)
        {
            gameModeType = GameModeType.End;
            enddingPanel.SetActive(true);
            return;
        }
        StartQuiz();
    }
    
}


//Inspector�Ɏ��s�{�^����ǉ�����N���X
//�g������N���X
[CustomEditor(typeof(QuizUIManager))]
public class InspecterStartButton : Editor//Editor���p��
{
    //���̃N���X���C���X�y�N�^�[�ɕ\�������ꍇ�̃I�[�o�[���C�h
    public override void OnInspectorGUI()
    {
        //���̃C���X�y�N�^�[��\��
        //Base ->���N���X(Editor)�̃����o�[�̃A�N�Z�T
        base.OnInspectorGUI();

        //Editor�̕ϐ�target(�C���X�y�N�^�[�ɕ\�������Ώۂ��㏑��)
        //target���g������N���X�ɕϊ�����
        QuizUIManager _quizUIManager = this.target as QuizUIManager;//(QuizUIManager)this.target;�Ɠ��`(�L���X�g�ł��Ȃ��ꍇ��null���o��)

        if (GUILayout.Button("Start Quiz"))
        {
            //public�֐��̏ꍇ
            _quizUIManager.StartQuiz();
            //private�֐��̏ꍇ
            //�֐��Ƀ��b�Z�[�W�𑗐M->����
            //SendMessage("�֐���",����,�Ԃ�l�����邩)
            //���̃N���X��[ExecuteAlway]�錾���K�v(Editor��œ������\�b�h�Ƃ����Ӗ�)
            //_quizUIManager.SendMessage("StartQuiz",null,SendMessageOptions.DontRequireReceiver);
        }
    }
}
