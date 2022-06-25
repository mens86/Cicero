using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class KeybManager : MonoBehaviour
{
    public static KeybManager Instance;
    [SerializeField] TMP_InputField textBox;
    //[SerializeField] TextMeshProUGUI printBox;

    private void Start()
    {
        Instance = this;
        //printBox.text = "";
        textBox.text = "";
    }

    public void DeleteLetter()
    {
        if (textBox.text.Length != 0)
        {
            textBox.text = textBox.text.Remove(textBox.text.Length - 1, 1);
        }
    }

    public void AddLetter(string letter)
    {
        textBox.text = textBox.text + letter;
    }

    public void SubmitWord()
    {
        //printBox.text = textBox.text;
        //textBox.text = "";
        // Debug.Log("Text submitted successfully!");



        List<string> res = GameObject.Find("InputField").GetComponent<Autocomplete>().GetResults(textBox.text);

        if (res.Count() != 0)
        {
            if (textBox.text != res[0])
            {
                textBox.text = res[0];
                textBox.ActivateInputField();
            }
            else
            {
                GameObject.Find("InputField").GetComponent<Autocomplete>().firstButton.GetComponent<Button>().onClick.Invoke();
                //inputField.text = "";
                textBox.ActivateInputField();

            }
        }

    }
}
