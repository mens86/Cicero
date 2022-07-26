using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BackSpaceHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Autocomplete autocomplete;
    bool _pressed = false;
    float timeButtonPressed = 0;
    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        timeButtonPressed = 0;
    }
    void Update()
    {
        if (!_pressed)
            return;

        timeButtonPressed += Time.deltaTime;
        if (timeButtonPressed > 0.5f)
        {
            autocomplete.inputField.text = "";

            _pressed = false;
        }
    }
}