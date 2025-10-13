using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnableIfEnumAttribute))]
public class EnableIfEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnableIfEnumAttribute condition = (EnableIfEnumAttribute)attribute;

        //現在のプロパティ名
        string conditionPath = property.propertyPath.Replace(property.name, condition.enumFiledName);
        SerializedProperty enumProp = property.serializedObject.FindProperty(conditionPath);

        //ない場合デフォルト表示
        if (enumProp == null)
        {
            //表示関数,boolは、子供も表示するかどうか
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        bool enabled = false;
        if (enumProp.propertyType == SerializedPropertyType.Enum)
        {
            int currentValue = enumProp.enumValueIndex;//enumの現在地を取得
            foreach(int valid in condition.enumValues){
                if (currentValue == valid)
                {
                    enabled = true;//現在地と指定地が同じ時のみ、真に更新
                    break;
                }
            }
        }
        //隠す設定
        if (!enabled && condition.hideWhenFalse)
        {
            //何も表示しない
            return;
        }

        //条件に合わない場合はグレー
        bool prev = GUI.enabled;
        GUI.enabled = enabled;
        EditorGUI.PropertyField (position, property, label, true);
        GUI.enabled = prev;
    }
    //無効にする場合、テキストフィールドの高さを0,つまり非表示にする。
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        EnableIfEnumAttribute condition = (EnableIfEnumAttribute)attribute;
        string conditionPath = property.propertyPath.Replace(property.name, condition.enumFiledName);
        SerializedProperty enumProp = property.serializedObject.FindProperty(conditionPath);

        //非表示なら、高さ0にする
        if (enumProp != null && enumProp.propertyType == SerializedPropertyType.Enum)
        {
            bool enabled = false;
            int currentValue = enumProp.enumValueIndex;//enumの現在地を取得
            foreach (int valid in condition.enumValues)
            {
                if (currentValue == valid)
                {
                    enabled = true;//現在地と指定地が同じ時のみ、真に更新
                    break;
                }
            }
            if (!enabled && condition.hideWhenFalse)
            {
                return 0f;
            }

        }
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
