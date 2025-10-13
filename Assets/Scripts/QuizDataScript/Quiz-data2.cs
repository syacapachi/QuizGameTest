using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


//�Q�[���Ŏ��ۂɎg�����
public class QuizData2Wrapper
{
    public string quizTitle = "";
    [SerializeReference]
    public QuizData2[] quizDatas;

    public List<QuizData2> LoadJson(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName + ".textdata");
        string textdata = File.ReadAllText(filePath);
        if (textdata == null)
        {
            Debug.LogError("Json File is Not Exist");
        }
        // ���ʕ�����ǂݍ���
        QuizData2Wrapper wapper = JsonUtility.FromJson<QuizData2Wrapper>(textdata);
        List<QuizData2> result = new List<QuizData2>();
        foreach (QuizData2 q in wapper.quizDatas)
        {
            //�N���X�ʂ̔h��������I��
            if (q.quiztype == Quiztype.text.ToString())
            {
                TextQuizData2 questiontext = JsonUtility.FromJson<TextQuizData2>(JsonUtility.ToJson(q));
                result.Add(questiontext);
            }
            else if (q.quiztype == Quiztype.image.ToString())
            {
                ImageQuizData2 questionimage = JsonUtility.FromJson<ImageQuizData2>(JsonUtility.ToJson(q));
                result.Add(questionimage);
            }
        }
        Debug.Log($"Load from Json count = {result.Count}");
        quizTitle = wapper.quizTitle;
        quizDatas = result.ToArray();
        return result;
    }
    public void ExportJson(string jsonname)
    {
        QuizData2Wrapper wapper = this;
        wapper.quizDatas = this.quizDatas;

        //�ꎞ�̈�̃p�X
        //Application.temporaryCachePath

        //�X�g���[�~���O�A�Z�b�g�̃p�X(StreamingAsset����)
        //Application.streamingAssetsPath

        //Unity�����p����f�[�^���ۑ������p�X(Asset����)
        //Application.dataPath

        //���s���ɕۑ������t�@�C��������p�X
        //Application.persistentDataPath
        string json = JsonUtility.ToJson(wapper, true);
        File.WriteAllText(Application.streamingAssetsPath + "/" + jsonname + ".textdata", json);
        Debug.Log($"ExportedJson:\n{json}");
    }
}

[System.Serializable]
public class QuizData2
{
    public int questionNumber;      // ���ԍ�        
    public string questionText;     // ��蕶
    public string[] choices;        // �g����������������悤�ɔz��
    public int correctAnswer;       // �𓚁iA,B,C,D�j
    public string explanation;      // ���
    public string tag;             // �^�O
    [HideInInspector]
    public string quiztype = "base";//�N�C�Y�̃^�C�v

}

[System.Serializable]
public class ImageQuizData2 : QuizData2
{
    public Sprite quizImage;
    public string imagePath;
    [HideInInspector]//WebGL�r���h�p
    public bool isUrlImage;
    public string caption;
    public ImageQuizData2()
    {
        quiztype = "image";
    }
}
[System.Serializable]
public class TextQuizData2 : QuizData2
{
    public string questiontext;
    public TextQuizData2()
    {
        quiztype = "text";
    }
}

