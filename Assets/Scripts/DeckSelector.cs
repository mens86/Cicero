using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;




public class DeckSelector : MonoBehaviour
{
    public List<TextAsset> availableDecks;
    public MemoryIndex memoryIndex;

    [SerializeField] float margins;
    [SerializeField] AnswerData deckSelectPrefab;
    [SerializeField] Titles CategoriesPrefab;
    [SerializeField] RectTransform decksContentArea;
    public RectTransform DecksContentArea { get { return decksContentArea; } }
    [SerializeField] TextMeshProUGUI Title = null;

    [SerializeField] GameObject MenuCanvas = null;
    [SerializeField] GameObject GameCanvas = null;
    [SerializeField] GameObject Managers = null;
    [SerializeField] GameEvents events = null;

    public List<AnswerData> PickedDecks_answersData = new List<AnswerData>();
    public List<string> listOfCategories = new List<string>();
    private char delimiter = ';';


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
        listOfCategories = PopulateCategories(availableDecks);
        ShowDecks();

    }

    public List<string> PopulateCategories(List<TextAsset> decks_textAssets)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < decks_textAssets.Count; i++)
        {
            TextAsset txt = decks_textAssets[i];
            string filecontent = txt.text;
            filecontent = filecontent.Replace("\r", "");

            string[] lines = filecontent.Split('\n');
            string[] cells = lines[0].Split(delimiter);
            if (!list.Contains(cells[1]))
            {
                list.Add(cells[1]);
            }
        }
        return list;
    }

    public void ShowDecks()
    {
        float offset = 0 - margins;


        for (int i = 0; i < listOfCategories.Count; i++)
        {

            Titles Title = (Titles)Instantiate(CategoriesPrefab, DecksContentArea);
            Title.Title.text = listOfCategories[i];

            Title.rectTransf.anchoredPosition = new Vector2(0, offset);
            offset -= (Title.rectTransf.sizeDelta.y + margins);
            DecksContentArea.sizeDelta = new Vector2(DecksContentArea.sizeDelta.x, offset * -1);

            for (int u = 0; u < availableDecks.Count; u++)
            {


                List<TextAsset> listForMethodBelow = new List<TextAsset>();
                listForMethodBelow.Add(availableDecks[u]);
                if (PopulateCategories(listForMethodBelow)[0] == Title.Title.text)
                {
                    AnswerData deck = (AnswerData)Instantiate(deckSelectPrefab, DecksContentArea);
                    deck.infoTextObject.text = availableDecks[u].name;
                    //deck.MasteryNumber.text = CalculateMasteryNumber(deck);

                    deck.Rect.anchoredPosition = new Vector2(0, offset);
                    offset -= (deck.Rect.sizeDelta.y + margins);
                    DecksContentArea.sizeDelta = new Vector2(DecksContentArea.sizeDelta.x, offset * -1);
                }


            }
        }

    }

    private string CalculateMasteryNumber(AnswerData deck)
    {
        List<Question> questionsOfDeck = memoryIndex.persistentQuestionList.Where(q => q.question_filename == deck.infoTextObject.text).ToList();
        int totalDeckKnowledge = 0;
        int currentDeckKnowledge = 0;

        foreach (var q in questionsOfDeck)
        {
            totalDeckKnowledge += q.Answers.Length * 3;
            currentDeckKnowledge += q.cardProprieties.cardKnowledge;
        }

        float percentageOfKnowledge = (currentDeckKnowledge * 100) / totalDeckKnowledge;

        return percentageOfKnowledge.ToString() + "%";
    }

    public void StartGame()
    {
        List<TextAsset> selectedDecks = new List<TextAsset>();

        foreach (AnswerData deckName in PickedDecks_answersData)
        {
            foreach (TextAsset deck in availableDecks)
            {
                if (deck.name == deckName.infoTextObject.text)
                {
                    selectedDecks.Add(deck);
                    break;  //Minor performance
                }
            }
        }

        Managers.GetComponent<GameManager>().SelectedDecks_names = selectedDecks;

        if (selectedDecks.Count != 0)
        {
            MenuCanvas.SetActive(false);
            GameCanvas.SetActive(true);
            Managers.SetActive(true);
        }
        else
        {
            Title.text = "I've told you to choose a study mix!";
        }

    }


    public void UpdateAnswers(AnswerData newAnswer)
    {

        bool alreadyPicked = false;
        for (int i = 0; i < PickedDecks_answersData.Count; i++)
        {
            if (PickedDecks_answersData[i].infoTextObject.text.Contains(newAnswer.infoTextObject.text))
            {
                alreadyPicked = true;
            }
        }

        if (alreadyPicked)
        {
            int indexToRemove = -1;
            for (int i = 0; i < PickedDecks_answersData.Count; i++)
            {
                if (PickedDecks_answersData[i].infoTextObject.text.Contains(newAnswer.infoTextObject.text))
                {
                    indexToRemove = i;
                }
            }
            PickedDecks_answersData.RemoveAt(indexToRemove);
        }

        else
        {
            PickedDecks_answersData.Add(newAnswer);
        }

    }

}
