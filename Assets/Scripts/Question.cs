using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum AnswerType { Multi, Single }


[Serializable]
public class Answer
{
    //public string Info = string.Empty;
    public List<string> correctAnswerGroup;

    public Answer() { }


    public bool IsCorrectAnswer(List<string> input)
    {
        return GetCorrectAnswer(input) != "";
    }

    public string GetCorrectAnswer(List<string> input)
    {
        foreach (var x in input)
        {

            int correctAnswerIndex = correctAnswerGroup.IndexOf(x);
            if (correctAnswerIndex != -1)
            {
                return correctAnswerGroup[correctAnswerIndex];
            }

        }
        return correctAnswerGroup[0];
    }


}

[Serializable()]
public class Question
{
    public String Info = null;
    public Answer[] Answers = null;
    public bool UseTimer = false;
    public int Timer = 0;
    public AnswerType Type = AnswerType.Single;
    public int AddScore = 0;

    public Question() { }



    public (UserAnswersScenario, float) CheckAnswers(List<string> pickedAnswers)
    {
        List<string> actualAnswers = new List<string>();
        //Qui, ogni volta che controlliamo una risposta, controlliamo anche i sinonimi
        foreach (var currAnswer in Answers)
        {
            string correctAnswer = currAnswer.GetCorrectAnswer(pickedAnswers);
            if (correctAnswer != "")
            {
                actualAnswers.Add(correctAnswer);
            }

        }

        /*
        Debug.Log("-----------------------");
        Debug.Log("Risposte giuste");
        foreach (var x in actualAnswers)
        {
            Debug.Log(x);
        }
        Debug.Log("-----------------------");

        Debug.Log("Risposte selezionate");
        foreach (var x in pickedAnswers)
        {
            Debug.Log(x);
        }
        Debug.Log("-----------------------");
        */


        float scoreMultiplier = 0;
        var qq = actualAnswers.Except(pickedAnswers).ToList();
        var pp = pickedAnswers.Except(actualAnswers).ToList();


        //meno delle giuste: 
        if (qq.Any() && qq.Count < actualAnswers.Count)
        {
            scoreMultiplier = actualAnswers.Count - qq.Count;
            return (UserAnswersScenario.LessThanCorrect, scoreMultiplier);
        }
        //più delle giuste: 
        if (!qq.Any() && pp.Any())
        {
            scoreMultiplier = (float)actualAnswers.Count / (float)pickedAnswers.Count;
            return (UserAnswersScenario.MoreThanCorrect, scoreMultiplier);
        }

        //tutte giuste
        if (!qq.Any() && !pp.Any())
        {
            scoreMultiplier = actualAnswers.Count;
            return (UserAnswersScenario.AllCorrect, scoreMultiplier);
        }

        //tutte sbagliate
        if (qq.Count == actualAnswers.Count)
        {
            scoreMultiplier = 0;
            return (UserAnswersScenario.AllWrong, scoreMultiplier);
        }

        scoreMultiplier = 0;
        return (UserAnswersScenario.AllWrong, scoreMultiplier);
    }

}
