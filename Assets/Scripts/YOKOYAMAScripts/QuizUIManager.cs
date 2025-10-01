using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
[ExecuteAlways]
public class QuizUIManager : MonoBehaviour
{
    [Header("���f�[�^")]
    [SerializeField] QuizDataBase quizDataBase;
    [Header("�o����")]
    [SerializeField] TextMeshProUGUI questionText;
    [Header("�񓚃{�^���v���n�u")]
    [SerializeField] GameObject ButtonParent;
    [SerializeField] GameObject ButtonPrefab;
    [Header("���U���g���")]
    [SerializeField] GameObject resultPanel;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] TextMeshProUGUI explainText;
    [SerializeField] Button nextButton;
    [Header("�o��ݒ�")]
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
    private void ShowRandomQuiz()
    {
        if (quizData.Count == 0)
        {
            Debug.LogError("�N�C�Y�f�[�^������܂���");
            return;
        }

        int randomIndex = Random.Range(0, quizData.Count);
        ShowQuiz(quizData[randomIndex]);
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
        resultPanel.SetActive(true);
    }

    private void OnNextButtonClick()
    {
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
