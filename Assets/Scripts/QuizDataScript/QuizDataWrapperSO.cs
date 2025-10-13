using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//Editor�ŃN�C�Y�f�[�^����鎞�p
//�A�^�b�`���邽�߂ɂ́A1�t�@�C��,1Unity�֘A�N���X(�t�@�C�����ƈ�v)
//���̂�ƍ\���͓����ɂ��邱��
[CreateAssetMenu(menuName = "QuizDataWrapperSO")]
public class QuizDataWrapperSO : ScriptableObject
{
    public string quizTitle = "";
    //public List<QuizDataSO> quizDatasSO = new List<QuizDataSO>(); 
    [SerializeReference,SerializeReferenceView(typeof(QuizData))]
    public QuizData[] quizDatas ;

    public List<QuizData> LoadJson(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName+".json");
        string textdata = File.ReadAllText(filePath);
        if (textdata == null)
        {
            Debug.LogError("Json File is Not Exist");
        }
        // ���ʕ�����ǂݍ���
        QuizDataWrapper wrapper = JsonUtility.FromJson<QuizDataWrapper>(textdata);

        Debug.Log($"wrapperLoad:{JsonUtility.ToJson(wrapper,true)}");//OK
        List<QuizData> result = new List<QuizData>();
        //List<QuizDataSO> resultSO = new List<QuizDataSO>();
        foreach (QuizData q in wrapper.quizDatas)
        {
            //Debug.Log($"q:{q}");
            Debug.Log($"q.quiztype:{q.quiztype}");//Null
            //�N���X�ʂ̔h��������I��
            if (q.quiztype == Quiztype.text.ToString())
            {
                TextQuizData questiontext = JsonUtility.FromJson<TextQuizData>(JsonUtility.ToJson(q));
                result.Add(questiontext);
            }
            else if (q.quiztype == Quiztype.image.ToString())
            {
                ImageQuizData questionimage = JsonUtility.FromJson<ImageQuizData>(JsonUtility.ToJson(q));
                result.Add(questionimage);
            }
        }
        Debug.Log($"Load from Json count = {result.Count}");
        quizDatas = result.ToArray();
        quizTitle = wrapper.quizTitle;
        return result;
    }
    public void ExportJson(string jsonName)
    {
        //SO�ł͂Ȃ��A�������f�[�^���g�p
        QuizDataWrapper wrapper = new QuizDataWrapper();
        wrapper.quizTitle = quizTitle;
        
        var list = new List<QuizData>();
        foreach (var q in quizDatas)
        {
            if (q is ImageQuizData imgQ && imgQ.quizImage != null)
            {
                // Sprite���ݒ肳��Ă���ꍇ�A���̃p�X��imagePath�ɏ�������
                string path = AssetDatabase.GetAssetPath(imgQ.quizImage);
                if (!string.IsNullOrEmpty(path))
                {
                    imgQ.imagePath = path;
                    imgQ.isUrlImage = path.StartsWith("http");
                }
            }
            list.Add(q);
        }
        wrapper.quizDatas = quizDatas.ToArray();
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(Application.streamingAssetsPath + "/" + jsonName + ".json", json);
        Debug.Log($"ExportedJson:\n{json}");
        //ProjectWindow���ēǂݍ���,�ύX��K��
        AssetDatabase.Refresh();
    }
    
}



[CustomEditor(typeof(QuizDataWrapperSO))]
public class QuizDadaSOInEditot : Editor
{
    //�N���X�̃����o�[���ᖳ���ƍX�V�ł��Ȃ�
    string loadJsonFileName = "loadJsonFileName";
    string exportJsonFileName = "exportJsonFileName";

    //private string[] quizTypeOptions = { "Text", "Image" };
    //private SerializedProperty quizTypeStringProp;

    //private void OnEnable()
    //{
    //    quizTypeStringProp = serializedObject.FindProperty("quiztype");
    //}

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //�ύX���㏑��������
        serializedObject.Update();
        //int index = Mathf.Max(0, System.Array.IndexOf(quizTypeOptions, quizTypeStringProp.stringValue));
        //int newIndex = EditorGUILayout.Popup("Quiz Type", index, quizTypeOptions);

        //if (newIndex != index)
        //{
        //    quizTypeStringProp.stringValue = quizTypeOptions[newIndex];
        //}
        QuizDataWrapperSO wrapper = target as QuizDataWrapperSO;
        
        loadJsonFileName = GUILayout.TextField(loadJsonFileName);
        if(GUILayout.Button("Load Json"))
        {
            wrapper.LoadJson(loadJsonFileName);
        }
        GUILayout.Space(1);
        exportJsonFileName = GUILayout.TextField(exportJsonFileName);
        if (GUILayout.Button("Export Json"))
        {
            wrapper.ExportJson(exportJsonFileName);
        }
        //�ύX��������݂����Ȃ��
        serializedObject.ApplyModifiedProperties();
    }
}