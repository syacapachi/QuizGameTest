using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class OnInspectorButtonEditor : Editor
{
    private Dictionary<string, object[]> methodParameters = new();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //メソッドを調査
        var targetType = target.GetType();
        var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<OnInspectorButtonAttribute>();
            if (attr == null)
                continue;

            // 実行中限定ならチェック
            if (attr.showOnlyInPlayMode && !Application.isPlaying)
                continue;

            DrawButtonForMethod(method, attr);
        }
    }

    private void DrawButtonForMethod(MethodInfo method, OnInspectorButtonAttribute attr)
    {
        //ラベルなしなら関数名を表示.
        string buttonLabel = string.IsNullOrEmpty(attr.label) ? method.Name : attr.label;
        //関数の引数を抽出.
        var parameters = method.GetParameters();

        EditorGUILayout.Space(4);

        if (parameters.Length == 0)
        {
            if (GUILayout.Button(buttonLabel))
                InvokeMethod(method, null);
        }
        else
        {
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
    //実行関数(関数に通知する、外部実行)
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

        // UnityEngine.Object系
        if (typeof(UnityEngine.Object).IsAssignableFrom(t))
            return EditorGUILayout.ObjectField(name, currentValue as UnityEngine.Object, t, true);

        EditorGUILayout.LabelField($"{name} ({t.Name}) : not supported");
        return currentValue;
    }
}
