using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class KeyboardButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{


    [SerializeField] Image containerBorderImage;
    [SerializeField] Image containerFillImage;
    [SerializeField] Image containerIcon;
    [SerializeField] TextMeshProUGUI containerText;
    [SerializeField] TextMeshProUGUI containerActionText;
    public Autocomplete autocomplete;
    public CustomCaret customCaret;
    public Text deletedLetter;

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

        }
        else
        {
            Debug.Log(containerText.text + " is pressed");
        }
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


    bool _pressed = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        Color pressedButtonColor = new Color32(12, 209, 69, 255);
        containerFillImage.GetComponent<Image>().color = pressedButtonColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        Color releasedButtonColor = new Color32(0, 0, 0, 0);
        containerFillImage.GetComponent<Image>().color = releasedButtonColor;

    }
    void Update()
    {
        if (!_pressed)
            return;

    }

    public void BackspaceButton()
    {


        if (autocomplete.autoCompleteText.text == "")
        {
            DeleteLetter();
            customCaret.backspace(deletedLetter);
        }
        else
        {
            autocomplete.autoCompleteText.text = "";
        }

    }


}