using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


//ゲームで実際に使われる方
[System.Serializable]
public class QuizDataWrapper
{
    [SerializeField] public QuizType quizType;
    public string quizTitle = "";
    [SerializeReference]
    public QuizData[] quizDatas;

    public List<QuizData> LoadJson(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName + ".loader");
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
    public async Task<List<QuizData>> LoadJsonAsync(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName + ".loader");
        Debug.Log(filePath);
        string textdata = File.ReadAllText(filePath);
        if (textdata == null)
        {
            Debug.LogError("Json File is Not Exist");
        }
        // 共通部分を読み込み
        QuizDataWrapper wrapper = JsonUtility.FromJson<QuizDataWrapper>(textdata);

        //Debug.Log($"wrapperLoad:{JsonUtility.ToJson(wrapper, true)}");//OK
        List<QuizData> result = new List<QuizData>();
        //List<QuizDataSO> resultSO = new List<QuizDataSO>();
        foreach (QuizData q in wrapper.quizDatas)
        {
            //Debug.Log($"q:{q}");
            Debug.Log($"q.quiztype:{q.quiztype}");//Null
            //クラス別の派生部分を選択(要素をいったんTextに直してからリロード)
            if (q.quiztype == Quiztype.text.ToString())
            {
                TextQuizData questiontext = JsonUtility.FromJson<TextQuizData>(JsonUtility.ToJson(q));
                result.Add(questiontext);
            }
            else if (q.quiztype == Quiztype.image.ToString())
            {
                ImageQuizData questionimage = JsonUtility.FromJson<ImageQuizData>(JsonUtility.ToJson(q));
                if (!string.IsNullOrEmpty(questionimage.imageName))
                {
                    if (questionimage.isUrlImage)
                    {
                        // URL画像ロード
                        await ImageLoader.LoadSpriteFromURL(questionimage.imageName, sp => questionimage.quizImage = sp);
                    }
                    else
                    {
                        Debug.Log("Image Loading...");
                        string folderPath = Path.Combine("ImageData",questionimage.imageName);
                        string fullPath = Path.Combine(Application.streamingAssetsPath,folderPath);
                        Debug.Log(fullPath);
                        // ファイル存在チェック
                        if (!File.Exists(fullPath))
                        {
                            Debug.LogError($"❌ File not found: {fullPath}");
                        }
                        byte[] imageBinary = await File.ReadAllBytesAsync(fullPath);
                        Debug.Log($"✅ Image Loaded ({imageBinary.Length} bytes)");

                        //だめでした...
                        //questionimage.quizImage = ImageLoader.SpriteFromByteArray(imageBinary);

                        // Unityメインスレッドで実行(エディターでは動かない)
                        await UnityMainThreadDispatcher.RunOnMainThreadAsync(() =>
                        {
                            Texture2D loadTexture = new Texture2D(2, 2);
                            if (!loadTexture.LoadImage(imageBinary))
                            {
                                Debug.LogWarning("⚠ LoadImage failed");
                                return;
                            }

                            Debug.Log("Create Sprite Data");
                            questionimage.quizImage = Sprite.Create(
                                loadTexture,
                                new Rect(0, 0, loadTexture.width, loadTexture.height),
                                Vector2.zero
                            );
                        });

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
                string AssetPath = AssetDatabase.GetAssetPath(imgQ.quizImage);
                string path = Path.GetFileName(AssetPath);
                if (!string.IsNullOrEmpty(path))
                {
                    imgQ.imageName = path;
                    imgQ.isUrlImage = path.StartsWith("http");
                }
            }
            list.Add(q);
        }
        wrapper.quizDatas = list.ToArray();
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
        string writePath = Path.Combine(Application.streamingAssetsPath , jsonName + ".loader");
        File.WriteAllText(writePath, json);
        Debug.Log($"ExportedJson:\n{json}");
        //ProjectWindowを再読み込み,変更を適応
        AssetDatabase.Refresh();
    }
}
[Serializable]
public enum Quiztype
{
    text,
    image,
    other
}
[System.Serializable]
public abstract class QuizData 
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
    [HideInInspector]
    public string imageName;
    [HideInInspector]//WebGLビルド用
    public bool isUrlImage;
    public ImageQuizData()
    {
        quiztype = "image";
    }
#if UNITY_EDITOR
    // エディタからアクセスできるプロパティ
    public Sprite EditorSprite => quizImage;
#endif
}
[System.Serializable]
public class TextQuizData : QuizData
{
    public TextQuizData()
    {
        quiztype = "text";
    }
}

