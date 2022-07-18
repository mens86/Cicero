using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[CreateAssetMenu(fileName = "GameEvents", menuName = "Quiz/new GameEvents")]
public class GameEvents : ScriptableObject
{

    public delegate void UpdateQuestionUICallback(Question question);
    public UpdateQuestionUICallback UpdateQuestionUI = null;

    public delegate void UpdateQuestionAnswerCallback(AnswerData pickedAnswer);
    public UpdateQuestionAnswerCallback updateQuestionAnswer = null;

    public delegate void DisplayResolutionScreenCallback(UIManager.ResolutionScreenType type, float score);
    public DisplayResolutionScreenCallback DisplayResolutionScreen = null;

    public delegate void ScoreUpdatedCallback();
    public ScoreUpdatedCallback ScoreUpdated = null;

    public const int maxlevel = 2;

    [HideInInspector]
    public float CurrentFinalScore = 0;
    [HideInInspector]
    public float StartupHighScore = 0;

}
