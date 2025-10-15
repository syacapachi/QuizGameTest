using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


//ゲームで実際に使われる方
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
        // 共通部分を読み込み
        QuizData2Wrapper wapper = JsonUtility.FromJson<QuizData2Wrapper>(textdata);
        List<QuizData2> result = new List<QuizData2>();
        foreach (QuizData2 q in wapper.quizDatas)
        {
            //クラス別の派生部分を選択
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
    public async Task<List<QuizData2>> LoadJsonAsync(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName + ".loader");
        string textdata = File.ReadAllText(filePath);
        if (textdata == null)
        {
            Debug.LogError("Json File is Not Exist");
        }
        // 共通部分を読み込み
        QuizData2Wrapper wrapper = JsonUtility.FromJson<QuizData2Wrapper>(textdata);

        //Debug.Log($"wrapperLoad:{JsonUtility.ToJson(wrapper, true)}");//OK
        List<QuizData2> result = new List<QuizData2>();
        //List<QuizDataSO> resultSO = new List<QuizDataSO>();
        foreach (QuizData2 q in wrapper.quizDatas)
        {
            //Debug.Log($"q:{q}");
            Debug.Log($"q.quiztype:{q.quiztype}");//Null
            //クラス別の派生部分を選択(要素をいったんTextに直してからリロード)
            if (q.quiztype == Quiztype.text.ToString())
            {
                TextQuizData2 questiontext = JsonUtility.FromJson<TextQuizData2>(JsonUtility.ToJson(q));
                result.Add(questiontext);
            }
            else if (q.quiztype == Quiztype.image.ToString())
            {
                ImageQuizData2 questionimage = JsonUtility.FromJson<ImageQuizData2>(JsonUtility.ToJson(q));
                if (!string.IsNullOrEmpty(questionimage.imageName))
                {
                    if (questionimage.isUrlImage)
                    {
                        // URL画像ロード
                        await ImageLoader.LoadSpriteFromURL(questionimage.imageName, sp => questionimage.quizImage = sp);
                    }
                    else
                    {

                        string resPath = Path.GetFileNameWithoutExtension(questionimage.imageName);
                        byte[] imagebynary = await File.ReadAllBytesAsync(resPath);
                        Texture2D loadTexture = new Texture2D(2, 2);
                        loadTexture.LoadImage(imagebynary);
                        questionimage.quizImage = Sprite.Create(loadTexture, new Rect(0, 0, loadTexture.width, loadTexture.height), Vector2.zero);
                    }
                }
                result.Add(questionimage);
            }
        }
        Debug.Log($"Load from Json count = {result.Count}");
        quizDatas = result.ToArray();
        quizTitle = wrapper.quizTitle;
        return result;
    }
    public void ExportJson(string jsonname)
    {
        QuizData2Wrapper wapper = this;
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
        File.WriteAllText(Application.streamingAssetsPath + "/" + jsonname + ".textdata", json);
        Debug.Log($"ExportedJson:\n{json}");
    }
}

[System.Serializable]
public class QuizData2
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
public class ImageQuizData2 : QuizData2
{
    public Sprite quizImage;
    public string imageName;
    [HideInInspector]//WebGLビルド用
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

