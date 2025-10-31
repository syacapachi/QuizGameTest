﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

//Editorでクイズデータを作る時用
//アタッチするためには、1ファイル,1Unity関連クラス(ファイル名と一致)
//元のやつと構成は同じにすること
//[CreateAssetMenu(menuName = "QuizData2WrapperSO")]
public class QuizData2WrapperSO : ScriptableObject
{
    public string quizTitle = "";
    //public List<QuizDataSO> quizDatasSO = new List<QuizDataSO>(); 
    [SerializeReference]
    public QuizData2[] quizDatas ;

    //外部で呼ぶ必要あり
    //Webから読む場合は、待機が発生するためコルーチンを使用
    private IEnumerator LoadJsonFromWeb(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName + ".json");
        string textdata = "";

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            using (var req = UnityEngine.Networking.UnityWebRequest.Get(filePath))
            {
                yield return req.SendWebRequest();
                textdata = req.downloadHandler.text;
            }
        }
        else
        {
            textdata = File.ReadAllText(filePath);
        }

        if (textdata == null)
        {
            Debug.LogError("Json File is Not Exist");
        }
        // 共通部分を読み込み
        QuizData2Wrapper wrapper = JsonUtility.FromJson<QuizData2Wrapper>(textdata);
        //タイトルを読む
        quizTitle = wrapper.quizTitle;
        //中身を読む
        List<QuizData2> list = new();

        foreach (var q in wrapper.quizDatas)
        {
            string raw = JsonUtility.ToJson(q);//一旦もどす
            QuizData2 newQ = null;

            switch (q.quiztype)
            {
                case "text":
                    newQ = JsonUtility.FromJson<TextQuizData2>(raw);
                    break;
                case "image":
                    var imgQ = JsonUtility.FromJson<ImageQuizData2>(raw);
                    if (!string.IsNullOrEmpty(imgQ.imageName))
                    {
                        if (imgQ.isUrlImage)
                        {
                            // URL画像ロード
                            yield return ImageLoader.LoadSpriteFromURL(imgQ.imageName, sp => imgQ.quizImage = sp);
                        }
                        else
                        {
                            // ローカル（Resources）からロード
                            string resPath = Path.GetFileNameWithoutExtension(imgQ.imageName);
                            imgQ.quizImage = Resources.Load<Sprite>(resPath);
                        }
                    }
                    newQ = imgQ;
                    break;
            }
            list.Add(newQ);
        }

        quizDatas = list.ToArray();
        Debug.Log("JSON読み込み完了");
    }
    public async Task<List<QuizData2>> LoadJsonAsync(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName + ".json");
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
                    //if (questionimage.isUrlImage)
                    //{
                    //    // URL画像ロード
                    //    await ImageLoader.LoadSpriteFromURL(questionimage.imageName, sp => questionimage.quizImage = sp);
                    //}
                    //else
                    //{

                        string folderPath = Path.Combine("ImageData", questionimage.imageName);
                        string fullPath = Path.Combine(Application.streamingAssetsPath, folderPath);
                        Debug.Log(fullPath);
                        // ファイル存在チェック
                        if (!File.Exists(fullPath))
                        {
                            Debug.LogError($"❌ File not found: {fullPath}");
                        }
                        byte[] imageBinary = File.ReadAllBytes(fullPath);//Editorでは同期処理
                        Debug.Log($"✅ Image Loaded ({imageBinary.Length} bytes)");
                        questionimage.quizImage = ImageLoader.SpriteFromByteArray(imageBinary);
                        //Texture2D loadTexture = new Texture2D(2, 2);
                        //loadTexture.LoadImage(imageBinary);
                        //questionimage.quizImage = Sprite.Create(loadTexture,new Rect(0,0,loadTexture.width,loadTexture.height),Vector2.zero);
                    //}
                }
                result.Add(questionimage);
            }
        }
        Debug.Log($"Load from Json count = {result.Count}");
        quizDatas = result.ToArray();
        quizTitle = wrapper.quizTitle;
        return result;
    }
    public void ExportJson()
    {
        //SOではなく、正しいデータを使用;
        QuizData2Wrapper wrapper = new QuizData2Wrapper();
        wrapper.quizTitle = quizTitle;

        var list = new List<QuizData2>();
        foreach (var q in quizDatas)
        {
            if (q is ImageQuizData2 imgQ && imgQ.quizImage != null)
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
        wrapper.quizDatas = quizDatas.ToArray();
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(Application.streamingAssetsPath + "/" + wrapper.quizTitle + ".json", json);
        Debug.Log($"ExportedJson:\n{json}");
        //ProjectWindowを再読み込み,変更を適応
        AssetDatabase.Refresh();
    }

}



[CustomEditor(typeof(QuizData2WrapperSO))]
public class QuizDada2SOInEditot : Editor
{
    //クラスのメンバーじゃ無いと更新できない
    string loadJsonFileName = "loadJsonFileName";


    //private string[] quizTypeOptions = { "Text", "Image" };
    //private SerializedProperty quizTypeStringProp;

    //private void OnEnable()
    //{
    //    quizTypeStringProp = serializedObject.FindProperty("quiztype");
    //}

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //変更を上書きするやつ
        serializedObject.Update();
        //int index = Mathf.Max(0, System.Array.IndexOf(quizTypeOptions, quizTypeStringProp.stringValue));
        //int newIndex = EditorGUILayout.Popup("Quiz Type", index, quizTypeOptions);

        //if (newIndex != index)
        //{
        //    quizTypeStringProp.stringValue = quizTypeOptions[newIndex];
        //}
        QuizData2WrapperSO wrapper = target as QuizData2WrapperSO;

        loadJsonFileName = GUILayout.TextField(loadJsonFileName);
        if (GUILayout.Button("Load Json"))
        {
            wrapper.LoadJsonAsync(loadJsonFileName);
        }
        GUILayout.Space(1);
        if (GUILayout.Button("Export Json"))
        {
            wrapper.ExportJson();
        }
        //変更を許可するみたいなやつ
        serializedObject.ApplyModifiedProperties();
    }
}