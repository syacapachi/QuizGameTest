using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "QuizDataBase")]
public class QuizDataBase :  ScriptableObject
{
    public List<QuizData> quizDatas = new List<QuizData>();

}
[System.Serializable]
public class QuizData 
{
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
}
[Serializable]
public class TextQuizData : QuizData
{
    public string questiontext;
}