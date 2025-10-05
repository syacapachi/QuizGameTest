using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


//ゲームで実際に使われる方
public class QuizDataWrapper : QuizData
{
    public List<QuizData> quizDatas = new List<QuizData>();


    public List<QuizData> LoadJson(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName+".json");
        string json = File.ReadAllText(filePath);
        if (json == null)
        {
            Debug.LogError("Json File is Not Exist");
        }
        // 共通部分を読み込み
        QuizDataWrapper wapper = JsonUtility.FromJson<QuizDataWrapper>(json);
        List<QuizData> result = new List<QuizData>();
        foreach (QuizData q in wapper.quizDatas)
        {
            //クラス別の派生部分を選択
            if (q.quiztype == "text")
            {
                TextQuizData questiontext = JsonUtility.FromJson<TextQuizData>(JsonUtility.ToJson(q));
                result.Add(questiontext);
            }
            else if (q.quiztype == "image")
            {
                ImageQuizData questionimage = JsonUtility.FromJson<ImageQuizData>(JsonUtility.ToJson(q));
                result.Add(questionimage);
            }
        }
        Debug.Log($"Load from Json count = {result.Count}");
        quizDatas = result;
        return result;
    }
    public void ExportJson(string jsonname)
    {
        QuizDataWrapper wapper = this;
        wapper.quizDatas = this.quizDatas;

        //一時領域のパス
        //Application.temporaryCachePath

        //ストリーミングアセットのパス(StreamingAsset直下)
        //Application.streamingAssetsPath

        //Unityが利用するデータが保存されるパス(Asset直下)
        //Application.dataPath

        //実行中に保存されるファイルがあるパス
        //Application.persistentDataPath
        string json = JsonUtility.ToJson(wapper, true);
        File.WriteAllText(Application.streamingAssetsPath + "/" + jsonname + ".json", json);
        Debug.Log($"ExportedJson:\n{json}");
    }
}

[System.Serializable]
public class QuizData 
{
    public string quiztype;         //クイズのタイプ
    public int questionNumber;      // 問題番号
    public string questionText;     // 問題文
    public string[] choices;        // 拡張性を持たせられるように配列化
    public int correctAnswer;    // 解答（A,B,C,D）
    public string explanation;      // 解説
    public string tag;             // タグ

    public QuizData(string[] csvData)
    {
        if (csvData.Length >= 9)
        {
            int.TryParse(csvData[0], out questionNumber);
            questionText = csvData[1];
            choices = csvData[2..^3];
            correctAnswer = Int32.Parse(csvData[^3]);
            explanation = csvData[^2];
            tag = csvData[^1];
        }
    }
    public QuizData()
    {
        
    }
    // 正答判定
    public bool IsCorrect(int answer)
    {
        return correctAnswer.Equals(answer);
    }

    // 選択肢を配列として取得
    public string[] GetChoices()
    {
        return choices;
    }
}

[Serializable]
public class ImageQuizData : QuizData
{
    public string imageurl;
    public string caption;
}
[Serializable]
public class TextQuizData : QuizData
{
    public string questiontext;
}