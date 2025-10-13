using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

// [SerializeReference] 対応 Inspector Drawer
[CustomPropertyDrawer(typeof(QuizData2), true)]
public class QuizDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceValue == null)
        {
            // タイプ選択ボタン
            if (GUI.Button(position, "＋ クイズタイプを選択"))
            {
                ShowTypeMenu(property);
            }
        }
        else
        {
            // クラス名をタイトルに表示
            var type = property.managedReferenceValue.GetType();
            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, $"▶ {type.Name}", EditorStyles.boldLabel);

            // 削除ボタン
            Rect btnRect = new Rect(position.x + position.width - 60, position.y, 60, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(btnRect, "削除"))
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
                return;
            }

            // 通常描画
            EditorGUI.indentLevel++;
            var body = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height);
            EditorGUI.PropertyField(body, property, true);
            EditorGUI.indentLevel--;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceValue == null)
            return EditorGUIUtility.singleLineHeight * 1.2f;

        return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight;
    }

    private void ShowTypeMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();

        // QuizData2 を継承する型を探索
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(QuizData2).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var type in types)
        {
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                property.managedReferenceValue = Activator.CreateInstance(type);
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }
}
