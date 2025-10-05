using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ImageQuizDataSO")]
[Serializable]
public class ImageQuizDataSO : QuizDataSO
{
    public Sprite questionImage;
}