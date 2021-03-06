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
    [SerializeField] DeckCategories CategoriesPrefab;
    [SerializeField] RectTransform decksContentArea;
    public RectTransform DecksContentArea { get { return decksContentArea; } }
    [SerializeField] TextMeshProUGUI CategoryName = null;

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

        ShowCategories();
        LoadPreferences();
    }



    public void ShowCategories()
    {
        listOfCategories = PopulateCategories(availableDecks);
        for (int i = 0; i < listOfCategories.Count; i++)
        {
            DeckCategories Category = (DeckCategories)Instantiate(CategoriesPrefab, DecksContentArea);
            Category.CategoryName.text = listOfCategories[i];
            Category.name = Category.CategoryName.text + "ThisIsACategory";
            ShowDecks(Category);
            AssignMedal(Category);

        }
    }


    public void AssignMedal(DeckCategories Category)
    {
        if (Category.categoryMasteryNumber < 50)
        {
            Category.medalSpot.GetComponent<Image>().color = new Color(0, 0, 0, 0);

        }
        if (Category.categoryMasteryNumber >= 50 && Category.categoryMasteryNumber < 75)
        {
            Category.medalSpot.sprite = Category.bronzeMedal;
        }
        if (Category.categoryMasteryNumber >= 75 && Category.categoryMasteryNumber < 90)
        {
            Category.medalSpot.sprite = Category.silverMedal;
        }
        if (Category.categoryMasteryNumber >= 90)
        {
            Category.medalSpot.sprite = Category.goldMedal;
        }
    }

    public List<string> PopulateCategories(List<TextAsset> decks_textAssets) //
    {
        List<string> list = new List<string>();
        for (int i = 0; i < decks_textAssets.Count; i++)
        {
            TextAsset txt = decks_textAssets[i];
            if (!list.Contains(CategoryParser(txt)))
            {
                list.Add(CategoryParser(txt));
            }
        }
        return list;
    }


    public string CategoryParser(TextAsset txt)
    {
        string filecontent = txt.text;
        filecontent = filecontent.Replace("\r", "");

        string[] lines = filecontent.Split('\n');
        string[] cells = lines[0].Split(delimiter);
        return cells[1];
    }

    public void ShowDecks(DeckCategories Category)
    {
        int count = 0;
        float sumOfdeckMasteries = 0;

        for (int u = 0; u < availableDecks.Count; u++)
        {

            if (CategoryParser(availableDecks[u]) == Category.CategoryName.text)
            {
                AnswerData deck = (AnswerData)Instantiate(deckSelectPrefab, DecksContentArea);
                deck.infoTextObject.text = availableDecks[u].name;
                deck.MasteryNumber.text = CalculateDeckMasteryNumber(deck);
                deck.name = Category.CategoryName.text;

                //category mastery
                count += 1;
                sumOfdeckMasteries += float.Parse(deck.MasteryNumber.text.Substring(0, deck.MasteryNumber.text.Length - 1));
            }
        }

        Category.categoryMasteryNumber = (sumOfdeckMasteries * 100) / (100 * count);

    }

    public void HideAndShowDecksButton(DeckCategories Category)
    {
        Category.HideAndShowUPdateUI();
        if (Category.expanded == true)
        {
            Category.expanded = false;
            foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
            {

                if (gameObj.name == Category.CategoryName.text)
                {
                    gameObj.transform.localScale = new Vector3(0, 0, 0);
                    gameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(gameObj.GetComponent<RectTransform>().sizeDelta.x, 0);

                }
            }
        }
        else
        {
            Category.expanded = true;
            foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
            {

                if (gameObj.name == Category.CategoryName.text)
                {
                    gameObj.transform.localScale = new Vector3(1, 1, 1);
                    gameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(gameObj.GetComponent<RectTransform>().sizeDelta.x, deckSelectPrefab.Rect.sizeDelta.y);
                }

            }
        }
    }



    public void SelectAllButton(DeckCategories Category)
    {
        Category.SelectAllSwitchState();
        //bool atLeastOneWasUnchecked = false;
        foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            if (gameObj.name == Category.CategoryName.text)
            {
                if (!gameObj.GetComponent<AnswerData>().Checked)
                {
                    //atLeastOneWasUnchecked = true;
                    gameObj.GetComponent<AnswerData>().SetStateToChecked();
                }
            }
        }

        if (!atLeastOneWasUnchecked(Category))
        {
            if (!Category.SelectAllChecked)
            {
                foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
                {
                    if (gameObj.name == Category.CategoryName.text)
                    {
                        gameObj.GetComponent<AnswerData>().SetStateToUnchecked();
                    }
                }
            }
        }
    }

    public bool atLeastOneWasUnchecked(DeckCategories Category)
    {
        foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            if (gameObj.name == Category.CategoryName.text)
            {
                if (!gameObj.GetComponent<AnswerData>().Checked)
                {
                    return true;
                }
            }
        }
        return false;
    }



    private string CalculateDeckMasteryNumber(AnswerData deck)
    {
        List<Question> questionsOfDeck = memoryIndex.persistentQuestionList.Where(q => q.question_filename == deck.infoTextObject.text).ToList();
        int totalDeckKnowledge = 0;
        int currentDeckKnowledge = 0;

        foreach (var q in questionsOfDeck)
        {
            totalDeckKnowledge += q.Answers.Length * 3;
            currentDeckKnowledge += q.cardProperties.cardKnowledge;
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
            SavePreferences();
            MenuCanvas.SetActive(false);
            GameCanvas.SetActive(true);
            Managers.SetActive(true);
        }
        else
        {
            CategoryName.text = "delige fasciculos, inquam!";
        }
    }

    public void SavePreferences()

    {
        Dictionary<string, string> preferences = new Dictionary<string, string>();

        foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            if (gameObj.name.Contains("ThisIsACategory"))
            {
                string toAdd = "";
                if (gameObj.GetComponent<DeckCategories>().SelectAllChecked)
                {
                    toAdd += "Y";
                }
                else
                {
                    toAdd += "N";
                }
                if (gameObj.GetComponent<DeckCategories>().expanded)
                {
                    toAdd += "Y";
                }
                else
                {
                    toAdd += "N";
                }
                preferences.Add(gameObj.name, toAdd);
            }
            if (listOfCategories.Contains(gameObj.name))
            {
                if (gameObj.GetComponent<AnswerData>().Checked)
                {
                    preferences.Add(gameObj.GetComponent<AnswerData>().infoTextObject.text, "Y");
                }
                else
                {
                    preferences.Add(gameObj.GetComponent<AnswerData>().infoTextObject.text, "N");
                }
            }
        }
        //Debug.Log("saving selected decks");
        string stringedDict = string.Join(",", preferences.Select(m => m.Key + ":" + m.Value).ToArray());
        PlayerPrefs.SetString("Deckpref", stringedDict);

    }

    public void LoadPreferences()
    {
        Dictionary<string, string> preferences = new Dictionary<string, string>();
        string stringedDict = PlayerPrefs.GetString("Deckpref");
        string[] dictElements = stringedDict.Split(',');
        foreach (var e in dictElements)
        {
            string[] keyAndValue = e.Split(':');
            preferences.Add(keyAndValue[0], keyAndValue[1]);
        }

        foreach (var e in preferences)
        {
            foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
            {
                if (gameObj.name.Contains("ThisIsACategory"))
                {
                    if (e.Key == gameObj.name)
                    {
                        if (e.Value[0].ToString() == "Y")
                        {
                            gameObj.GetComponent<DeckCategories>().SelectAllChecked = true;
                            gameObj.GetComponent<DeckCategories>().SelectAllUPdateUI();
                        }
                        if (e.Value[1].ToString() == "N")
                        {
                            HideAndShowDecksButton(gameObj.GetComponent<DeckCategories>());
                        }
                        else
                        {
                            gameObj.GetComponent<DeckCategories>().expanded = false;
                            gameObj.GetComponent<DeckCategories>().HideAndShowUPdateUI();
                            gameObj.GetComponent<DeckCategories>().expanded = true; //non so perch??, non ho voglia di capire perch??, ma funziona
                        }
                    }
                }
                if (listOfCategories.Contains(gameObj.name))
                {
                    if (e.Key == gameObj.GetComponent<AnswerData>().infoTextObject.text)
                    {
                        if (e.Value == "Y")
                        {
                            gameObj.GetComponent<AnswerData>().SetStateToChecked();
                        }
                    }
                }
            }
        }
    }





    public void UpdateAnswers(AnswerData newAnswer)
    {


        bool alreadyPicked = false;
        for (int i = 0; i < PickedDecks_answersData.Count; i++)
        {
            if (PickedDecks_answersData[i].infoTextObject.text == newAnswer.infoTextObject.text)
            {
                alreadyPicked = true;
            }
        }

        if (alreadyPicked)
        {
            int indexToRemove = -1;
            for (int i = 0; i < PickedDecks_answersData.Count; i++)
            {
                if (PickedDecks_answersData[i].infoTextObject.text == newAnswer.infoTextObject.text)
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


        //set category to "all picked" if you have picked all decks; "unpicked" otherwise
        foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
        {
            if (gameObj.name.Contains("ThisIsACategory"))
            {
                if (!atLeastOneWasUnchecked(gameObj.GetComponent<DeckCategories>()))
                {
                    gameObj.GetComponent<DeckCategories>().SelectAllChecked = true;
                    gameObj.GetComponent<DeckCategories>().SelectAllUPdateUI();
                }
                else
                {
                    gameObj.GetComponent<DeckCategories>().SelectAllChecked = false;
                    gameObj.GetComponent<DeckCategories>().SelectAllUPdateUI();
                }
            }
        }

    }



}




