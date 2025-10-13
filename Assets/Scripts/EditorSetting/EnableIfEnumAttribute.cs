using System;
using UnityEngine;

/// <summary>
/// �Ώۂ�Enum�ϐ��̒l�ɂ���āA�ҏW�\�E�s�\��؂�ւ��܂��B
/// <param name="enumFiledName"> enumFiledName:��r�Ώۂ�Enum�^�C�v�̕ϐ�</param>
/// <param name="hideWhenFalse"> hideWhenFalse:�B���ݒ�̎�</param>
/// <param name="enumValues"> �Ώۂ�Enum�l�z��,�����̏ꍇ�́A�����ꂩ1�I�΂ꂽ�ꍇ�ɕҏW�\�ɂȂ�܂�</param>
/// </summary>
[AttributeUsage(AttributeTargets.Field,AllowMultiple = false)]
public class EnableIfEnumAttribute : PropertyAttribute
{
    public string enumFiledName; //�����𔻒肷��t�B�[���h��
    public int[] enumValues;   //�L�����Ώۂ�enum�̒l
    public bool hideWhenFalse;

    public EnableIfEnumAttribute(string enumFiledName, bool hideWhenFalse = false , params object[] enumValues)
    {
        this.enumFiledName = enumFiledName;
        this.enumValues = new int[enumValues.Length];
        for (int i = 0; i < enumValues.Length; i++)
        {
            this.enumValues[i] = (int)enumValues[i];
        }

        this.hideWhenFalse = hideWhenFalse;
    }
    //public EnableIfEnumAttribute(string enumFiledName, params object[] enumValues)
    //{
    //    this.enumFiledName = enumFiledName;
    //    this.enumValues = new int[enumValues.Length];
    //    for (int i = 0; i < enumValues.Length; i++)
    //    {
    //        this.enumValues[i] = (int)enumValues[i];
    //    }
    //    this.hideWhenFalse = false;
    //}
}