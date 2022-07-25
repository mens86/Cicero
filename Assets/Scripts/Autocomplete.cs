using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;

public class Autocomplete : MonoBehaviour
{
    public TMP_InputField inputField;
    public RectTransform resultsParent;
    public RectTransform prefab;
    public List<string> allAnswers;
    public GameManager gameManager;
    public TextMeshProUGUI inputFieldText;
    public TextMeshProUGUI autoCompleteText;


    public RectTransform firstButton;
    public string currentTextInInputField = "";
    public string oldtext = "";





    private void Awake()
    {
        inputField.ActivateInputField();
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }


    private void OnInputValueChanged(string newText)
    {
        if (inputField.text == "")
        {
            ClearResults();
        }
        else
        {
            ClearResults();
            FillResults(GetResults(newText));
        }


        //text hightlight    
        if (GetResults(newText).Count != 0 && newText.Length > oldtext.Length)
        {
            currentTextInInputField = GetResults(newText)[0];

            if (currentTextInInputField != "" && inputField.text != "")
            {
                autoCompleteText.text = "";
                for (int i = inputField.text.Length; i < currentTextInInputField.Length; i++)
                {
                    autoCompleteText.text += "<mark =#000000AA>" + currentTextInInputField[i] + "</mark>";
                }
            }
            else
            {
                autoCompleteText.text = "";
            }
        }
        else
        {
            autoCompleteText.text = "";
        }
        oldtext = newText;
    }






    private void ClearResults()
    {
        // Reverse loop since you destroy children
        for (int childIndex = resultsParent.childCount - 1; childIndex >= 0; --childIndex)
        {
            Transform child = resultsParent.GetChild(childIndex);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    private void FillResults(List<string> results)
    {
        float margins = 5;
        float offset = 0 - margins;
        int resultToDisplay = 30; //decidi quanti risultati vuoi mettere nell'autocomplete. Non può essere tutti perché poi con liste grosse va lento
        if (results.Count < resultToDisplay)
        {
            resultToDisplay = results.Count;
        }



        for (int resultIndex = 0; resultIndex < resultToDisplay; resultIndex++)
        {
            RectTransform child = Instantiate(prefab, resultsParent) as RectTransform;
            child.GetComponentInChildren<TextMeshProUGUI>().text = results[resultIndex];


            child.anchoredPosition = new Vector2(0, offset);
            offset -= (child.sizeDelta.y + margins);
            resultsParent.sizeDelta = new Vector2(resultsParent.sizeDelta.x, offset * -1);

            child.SetParent(resultsParent);

        }

    }


    public List<string> GetResults(string input)
    {


        //List<string> results = allAnswers.FindAll((str) => str.IndexOf(input.ToLower()) >= 0);
        //per fare ricerca solo con inizio parola
        List<string> results = allAnswers.Where(str => str.Length >= input.Length).Where(str => str.Substring(0, input.Length).IndexOf(input) >= 0).ToList();



        List<string> answersToSubtract = new List<string>();
        foreach (var answer in gameManager.PickedAnswers)
        {
            answersToSubtract.Add(answer.infoTextObject.text);
        }
        results.Sort();
        return results.Except(answersToSubtract).ToList();



    }

}