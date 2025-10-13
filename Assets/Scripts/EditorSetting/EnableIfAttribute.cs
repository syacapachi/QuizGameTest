using System;
using UnityEngine;

public enum ConditionLogic
{
    AND,
    OR,
    NOT,
    NAND,
    NOR,
    XOR
}
/// <summary>
/// <param name = "conditionFieldNames"> :bool変数配列,名前の先頭に!をつけると否定に扱いになります</param>
/// <param name="hideWhenFalse">:bool配列の結果が偽の時、インスペクター上に表示するか、灰色(編集不能にするか) </param>
/// <param name="logic"><see cref="ConditionLogic">の値を利用、bool変数配列に利用する演算法,(Notは、先頭要素のみ否定)</param>
/// </summary>
[AttributeUsage(AttributeTargets.Field,Inherited = true,AllowMultiple = false)]
public class EnableIfAttribute : PropertyAttribute
{
    /// <summary>
    /// 名前の先頭に!をつけた場合否定になる
    /// </summary>
    public string[] conditionFieldNames;
    public bool hideWhenFalse;
    public ConditionLogic logic;

    //ここに名前を入れる
    public EnableIfAttribute(string conditionFieldName, bool hideWhenFalse = false)
    {
        this.conditionFieldNames = new[] { conditionFieldName };
        this.hideWhenFalse = hideWhenFalse;
        this.logic = ConditionLogic.AND;
    }

    // 複数条件用コンストラクタ
    public EnableIfAttribute(string[] conditionFieldNames, ConditionLogic logic = ConditionLogic.AND, bool hideWhenFalse = false)
    {
        this.conditionFieldNames = conditionFieldNames;
        this.hideWhenFalse = hideWhenFalse;
        this.logic = logic;
    }
}
