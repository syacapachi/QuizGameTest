using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

//Editorでクイズデータを作る時用
//アタッチするためには、1ファイル,1Unity関連クラス(ファイル名と一致)
//元のやつと構成は同じにすること
[CreateAssetMenu(menuName = "QuizDataWrapperSO")]
public class QuizDataWrapperSO : ScriptableObject
{
    public string quizTitle = "";
    //public List<QuizDataSO> quizDatasSO = new List<QuizDataSO>(); 
    [SerializeReference,SerializeReferenceView(typeof(QuizData))]
    public QuizData[] quizDatas ;

    public async Task<List<QuizData>> LoadJson(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName+".json");
        string textdata = File.ReadAllText(filePath);
        if (textdata == null)
        {
            Debug.LogError("Json File is Not Exist");
        }
        // 共通部分を読み込み
        QuizDataWrapper wrapper = JsonUtility.FromJson<QuizDataWrapper>(textdata);

        Debug.Log($"wrapperLoad:{JsonUtility.ToJson(wrapper,true)}");//OK
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
                if (!string.IsNullOrEmpty(questionimage.imageName))
                {
                    
                    Debug.Log("Image Loading...");
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

                    ////メインスレッドセットアップ
                    //SynchronizationContext _mainContext;
                    //_mainContext = SynchronizationContext.Current;
                    //_mainContext.Post(_ =>
                    //{
                    //   questionimage.quizImage = ImageLoader.SpriteFromByteArray(imageBinary);
                    //}, null);
#if UNITY_EDITOR
                    // Editor用プレビュー反映
                    var so = new SerializedObject(this);
                    so.Update();
                    // EditorSpriteにはEditor上で表示したいスプライトを反映
                    typeof(ImageQuizData)
                        //変数をさがす、NonPublic && Instanceで、private,protected変数も可能
                        .GetField("quizImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                        //安全に代入ImageQuizData questuinimage,quizImage = 右辺；(同じ値を再代入)
                        .SetValue(questionimage, questionimage.quizImage);
#endif


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
        //SOではなく、正しいデータを使用
        QuizDataWrapper wrapper = new QuizDataWrapper();
        wrapper.quizTitle = quizTitle;
        
        var list = new List<QuizData>();
        foreach (var q in quizDatas)
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
                Debug.Log(AssetPath +"\n"+ exportPath);
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
        string writePath = Path.Combine(Application.streamingAssetsPath, jsonName + ".json");
        File.WriteAllText(writePath, json);
        Debug.Log($"ExportedJson:\n{json}at{writePath}");
#if UNITY_EDITOR
        //ProjectWindowを再読み込み,変更を適応
        AssetDatabase.Refresh();
#endif
    }
    
}



[CustomEditor(typeof(QuizDataWrapperSO))]
public class QuizDadaSOInEditot : Editor
{
    //クラスのメンバーじゃ無いと更新できない
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
        //変更を上書きするやつ
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
        //変更を許可するみたいなやつ
        serializedObject.ApplyModifiedProperties();
    }
}