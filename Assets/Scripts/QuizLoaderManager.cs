using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro.SpriteAssetUtilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


public class QuizLoaderManager : MonoBehaviour
{
    public string[] LoadJsonFileName(string path)
    {
        string[] jsonpath = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);
        string[] result = new string[jsonpath.Length];
        for(int i = 0; i < jsonpath.Length; i++)
        {
            result[i] = Path.GetFileNameWithoutExtension(jsonpath[i]);
        }
        return result;
    }
    
    public IEnumerator LoadJsonCoroutine(string jsonName, System.Action<QuizDataWrapper> onCompleted)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName + ".json");
        Debug.Log(filePath);

        if (!File.Exists(filePath))
        {
            Debug.LogError("Json File is Not Exist");
            onCompleted?.Invoke(null);
            yield break;
        }

        // JSON読み込み
        string textdata = File.ReadAllText(filePath);
        QuizDataWrapper wrapper = JsonUtility.FromJson<QuizDataWrapper>(textdata);
        List<QuizData> result = new List<QuizData>();

        foreach (QuizData q in wrapper.quizDatas)
        {
            if(q == null)
            {
                Debug.Log("q is Null");
                continue;
            }
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
                        // URL画像読み込み (UnityWebRequestTexture使用)
                        yield return StartCoroutine(ImageLoader.LoadSpriteFromURLCoroutine(questionimage.imageName, sp => questionimage.quizImage = sp));
                    }
                    else
                    {
                        Debug.Log("Image Loading...");
                        string folderPath = Path.Combine("ImageData", questionimage.imageName);
                        string fullPath = Path.Combine(Application.streamingAssetsPath, folderPath);

                        if (!File.Exists(fullPath))
                        {
                            Debug.LogError($"❌ File not found: {fullPath}");
                            continue;
                        }

                        // UnityWebRequestTextureを使って画像を読み込む
                        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + fullPath))
                        {
                            yield return uwr.SendWebRequest();

                            if (uwr.result != UnityWebRequest.Result.Success)
                            {
                                Debug.LogError($"Image Load Error: {uwr.error}");
                            }
                            else
                            {
                                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                                questionimage.quizImage = Sprite.Create(
                                    tex,
                                    new Rect(0, 0, tex.width, tex.height),
                                    Vector2.zero
                                );
                                Debug.Log($"✅ Image Loaded ({tex.width}x{tex.height})");
                            }
                        }
                    }
                }
                result.Add(questionimage);
            }
        }

        Debug.Log($"Load from Json count = {result.Count}");
        wrapper.quizDatas = result.ToArray();

        onCompleted?.Invoke(wrapper);
    }
    public void ExportJson(QuizDataWrapper wrapper)
    {
        var list = new List<QuizData>();
        foreach (var q in wrapper.quizDatas)
        {
            if (q is ImageQuizData imgQ && imgQ.quizImage != null)
            {
                // Spriteが設定されている場合、StreamingAssetsに移動しパスをimagePathに書き込む
                // Asset以下のパス
                string AssetPath = AssetDatabase.GetAssetPath(imgQ.quizImage);
                string fileName = Path.GetFileName(AssetPath);
                //絶対パスE://
                string exportDir = Path.Combine(Application.streamingAssetsPath, "ImageData");
                if (!Directory.Exists(exportDir))
                    Directory.CreateDirectory(exportDir);

                string exportPath = Path.Combine(exportDir, fileName);
                Debug.Log(AssetPath + "\n" + exportPath);
                if (!File.Exists(exportPath))
                {
                    File.Copy(AssetPath, exportPath);
                    Debug.Log($"✅ Copied image to StreamingAssets: {exportPath}");
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    imgQ.imageName = fileName;
                    imgQ.isUrlImage = fileName.StartsWith("http");
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
        string writePath = Path.Combine(Application.streamingAssetsPath, wrapper.quizTitle + ".loader");
        File.WriteAllText(writePath, json);
        Debug.Log($"ExportedJson:\n{json}");
    }
}

//[CustomEditor(typeof(QuizLoadFromJson))]
//public class JsonLoder : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();
//        QuizLoadFromJson _json = target as QuizLoadFromJson;
//        if (GUILayout.Button("Load from Json file"))
//        {
//            _json.Load();
//        }
//        if (GUILayout.Button("Export Json file"))
//        {
//            _json.Export();
//        }
//    }
//}
