#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// [OnInspectorButton]属性を持つメソッドを、Inspectorにボタンとして表示。
/// MonoBehaviour / ScriptableObject 両対応版。
/// ネストした ScriptableObjectも再帰的に描画。
/// </summary>
[CustomEditor(typeof(UnityEngine.Object), true)]
public class OnInspectorButtonEditor : Editor
{
    private Dictionary<string, object[]> methodParameters = new();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        //各インスペクターで呼ばれる。
        var targetType = target.GetType();
        //自分自身は描画しない(エラー回避)
        if (targetType == typeof(OnInspectorButtonEditor)) return;

        // メソッドを列挙
        var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<OnInspectorButtonAttribute>();
            if (attr == null)
                continue;

            // 実行中のみ表示
            if (attr.showOnlyInPlayMode && !Application.isPlaying)
                continue;

            DrawButtonForMethod(method, attr);
        }

        // ネストしたScriptableObjectを再帰的に描画
        DrawNestedScriptableObjects(target);
    }

    private void DrawButtonForMethod(MethodInfo method, OnInspectorButtonAttribute attr)
    {
        //ラベルがない場合は関数名で上書き
        string buttonLabel = string.IsNullOrEmpty(attr.label) ? method.Name : attr.label;
        //引数を取得
        var parameters = method.GetParameters();

        EditorGUILayout.Space(4);

        if (parameters.Length == 0)
        {
            if (GUILayout.Button(buttonLabel))
                InvokeMethod(method, null);
        }
        else
        {
            //初回は辞書に登録することで次回以降の検索の手間を省く
            if (!methodParameters.ContainsKey(method.Name))
                methodParameters[method.Name] = new object[parameters.Length];

            var values = methodParameters[method.Name];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"{method.Name} Parameters", EditorStyles.boldLabel);

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                values[i] = DrawFieldForType(param, values[i]);
            }

            if (GUILayout.Button(buttonLabel))
                InvokeMethod(method, values);

            EditorGUILayout.EndVertical();
        }
    }

    private void InvokeMethod(MethodInfo method, object[] values)
    {
        try
        {
            method.Invoke(target, values);
        }
        catch (Exception e)
        {
            Debug.LogError($"[OnInspectorButton] {method.Name} failed: {e}");
        }
    }

    private object DrawFieldForType(ParameterInfo param, object currentValue)
    {
        Type t = param.ParameterType;
        string name = ObjectNames.NicifyVariableName(param.Name);

        if (t == typeof(int))
            return EditorGUILayout.IntField(name, currentValue != null ? (int)currentValue : 0);
        if (t == typeof(float))
            return EditorGUILayout.FloatField(name, currentValue != null ? (float)currentValue : 0f);
        if (t == typeof(string))
            return EditorGUILayout.TextField(name, currentValue as string ?? "");
        if (t == typeof(bool))
            return EditorGUILayout.Toggle(name, currentValue != null && (bool)currentValue);
        if (t == typeof(Vector2))
            return EditorGUILayout.Vector2Field(name, currentValue != null ? (Vector2)currentValue : Vector2.zero);
        if (t == typeof(Vector3))
            return EditorGUILayout.Vector3Field(name, currentValue != null ? (Vector3)currentValue : Vector3.zero);
        if (t == typeof(Color))
            return EditorGUILayout.ColorField(name, currentValue != null ? (Color)currentValue : Color.white);

        // Enum
        if (t.IsEnum)
        {
            if (currentValue == null)
                currentValue = Enum.GetValues(t).GetValue(0);
            return EditorGUILayout.EnumPopup(name, (Enum)currentValue);
        }

        // UnityEngine.Object
        if (typeof(UnityEngine.Object).IsAssignableFrom(t))
            return EditorGUILayout.ObjectField(name, currentValue as UnityEngine.Object, t, true);

        // 配列またはList
        if (typeof(IList).IsAssignableFrom(t))
        {
            EditorGUILayout.LabelField($"{name} ({t.Name}) : List/Array not supported in editor parameters");
            return currentValue;
        }

        EditorGUILayout.LabelField($"{name} ({t.Name}) : not supported");
        return currentValue;
    }

    /// <summary>
    /// ScriptableObjectのネストされたフィールドを再帰的に描画
    /// </summary>
    private void DrawNestedScriptableObjects(UnityEngine.Object obj, int depth = 0)
    {
        if (obj == null || depth > 3) return;
        //SOに入っている[SerialiFiled],publicを取得(インスペクターで描画可能なやつ)
        var so = new SerializedObject(obj);
        //[Serializable]の先頭
        var prop = so.GetIterator();

        bool expanded = false;

        //次に行けるかどうか
        while (prop.NextVisible(true))
        {
            //[SerializeReference]が有る場合は描画
            if (prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                UnityEngine.Object refObj = prop.objectReferenceValue;
                if (refObj is ScriptableObject nestedSO)
                {
                    EditorGUILayout.Space(3);
                    expanded = EditorGUILayout.Foldout(expanded, $"▶ {nestedSO.name} ({nestedSO.GetType().Name})", true);
                    if (expanded)
                    {
                        EditorGUI.indentLevel++;
                        DrawNestedScriptableObjects(nestedSO, depth + 1);
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }
    }
}
#endif
