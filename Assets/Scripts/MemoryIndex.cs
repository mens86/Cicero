using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public enum UserAnswerState { AllWrong, AlmostAllWrong, AlmostAllRight, AllRight }

public class MemoryIndex : MonoBehaviour
{
    private string QUESTIONS_FILEPATH => Application.persistentDataPath + "/questions.bin";

    public List<Question> persistentQuestionList;
    public List<TextAsset> QuestionsFileNames;


    public float currentInterval; // quanto tempo massimo passerà prima che la carta debba per forza ripresentarsi  (sommato all' "in questo momento" dà la data di scadenza)
    public float intervalModifier = 1.0f; //se è minore di 1 riduce tutti i tempi di attesa prima che la carta debba ripresentarsi

    //Learning/New State currentinterval modifiers
    public int learningStep0 = 60; //primo step in secondi [il tizio preferisce 25 minuti]
    public int learningStep1 = 300; //secondo step in secondi (5 minuti). Sarebbe (learningStep0 + LearningStep2) / 2; //[standard è quindi circa 5 min - il tizio preferisce 12 ore]
    public int LearningStep2 = 600; //secondo step in secondi (10 minuti). [il tizio preferisce 1 giorno prima di dire che ha imparato qualcosa – ci sta. È anche perché trova ingiusto perdere l’ease, dopo un giorno]

    //graduated State currentinterval modifiers
    public int graduatingInterval = 86400; //un giorno in secondi [il tizio preferisce 3 giorni]
    public int easyInterval = 345600; //quattro giorni in secondi
    public float ease = 2.5f; //moltiplicatore che allarga la scadenza delle carte facili e accorcia quella delle difficili
    public float minimumEase = 1.3f; //la presenza dell'ease minimo evita la ripetizione eccessiva delle parole difficili
    public float easyBonus = 1.5f; //bonus che distanzia maggiormente le facili

    //relearning State currentinterval modifiers
    public int relearningStep = 600; //relearningStep in secondi (10 minuti)
    public float newInterval = 0.0f; //settato a 0 di default serve a far ricominciare una carta se finisce in relearning [il tizio preferisce 0.20, per non far ricominciare una carta da 0 se è per sbaglio finita in relearning. Ha senso, ma forse allora potremmo togliere un po' di interval, se in relearning si fa male]
    public int minimumInterval = 86400; //un giorno in secondi - se new interval*currentinterval fa meno di un giorno, setti a un giorno. Se metti la cosa della riga sopra forse non serve.

    //Leeches
    public int leechTreshold = 4; //numero di volte consecutive in cui sbagliamo completamente una carta prima che diventi "sanguisuga"
    public bool isLeech = false; //stabilisce se una carta è sanguisuga sulla base dell'identità tra CurrentLeechInt e il LeechTreshold [cosa farsi se isLeexh è true, deciderò]  

    void Awake()
    {
        persistentQuestionList = Load();
        if (persistentQuestionList == null)
        {
            persistentQuestionList = new List<Question>();
            persistentQuestionList = FindObjectOfType<ParserQuestions_csv>().ParseQuestionsFile(QuestionsFileNames);
        }
        else
        {
            Debug.Log("Loaded questions " + persistentQuestionList.Count);
        }

    }
    void OnDestroy()
    {
        Save(persistentQuestionList);
    }

    private void Save(List<Question> persistentQuestionList)
    {
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(QUESTIONS_FILEPATH, FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, persistentQuestionList);
        stream.Close();
        Debug.Log("saved file to " + QUESTIONS_FILEPATH);
    }

    private List<Question> Load()
    {
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(QUESTIONS_FILEPATH, FileMode.Open, FileAccess.Read, FileShare.Read);
        List<Question> obj = (List<Question>)formatter.Deserialize(stream);
        stream.Close();
        return obj;
    }


    public void UpdateMemoryIndex(string question, CardProprieties cardProprieties, UserAnswerState WhatUserAnswered)
    {
        string cardState = question + "State";
        string cardExpiringDate = question + "ExpiringDate";
        //string questionCurrentLeech = question + "CurrentLeech"; 
        string cardKnowledge = question + "CardKnowledge";

        string currentCardState = PlayerPrefs.GetString(cardState);

        if (currentCardState == null) // = NewCard
        {
            switch (WhatUserAnswered)
            {
                case UserAnswerState.AllWrong:
                    currentInterval = learningStep0;
                    cardProprieties.cardState = "LearningCard";

                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    break;
                case UserAnswerState.AlmostAllWrong:
                    currentInterval = learningStep1;
                    PlayerPrefs.SetString(cardState, "LearningCard");
                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    break;
                case UserAnswerState.AlmostAllRight:
                    currentInterval = LearningStep2;
                    PlayerPrefs.SetString(cardState, "LearningCard");
                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    break;
                case UserAnswerState.AllRight:
                    currentInterval = easyInterval;
                    PlayerPrefs.SetString(cardState, "GraduatedCard");
                    PlayerPrefs.SetInt(cardKnowledge, 3);
                    break;
            }

        }
        else if (currentCardState == "LearningCard")
        {
            switch (WhatUserAnswered)
            {
                case UserAnswerState.AllWrong:
                    currentInterval = learningStep0;
                    PlayerPrefs.SetString(cardState, "LearningCard");
                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    break;
                case UserAnswerState.AlmostAllWrong:
                    currentInterval = learningStep1;
                    PlayerPrefs.SetString(cardState, "LearningCard");
                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    break;
                case UserAnswerState.AlmostAllRight:
                    currentInterval = graduatingInterval;
                    PlayerPrefs.SetString(cardState, "GraduatedCard");
                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    break;
                case UserAnswerState.AllRight:
                    currentInterval = easyInterval;
                    PlayerPrefs.SetString(cardState, "GraduatedCard");
                    PlayerPrefs.SetInt(cardKnowledge, 3);
                    break;
            }
        }
        else if (currentCardState == "GraduatedCard")
        {
            switch (WhatUserAnswered)
            {
                case UserAnswerState.AllWrong:
                    currentInterval = relearningStep;
                    PlayerPrefs.SetString(cardState, "RelearningCard");
                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    ease = ease > minimumEase ? ease -= 0.20f : minimumEase; //ease -20%
                    break;
                case UserAnswerState.AlmostAllWrong:
                    currentInterval = 1.2f * currentInterval * intervalModifier;
                    PlayerPrefs.SetString(cardState, "RelearningCard");
                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    ease = ease > minimumEase ? ease -= 0.15f : minimumEase; //ease -15%
                    break;
                case UserAnswerState.AlmostAllRight:
                    currentInterval = ease * currentInterval * intervalModifier;
                    PlayerPrefs.SetInt(cardKnowledge, 2);
                    break;
                case UserAnswerState.AllRight:
                    currentInterval = ease * currentInterval * easyBonus;
                    ease = ease > minimumEase ? ease += 0.15f : minimumEase; //ease +15%
                    PlayerPrefs.SetInt(cardKnowledge, 3);
                    break;
            }
        }
        else if (currentCardState == "RelearningCard")
        {
            switch (WhatUserAnswered)
            {
                case UserAnswerState.AllWrong:
                    currentInterval = relearningStep;
                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    break;
                case UserAnswerState.AlmostAllWrong:
                    currentInterval = 1.5f * relearningStep;
                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    break;
                case UserAnswerState.AlmostAllRight:
                    currentInterval = newInterval * currentInterval;
                    PlayerPrefs.SetString(cardState, "GraduatedCard");
                    PlayerPrefs.SetInt(cardKnowledge, 1);
                    break;
                case UserAnswerState.AllRight:
                    currentInterval = easyInterval;
                    PlayerPrefs.SetString(cardState, "GraduatedCard");
                    PlayerPrefs.SetInt(cardKnowledge, 2);

                    break;
            }
        }

        SetExpiringDate();

    }

    void SetExpiringDate()
    {

    }
    /*


        QUI DEVO ANCORA FARE IL SETEXPIRINGDATE e IL CURRENTLEECH 

        SORTING DA FARE SU GAMEMANAGER - L’algoritmo deve chiedere prima quelle che hanno il tempo scaduto/ (in modo random), poi andare in ordine sulla base di quanto tempo manca.
        Poi su gamemanager Awake (o su start? Comunque all'inizio di ogni sessione da 10 carte), dopo aver preso la lista di questions, faccio un forloop sulle questions e le uso come key per accedere alla "data di scadenza", 
        creo un tipo che contiene stringa e int e mi faccio una lista di questo tipo, dopodiché lo sorto con l'int, come dice questo video e con un altro for loop creo la nuova lista solo di string che mi dà le question sortate.
        https://learn.unity.com/tutorial/lists-and-dictionaries/?tab=overview#


        DA TENERE PRESENTE A LIVELLO DI DESIGN
        Poiché per me "newCard" è solo uno stato di studio (), a differenza che in anki, farò sottomazzi piccolini studiabili a gruppi, stile brainscape, che è la cosa migliore

        SI PUO' immaginare una mastery che decade nel tempo? Così si è portati a riprenderre mazzi portati al 100%


    */

}

