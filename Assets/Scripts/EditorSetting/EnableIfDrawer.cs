using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;

//EnableAttributeが設定されたプロパティを表示するときに呼ばれる
[CustomPropertyDrawer(typeof(EnableIfAttribute))]
public class EnableIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnableIfAttribute enAttr = (EnableIfAttribute)attribute;
        
        bool enable = EvaluateConditions(property, enAttr);

        //隠す設定
        if (!enable && enAttr.hideWhenFalse)
        {
            //何もしない
            //非表示にする
            return;
        }
        //有効 or グレーアウトして表示させる
        bool prev = GUI.enabled;
        GUI.enabled = enable; //trueなら編集可能,falseなら編集できない
        //EditorGUIは、GUIの設定を元に、インスペクターを更新する。
        EditorGUI.PropertyField (position, property, label,true);
        //他のGUIが影響を受けないように戻す(このオーバーライドは、EnableIfAttirbuteが呼ばれた時に起こる)
        GUI.enabled = prev;
    }

    //無効にする場合、テキストフィールドの高さを0,つまり非表示にする。
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        EnableIfAttribute enAttr = (EnableIfAttribute)attribute;
        bool enable = EvaluateConditions(property, enAttr);

        //非表示なら、高さ0にする
        if (!enable && enAttr.hideWhenFalse)
        {
            return 0f;
        }
            
        return EditorGUI.GetPropertyHeight(property, label,true);
    }
    private bool EvaluateConditions(SerializedProperty property, EnableIfAttribute enAttr)
    {
        if (enAttr.conditionFieldNames == null || enAttr.conditionFieldNames.Length == 0)
            return true;

        bool[] results = new bool[enAttr.conditionFieldNames.Length];

        for (int i = 0; i < enAttr.conditionFieldNames.Length; i++)
        {
            string rawName = enAttr.conditionFieldNames[i];
            bool negate = false;

            // 否定プレフィックス '!' に対応
            if (rawName.StartsWith("!"))
            {
                negate = true;
                rawName = rawName.Substring(1); // 先頭の!を削除
            }
            //同じ階層(同一ファイル)にある名前検索
            string conditionPath = property.propertyPath.Replace(property.name, rawName);
            //名前の前にある属性(int,float, bool...)を取得
            SerializedProperty conditionProp = property.serializedObject.FindProperty(conditionPath);

            bool value = false;
            if (conditionProp != null && conditionProp.propertyType == SerializedPropertyType.Boolean)
                value = conditionProp.boolValue;

            // 否定演算を適用
            results[i] = negate ? !value : value;
        }

        // ---- 論理演算まとめ ----
        switch (enAttr.logic)
        {
            case ConditionLogic.AND:
                return Array.TrueForAll(results, r => r);

            case ConditionLogic.OR:
                return Array.Exists(results, r => r);

            case ConditionLogic.NOT:
                return !results[0];

            case ConditionLogic.NAND:
                return !Array.TrueForAll(results, r => r);

            case ConditionLogic.NOR:
                return !Array.Exists(results, r => r);

            case ConditionLogic.XOR:
                int count = 0;
                foreach (bool r in results)
                    if (r) count++;
                return (count % 2 == 1);

            default:
                return true;
        }
    }
}
