using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnswerType { Multi, Single }

[Serializable()]
public class Answer
{
    public string Info = string.Empty;

    public Answer() { }

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

    public List<string> GetCorrectAnswers()
    {
        List<string> CorrectAnswers = new List<string>();
        for (int i = 0; i < Answers.Length; i++)
        {
            CorrectAnswers.Add(Answers[i].Info);
        }

        return CorrectAnswers;
    }



}
