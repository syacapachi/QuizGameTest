using System.Reflection;
using System;
using UnityEditor;

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// [SerializeReferenceView] 用 PropertyDrawer(UnityEditorの最上位)
/// </summary>
[CustomPropertyDrawer(typeof(SerializeReferenceViewAttribute))]
public class SerializeReferenceViewDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (SerializeReferenceViewAttribute)attribute;

        EditorGUI.BeginProperty(position, label, property);

        if (property.managedReferenceValue == null)
        {
            // タイプ選択ボタン
            if (GUI.Button(position, "＋ 型を選択"))
            {
                ShowTypeMenu(property, attr.BaseType);
            }
        }
        else
        {
            // クラス名をタイトルに表示
            var type = property.managedReferenceValue.GetType();
            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, $" {type.Name}", EditorStyles.boldLabel);

            // 削除ボタン 属性をnullにして更新
            Rect btnRect = new Rect(position.x + position.width - 60, position.y, 60, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(btnRect, "削除"))
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
                return;
            }

            // 子プロパティ描画
            EditorGUI.indentLevel++;//インデックスを下へ
            var body = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height);
            //属性に含まれるシリアル化された情報を描画
            EditorGUI.PropertyField(body, property, true);
            EditorGUI.indentLevel--;//インデックスを戻す

            // OnInspectorButton対応
            DrawOnInspectorButtons(property.managedReferenceValue);
        }

        EditorGUI.EndProperty();
    }

    //[OnInspectorButton]に対応
    private void DrawOnInspectorButtons(object instance)
    {
        if (instance == null) return;

        var methods = instance.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<OnInspectorButtonAttribute>() != null);

        foreach (var method in methods)
        {
            var buttonAttr = method.GetCustomAttribute<OnInspectorButtonAttribute>();
            if (buttonAttr.showOnlyInPlayMode && !Application.isPlaying)
                continue;

            string buttonLabel = buttonAttr.label ?? method.Name;
            GUILayout.Space(5);
            EditorGUILayout.LabelField($"▶ {buttonLabel}", EditorStyles.boldLabel);

            var parameters = method.GetParameters();
            var args = new List<object>();

            // 引数GUI
            foreach (var param in parameters)
            {
                args.Add(DrawParameterField(param));
            }

            // ボタン生成
            if (GUILayout.Button($"実行: {buttonLabel}"))
            {
                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        method.Invoke(instance, args.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[OnInspectorButton] {method.Name} 実行エラー: {ex}");
                    }
                };
            }
        }
    }
    private object DrawParameterField(ParameterInfo param)
    {
        Type type = param.ParameterType;
        string label = param.Name;
        object defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;

        if (type == typeof(int))
            return EditorGUILayout.IntField(label, 0);

        if (type == typeof(float))
            return EditorGUILayout.FloatField(label, 0f);

        if (type == typeof(string))
            return EditorGUILayout.TextField(label, "");

        if (type == typeof(bool))
            return EditorGUILayout.Toggle(label, false);

        if (type.IsEnum)
            return EditorGUILayout.EnumPopup(label, (Enum)Enum.GetValues(type).GetValue(0));

        // 不明な型は表示のみ
        EditorGUILayout.LabelField(label, $"未対応型: {type.Name}");
        return defaultValue;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //何もないならスペースを1.2fに
        if (property.managedReferenceValue == null)
            return EditorGUIUtility.singleLineHeight * 1.2f;

        return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight;
    }

    private void ShowTypeMenu(SerializedProperty property, Type baseType)
    {
        GenericMenu menu = new GenericMenu();

        Type fieldType = fieldInfo.FieldType;
        Type targetBase = baseType ?? fieldType;

        // ジェネリックなども含め全型,基底クラスを継承するクラスを探索
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                targetBase.IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface);

        foreach (var type in types)
        {
            string menuName = type.FullName.Replace('.', '/');
            menu.AddItem(new GUIContent(menuName), false, () =>
            {
                property.managedReferenceValue = Activator.CreateInstance(type);
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }
}
