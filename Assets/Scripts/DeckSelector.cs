using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class DeckSelector : MonoBehaviour
{
    [SerializeField] float margins;
    [SerializeField] AnswerData deckSelectPrefab;
    [SerializeField] RectTransform decksContentArea;
    public RectTransform DecksContentArea { get { return decksContentArea; } }

    [SerializeField] GameEvents events = null;
    [SerializeField] GameManager GameManager = null;
    public List<AnswerData> PickedDecks = new List<AnswerData>();


    void OnEnable()
    {
        events.updateQuestionAnswer += UpdateAnswers;
    }

    void OnDisable()
    {
        events.updateQuestionAnswer -= UpdateAnswers;
    }


    void Start()
    {
        ShowDecks();
    }


    public void ShowDecks()
    {
        List<string> availableDecks = GetDecksFromFolder();
        float offset = 0 - margins;

        for (int i = 0; i < availableDecks.Count; i++)
        {

            AnswerData deck = (AnswerData)Instantiate(deckSelectPrefab, DecksContentArea);
            deck.infoTextObject.text = availableDecks[i];

            deck.Rect.anchoredPosition = new Vector2(0, offset);
            offset -= (deck.Rect.sizeDelta.y + margins);
            DecksContentArea.sizeDelta = new Vector2(DecksContentArea.sizeDelta.x, offset * -1);
        }
    }

    public List<string> GetDecksFromFolder()
    {
        List<string> csvDecks = new List<string>();

        DirectoryInfo di = new DirectoryInfo("Assets/Resources");
        FileInfo[] smFiles = di.GetFiles("*.csv");
        foreach (FileInfo fi in smFiles)
        {
            csvDecks.Add(Path.GetFileNameWithoutExtension(fi.Name));
        }
        return csvDecks;
    }


    public void StartGame()
    {


        List<string> ListOfDecks = new List<string>();
        for (int i = 0; i < PickedDecks.Count; i++)
        {
            ListOfDecks.Add(PickedDecks[i].infoTextObject.text);
        }
        GameManager.QuestionsFileNames = ListOfDecks;

        //attivare il gioco
    }


    public void UpdateAnswers(AnswerData newAnswer)
    {

        bool alreadyPicked = false;
        for (int i = 0; i < PickedDecks.Count; i++)
        {
            if (PickedDecks[i].infoTextObject.text.Contains(newAnswer.infoTextObject.text))
            {
                alreadyPicked = true;
            }
        }

        if (alreadyPicked)
        {
            int indexToRemove = -1;
            for (int i = 0; i < PickedDecks.Count; i++)
            {
                if (PickedDecks[i].infoTextObject.text.Contains(newAnswer.infoTextObject.text))
                {
                    indexToRemove = i;
                }
            }
            PickedDecks.RemoveAt(indexToRemove);
        }

        else
        {
            PickedDecks.Add(newAnswer);
        }

    }








}
