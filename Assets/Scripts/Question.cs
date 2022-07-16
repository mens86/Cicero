using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum AnswerType { Multi, Single }


[Serializable]
public class Answer
{
    //all correct synonyms
    public List<string> groupOfSynonyms;

    public Answer() { }


    public bool IsCorrectAnswer(List<string> input)
    {
        return GetCorrectAnswer(input) != "";
    }

    public string GetCorrectAnswer(List<string> input)
    {
        foreach (var x in input)
        {
            int correctAnswerIndex = groupOfSynonyms.IndexOf(x);
            if (correctAnswerIndex != -1)
            {
                return groupOfSynonyms[correctAnswerIndex];
            }
        }
        return groupOfSynonyms[0];
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
    public CardProprieties cardProprieties;
    public string question_filename;

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

        float scoreMultiplier = 0;
        var qq = actualAnswers.Except(pickedAnswers).ToList();
        var pp = pickedAnswers.Except(actualAnswers).ToList();

        MemoryIndex memoryIndex = GameObject.Find("MemoryIndex").GetComponent<MemoryIndex>();
        //meno delle giuste: 
        if (qq.Any() && qq.Count < actualAnswers.Count)
        {
            scoreMultiplier = actualAnswers.Count - qq.Count;
            if (qq.Count == 1) //la discrimine per dire "eran quasi tutte giuste" è che se ne sbagli solo una (indipendentemente dal numero di risposte di una domanda)
            {
                memoryIndex.UpdateMemoryIndex(this, UserAnswerState.AlmostAllRight);
            }
            else
            {
                memoryIndex.UpdateMemoryIndex(this, UserAnswerState.AlmostAllWrong);
            }
            return (UserAnswersScenario.LessThanCorrect, scoreMultiplier);
        }
        //più delle giuste: 
        if (!qq.Any() && pp.Any())
        {
            scoreMultiplier = (float)actualAnswers.Count / (float)pickedAnswers.Count;
            if (pp.Count == 1) //la discrimine per dire "eran quasi tutte giuste" è che se ne sbagli solo una (indipendentemente dal numero di risposte di una domanda)
            {
                memoryIndex.UpdateMemoryIndex(this, UserAnswerState.AlmostAllRight);
            }
            else
            {
                memoryIndex.UpdateMemoryIndex(this, UserAnswerState.AlmostAllWrong);
            }
            return (UserAnswersScenario.MoreThanCorrect, scoreMultiplier);
        }

        //tutte giuste
        if (!qq.Any() && !pp.Any())
        {
            scoreMultiplier = actualAnswers.Count;
            memoryIndex.UpdateMemoryIndex(this, UserAnswerState.AllRight);
            return (UserAnswersScenario.AllCorrect, scoreMultiplier);

        }

        //tutte sbagliate
        if (qq.Count == actualAnswers.Count)
        {
            scoreMultiplier = 0;
            memoryIndex.UpdateMemoryIndex(this, UserAnswerState.AllWrong);
            return (UserAnswersScenario.AllWrong, scoreMultiplier);
        }

        scoreMultiplier = 0;
        return (UserAnswersScenario.AllWrong, scoreMultiplier);
    }

}

[Serializable]
public class CardProprieties
{
    public CardProprieties() { }

    public string cardState = "NewCard";
    public int cardKnowledge = 0; // questo numero serve a mostrare all'utente la % di conoscenza del mazzo
    public float cardEase = 2.5f; //moltiplicatore che allarga la scadenza delle carte facili e accorcia quella delle difficili
    public float cardCurrentInterval = 0.0f;
    public DateTime cardExpDate;

    public int cardCurrentLeechLevel = 0; //attuale numero di volte consecutive in cui è stata sbagliata una carta //questo devo pensare a come cazzo fare il consecutive
    public bool isLeech = false;
    public int leechTreshold = 6; //numero di volte consecutive in cui sbagliamo completamente una carta prima che diventi "sanguisuga"
    public void LeechCount(int leechLevel)
    {
        cardCurrentLeechLevel += leechLevel;
        cardCurrentLeechLevel = (cardCurrentLeechLevel < leechTreshold) ? cardCurrentLeechLevel : leechTreshold;
        cardCurrentLeechLevel = (cardCurrentLeechLevel > 0) ? cardCurrentLeechLevel : 0;
        isLeech = (cardCurrentLeechLevel == leechTreshold) ? true : false;


        if (isLeech) //what to do if is leech - 4 days interval
        {
            MemoryIndex memoryIndex = GameObject.Find("MemoryIndex").GetComponent<MemoryIndex>();
            memoryIndex.currentInterval = 345600;
        }
    }
}

