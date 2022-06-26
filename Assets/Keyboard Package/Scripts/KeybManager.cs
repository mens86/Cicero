using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class KeybManager : MonoBehaviour
{
    public static KeybManager Instance;
    [SerializeField] TMP_InputField textBox;


    private void Start()
    {
        Instance = this;
        textBox.text = "";
    }

    public void DeleteLetter()
    {
        if (textBox.text.Length != 0)
        {
            GameObject.Find("DeletedLetter").GetComponent<Text>().text = textBox.text.Substring(textBox.text.Length - 1);
            textBox.text = textBox.text.Remove(textBox.text.Length - 1, 1);
        }
    }

    public void AddLetter(string letter)
    {
        textBox.text = textBox.text + letter;

        //sta merda, corutine compresa, sarebbe solo per avere il caret alla fine (se solo andasse su android). A
        //Anzi, stando così le cose, serve a far scorrere il testo oltre quando arrivi alla fine dell'inputfield
        textBox.ActivateInputField();
        StartCoroutine(MoveTextEnd_NextFrame());
    }

    IEnumerator MoveTextEnd_NextFrame()
    {
        yield return 0;
        textBox.MoveTextEnd(false);
    }



    public void SubmitWord()
    {
        List<string> res = GameObject.Find("InputField").GetComponent<Autocomplete>().GetResults(textBox.text);

        if (res.Count() != 0)
        {
            if (textBox.text != res[0])
            {
                textBox.text = res[0];
                textBox.ActivateInputField();
                GameObject.Find("InputField").GetComponent<Autocomplete>().inputField.ActivateInputField();
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
