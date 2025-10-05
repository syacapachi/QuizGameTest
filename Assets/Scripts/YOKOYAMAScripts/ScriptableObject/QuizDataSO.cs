using UnityEditor;
using UnityEngine;

[System.Serializable]
public class QuizDataSO : ScriptableObject
{
    public string quiztype;         //クイズのタイプ
    public int questionNumber;      // 問題番号
    public string questionText;     // 問題文
    public string[] choices;        // 拡張性を持たせられるように配列化
    public int correctAnswer;    // 解答（A,B,C,D）
    public string explanation;      // 解説
    public string tag;             // タグ

    public void CreateAsset()
    {
        QuizDataSO asset = ScriptableObject.CreateInstance<QuizDataSO>();
        asset = this;
        UnityEngine.Debug.Log("Export Quiz Asset");
        //アセットの作成.
        AssetDatabase.CreateAsset(asset, $"Assets/Quizdata/AddData{questionNumber}.asset");
        //アセットの即時保存(CreateAssetでも保存されるが、キャッシュがある場合されない)
        AssetDatabase.SaveAssets();
    }    
}
