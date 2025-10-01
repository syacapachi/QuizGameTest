using TMPro;
using UnityEngine;

public class AnswerButtonAccesser : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI buttonTag;
    [SerializeField] TextMeshProUGUI buttonQuestion;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetTag(string tag)
    { 
        buttonTag.text = tag;
    }
    public void UpdateQuestion(string question)
    {
        buttonQuestion.text = question;
    }
}
