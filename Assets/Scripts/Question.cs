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

[Serializable]
public class CardProprieties
{
    public string cardState = "NewCard"; //attuale numero di volte consecutive in cui è stata sbagliata una carta //questo devo pensare a come cazzo fare il consecutive
    public int cardKnowledge = 0; // questo numero serve a mostrare all'utente la % di conoscenza del mazzo
    public int cardExpDate = 999;
    public int cardCurrentLeechLevel = 0;
    public bool isLeech = false;
    /*
        public CardProprieties(string newstate, int newknowledge, int newexpDate, int newcurrentLeechLevel, bool newisLeech)
        {
            state = newstate;
            knowledge = newknowledge;
            expDate = newexpDate;
            currentLeechLevel = newcurrentLeechLevel;
            isLeech = newisLeech;

        }
    */

    //Nell'awake del gamemanager, assieme alle question della sessione, viene caricata la lista di CardProprieties di tutte le carte fin qui viste e modificate nelle loro 
    //proprietà (potenzialmente tutte le carte di tutti i mazzi, nel lungo periodo). In memoryindex si aggiunge o modifica roba a questa lista di CardProprieties.
    //Finita la sessione si salva la lista




    //il problema è che questa lista è sempre diversa, perché dipende dai mazzi che scelgo all'inizio, quindi non la posso salvare. A meno che non salvi e carichi le liste di tutti i mazzi
    //
    //qui sovrascrivo ogni volta le carte 
    //
    //Alla fine della sessione salvo la lista di cardproprieties
}

