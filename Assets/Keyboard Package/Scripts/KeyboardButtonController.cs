using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyboardButtonController : MonoBehaviour
{
    [SerializeField] Image containerBorderImage;
    [SerializeField] Image containerFillImage;
    [SerializeField] Image containerIcon;
    [SerializeField] TextMeshProUGUI containerText;
    [SerializeField] TextMeshProUGUI containerActionText;

    private void Start()
    {
        SetContainerBorderColor(ColorDataStore.GetKeyboardBorderColor());
        SetContainerFillColor(ColorDataStore.GetKeyboardFillColor());
        SetContainerTextColor(ColorDataStore.GetKeyboardTextColor());
        SetContainerActionTextColor(ColorDataStore.GetKeyboardActionTextColor());
    }

    public void SetContainerBorderColor(Color color) => containerBorderImage.color = color;
    public void SetContainerFillColor(Color color) => containerFillImage.color = color;
    public void SetContainerTextColor(Color color) => containerText.color = color;
    public void SetContainerActionTextColor(Color color)
    {
        containerActionText.color = color;
        containerIcon.color = color;
    }

    private IEnumerator waitHalfSec;
    public void AddLetter()
    {
        if (KeybManager.Instance != null)
        {
            KeybManager.Instance.AddLetter(containerText.text);


            //sta merda, corutine compresa, per cambiare colore al click, visto che non posso usare l'highlighted se voglio allargare i bottoni laterali.
            Color pressedButtonColor = new Color32(25, 77, 255, 255);
            containerFillImage.GetComponent<Image>().color = pressedButtonColor;
            waitHalfSec = PleasewaitHalfSec();
            StartCoroutine(waitHalfSec);

        }
        else
        {
            Debug.Log(containerText.text + " is pressed");
        }
    }

    IEnumerator PleasewaitHalfSec()
    {
        yield return new WaitForSeconds(0.1f);
        Color releasedButtonColor = new Color32(0, 0, 0, 0);
        containerFillImage.GetComponent<Image>().color = releasedButtonColor;
    }



    public void DeleteLetter()
    {
        if (KeybManager.Instance != null)
        {
            KeybManager.Instance.DeleteLetter();
        }
        else
        {
            Debug.Log("Last char deleted");
        }
    }
    public void SubmitWord()
    {
        if (KeybManager.Instance != null)
        {
            KeybManager.Instance.SubmitWord();
        }
        else
        {
            Debug.Log("Submitted successfully!");
        }
    }
}