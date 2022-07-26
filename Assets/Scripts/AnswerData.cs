using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerData : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] public TextMeshProUGUI infoTextObject;
    [SerializeField] public Image toggle;
    [SerializeField] public TextMeshProUGUI MasteryNumber;



    [Header("Textures")]
    [SerializeField] Sprite uncheckedToggle;
    [SerializeField] Sprite checkedToggle;

    [Header("References")]
    [SerializeField] GameEvents events;

    private RectTransform _rect;
    public RectTransform Rect
    {
        get
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            }
            return _rect;
        }
    }




    private int _answerIndex = -1;
    public int AnswerIndex { get { return _answerIndex; } }

    public bool Checked = false;



    public void UpdateData(string info, int index)
    {
        infoTextObject.text = info;
        _answerIndex = index;
    }

    public void Reset()
    {
        Checked = false;
        UPdateUI();
    }

    public void SwitchState()
    {
        Checked = !Checked;
        UPdateUI();


        if (events.updateQuestionAnswer != null)
        {
            events.updateQuestionAnswer(this);
        }

    }

    public void SetStateToChecked()
    {
        Checked = true;
        toggle.sprite = checkedToggle;

        if (events.updateQuestionAnswer != null)
        {
            events.updateQuestionAnswer(this);
        }
    }

    public void SetStateToUnchecked()
    {
        Checked = false;
        toggle.sprite = uncheckedToggle;

        if (events.updateQuestionAnswer != null)
        {
            events.updateQuestionAnswer(this);
        }
    }

    void UPdateUI()
    {
        toggle.sprite = (Checked) ? checkedToggle : uncheckedToggle;
    }









}
