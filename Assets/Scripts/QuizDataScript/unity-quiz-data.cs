using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


//ゲームで実際に使われる方
public class QuizDataWrapper
{
    public string quizTitle = "";
    [SerializeReference]
    public QuizData[] quizDatas;

    public List<QuizData> LoadJson(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName + ".json");
        string textdata = File.ReadAllText(filePath);
        if (textdata == null)
        {
            Debug.LogError("Json File is Not Exist");
        }
        // 共通部分を読み込み
        QuizDataWrapper wrapper = JsonUtility.FromJson<QuizDataWrapper>(textdata);

        Debug.Log($"wrapperLoad:{JsonUtility.ToJson(wrapper, true)}");//OK
        List<QuizData> result = new List<QuizData>();
        //List<QuizDataSO> resultSO = new List<QuizDataSO>();
        foreach (QuizData q in wrapper.quizDatas)
        {
            //Debug.Log($"q:{q}");
            Debug.Log($"q.quiztype:{q.quiztype}");//Null
            //クラス別の派生部分を選択
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
        QuizDataWrapper wrapper = new QuizDataWrapper();

        wrapper.quizTitle = quizTitle;
        var list = new List<QuizData>();
        foreach (var q in quizDatas)
        {
            if (q is ImageQuizData imgQ && imgQ.quizImage != null)
            {
                // Spriteが設定されている場合、そのパスをimagePathに書き込む
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
        //memo
        //一時領域のパス
        //Application.temporaryCachePath

        //ストリーミングアセットのパス(StreamingAsset直下)
        //Application.streamingAssetsPath

        //Unityが利用するデータが保存されるパス(Asset直下)
        //Application.dataPath

        //実行中に保存されるファイルがあるパス
        //Application.persistentDataPath
        File.WriteAllText(Application.streamingAssetsPath + "/" + jsonName + ".json", json);
        Debug.Log($"ExportedJson:\n{json}");
        //ProjectWindowを再読み込み,変更を適応
        AssetDatabase.Refresh();
    }
}
public enum Quiztype
{
    text,
    image,
    other
}
[System.Serializable]
public class QuizData 
{
    public int questionNumber;      // 問題番号         
    public string questionText;     // 問題文
    public string[] choices;        // 拡張性を持たせられるように配列化
    public int correctAnswer;       // 解答（A,B,C,D）
    public string explanation;      // 解説
    public string tag;             // タグ
    [HideInInspector]
    public string quiztype = "base";//クイズのタイプ

}

[System.Serializable]
public class ImageQuizData : QuizData
{
    public Sprite quizImage;
    public string imagePath;
    [HideInInspector]//WebGLビルド用
    public bool isUrlImage;
    public string caption;
    public ImageQuizData()
    {
        quiztype = "image";
    }
}
[System.Serializable]
public class TextQuizData : QuizData
{
    public string questiontext;
    public TextQuizData()
    {
        quiztype = "text";
    }
}

