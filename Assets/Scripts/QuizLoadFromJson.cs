using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro.SpriteAssetUtilities;
using UnityEditor;
using UnityEngine;


public class QuizLoadFromJson : MonoBehaviour
{
    [Header("�ΏۃN�C�Y�f�[�^�x�[�X")]
    [SerializeField] public QuizDataWrapperSO dataDase; // �����f�t�H���g
    [SerializeField] string jsonLoadpath = "quiz_data";
    [SerializeField] string jsonExportName = "quiz_data";

    public void Load() 
    {
        dataDase.LoadJson(jsonLoadpath);   
    }
    public void Export()
    {
        dataDase.ExportJson(jsonExportName);

    }
}

[CustomEditor(typeof(QuizLoadFromJson))]
public class JsonLoder : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        QuizLoadFromJson _json = target as QuizLoadFromJson;
        if (GUILayout.Button("Load from Json file"))
        {
            _json.Load();
        }
        if (GUILayout.Button("Export Json file"))
        {
            _json.Export();
        }
    }
}
