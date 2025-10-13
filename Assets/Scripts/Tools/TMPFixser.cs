using System;
using System.Text;
//���{���InputField���g���ۂɂ������Ă���]���ȉ��s�Ƃ��A�S�p�Ƃ��𖳌�������
public static class TMPTextUtils
{
    public static string NormalizeText(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        return input
            .Normalize(NormalizationForm.FormKC)
            .Replace("\u200B", "")
            .Replace("\uFEFF", "")
            .Replace("\u00A0", "")
            .Replace("\r", "")
            .Replace("\n", "")
            .Trim();
    }

    public static int ParseInt(string input, int defaultValue = 0)
    {
        string normalized = NormalizeText(input);
        return int.TryParse(normalized, out int result) ? result : defaultValue;
    }
}