using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

//Editor�ŃN�C�Y�f�[�^����鎞�p
//�A�^�b�`���邽�߂ɂ́A1�t�@�C��,1Unity�֘A�N���X(�t�@�C�����ƈ�v)
//���̂�ƍ\���͓����ɂ��邱��
[CreateAssetMenu(menuName = "QuizData2WrapperSO")]
public class QuizData2WrapperSO : ScriptableObject
{
    public string quizTitle = "";
    //public List<QuizDataSO> quizDatasSO = new List<QuizDataSO>(); 
    [SerializeReference]
    public QuizData2[] quizDatas ;
    
    //�O���ŌĂԕK�v����
    //Web����ǂޏꍇ�́A�ҋ@���������邽�߃R���[�`�����g�p
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
        // ���ʕ�����ǂݍ���
        QuizData2Wrapper wrapper = JsonUtility.FromJson<QuizData2Wrapper>(textdata);
        //�^�C�g����ǂ�
        quizTitle = wrapper.quizTitle;
        //���g��ǂ�
        List<QuizData2> list = new();

        foreach (var q in wrapper.quizDatas)
        {
            string raw = JsonUtility.ToJson(q);//��U���ǂ�
            QuizData2 newQ = null;

            switch (q.quiztype)
            {
                case "text":
                    newQ = JsonUtility.FromJson<TextQuizData2>(raw);
                    break;
                case "image":
                    var imgQ = JsonUtility.FromJson<ImageQuizData2>(raw);
                    if (!string.IsNullOrEmpty(imgQ.imagePath))
                    {
                        if (imgQ.isUrlImage)
                        {
                            // URL�摜���[�h
                            yield return ImageLoader.LoadSpriteFromURL(imgQ.imagePath, sp => imgQ.quizImage = sp);
                        }
                        else
                        {
                            // ���[�J���iResources�j���烍�[�h
                            string resPath = Path.GetFileNameWithoutExtension(imgQ.imagePath);
                            imgQ.quizImage = Resources.Load<Sprite>(resPath);
                        }
                    }
                    newQ = imgQ;
                    break;
            }
            list.Add(newQ);
        }

        quizDatas = list.ToArray();
        Debug.Log("JSON�ǂݍ��݊���");
    }
    public async Task<List<QuizData2>> LoadJsonAsync(string jsonName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonName + ".json");
        string textdata = File.ReadAllText(filePath);
        if (textdata == null)
        {
            Debug.LogError("Json File is Not Exist");
        }
        // ���ʕ�����ǂݍ���
        QuizData2Wrapper wrapper = JsonUtility.FromJson<QuizData2Wrapper>(textdata);

        //Debug.Log($"wrapperLoad:{JsonUtility.ToJson(wrapper, true)}");//OK
        List<QuizData2> result = new List<QuizData2>();
        //List<QuizDataSO> resultSO = new List<QuizDataSO>();
        foreach (QuizData2 q in wrapper.quizDatas)
        {
            //Debug.Log($"q:{q}");
            Debug.Log($"q.quiztype:{q.quiztype}");//Null
            //�N���X�ʂ̔h��������I��(�v�f����������Text�ɒ����Ă��烊���[�h)
            if (q.quiztype == Quiztype.text.ToString())
            {
                TextQuizData2 questiontext = JsonUtility.FromJson<TextQuizData2>(JsonUtility.ToJson(q));
                result.Add(questiontext);
            }
            else if (q.quiztype == Quiztype.image.ToString())
            {
                ImageQuizData2 questionimage = JsonUtility.FromJson<ImageQuizData2>(JsonUtility.ToJson(q));
                if (!string.IsNullOrEmpty(questionimage.imagePath))
                {
                    if (questionimage.isUrlImage)
                    {
                        // URL�摜���[�h
                        await ImageLoader.LoadSpriteFromURL(questionimage.imagePath, sp => questionimage.quizImage = sp);
                    }
                    else
                    {
                        // ���[�J���iResources�j���烍�[�h
                        string resPath = Path.GetFileNameWithoutExtension(questionimage.imagePath);
                        questionimage.quizImage = Resources.Load<Sprite>(resPath);
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
        //SO�ł͂Ȃ��A�������f�[�^���g�p;
        QuizData2Wrapper wrapper = new QuizData2Wrapper();
        wrapper.quizTitle = quizTitle;

        var list = new List<QuizData2>();
        foreach (var q in quizDatas)
        {
            if (q is ImageQuizData2 imgQ && imgQ.quizImage != null)
            {
                // Sprite���ݒ肳��Ă���ꍇ�A���̃p�X��imagePath�ɏ�������
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
        File.WriteAllText(Application.streamingAssetsPath + "/" + jsonName + ".json", json);
        Debug.Log($"ExportedJson:\n{json}");
        //ProjectWindow���ēǂݍ���,�ύX��K��
        AssetDatabase.Refresh();
    }

}



[CustomEditor(typeof(QuizData2WrapperSO))]
public class QuizDada2SOInEditot : Editor
{
    //�N���X�̃����o�[���ᖳ���ƍX�V�ł��Ȃ�
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
        //�ύX���㏑��������
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
        exportJsonFileName = GUILayout.TextField(exportJsonFileName);
        if (GUILayout.Button("Export Json"))
        {
            wrapper.ExportJson(exportJsonFileName);
        }
        //�ύX��������݂����Ȃ��
        serializedObject.ApplyModifiedProperties();
    }
}