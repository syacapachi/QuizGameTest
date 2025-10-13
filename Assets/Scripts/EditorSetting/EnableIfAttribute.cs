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
/// <param name = "conditionFieldNames"> :bool�ϐ��z��,���O�̐擪��!������Ɣے�Ɉ����ɂȂ�܂�</param>
/// <param name="hideWhenFalse">:bool�z��̌��ʂ��U�̎��A�C���X�y�N�^�[��ɕ\�����邩�A�D�F(�ҏW�s�\�ɂ��邩) </param>
/// <param name="logic"><see cref="ConditionLogic">�̒l�𗘗p�Abool�ϐ��z��ɗ��p���鉉�Z�@,(Not�́A�擪�v�f�̂ݔے�)</param>
/// </summary>
[AttributeUsage(AttributeTargets.Field,Inherited = true,AllowMultiple = false)]
public class EnableIfAttribute : PropertyAttribute
{
    /// <summary>
    /// ���O�̐擪��!�������ꍇ�ے�ɂȂ�
    /// </summary>
    public string[] conditionFieldNames;
    public bool hideWhenFalse;
    public ConditionLogic logic;

    //�����ɖ��O������
    public EnableIfAttribute(string conditionFieldName, bool hideWhenFalse = false)
    {
        this.conditionFieldNames = new[] { conditionFieldName };
        this.hideWhenFalse = hideWhenFalse;
        this.logic = ConditionLogic.AND;
    }

    // ���������p�R���X�g���N�^
    public EnableIfAttribute(string[] conditionFieldNames, ConditionLogic logic = ConditionLogic.AND, bool hideWhenFalse = false)
    {
        this.conditionFieldNames = conditionFieldNames;
        this.hideWhenFalse = hideWhenFalse;
        this.logic = logic;
    }
}
