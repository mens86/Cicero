using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.Networking;
using System.Linq;


public class DeckSelector : MonoBehaviour
{
    public List<TextAsset> availableDecks;

    [SerializeField] float margins;
    [SerializeField] AnswerData deckSelectPrefab;
    [SerializeField] RectTransform decksContentArea;
    public RectTransform DecksContentArea { get { return decksContentArea; } }
    [SerializeField] TextMeshProUGUI Title = null;

    [SerializeField] GameObject MenuCanvas = null;
    [SerializeField] GameObject GameCanvas = null;
    [SerializeField] GameObject Managers = null;
    [SerializeField] GameEvents events = null;

    public List<AnswerData> PickedDecks_answersData = new List<AnswerData>();



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
        float offset = 0 - margins;

        for (int i = 0; i < availableDecks.Count; i++)
        {

            AnswerData deck = (AnswerData)Instantiate(deckSelectPrefab, DecksContentArea);
            deck.infoTextObject.text = availableDecks[i].name;

            deck.Rect.anchoredPosition = new Vector2(0, offset);
            offset -= (deck.Rect.sizeDelta.y + margins);
            DecksContentArea.sizeDelta = new Vector2(DecksContentArea.sizeDelta.x, offset * -1);
        }
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
