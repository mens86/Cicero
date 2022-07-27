using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DeckCategories : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI CategoryName;
    [SerializeField] public Image SelectAllButton;
    [SerializeField] public Image ShowCategoryDecksButton;
    [SerializeField] public RectTransform rectTransf;
    [SerializeField] public float categoryMasteryNumber;
    public bool expanded = true;
    [Header("SelectAllTextures")]
    [SerializeField] public Image SelectAllToggle;
    [SerializeField] Sprite SelectAllUncheckedToggle;
    [SerializeField] Sprite SelectAllCheckedToggle;
    [Header("HideAndShow")]
    [SerializeField] public Image HideAndShowToggle;
    [SerializeField] Sprite HideAndShowUncheckedToggle;
    [SerializeField] Sprite HideAndShowCheckedToggle;
    [Header("Medals")]
    [SerializeField] public Image medalSpot;
    [SerializeField] public Sprite goldMedal;
    [SerializeField] public Sprite silverMedal;
    [SerializeField] public Sprite bronzeMedal;

    public bool SelectAllChecked = false;

    public void SelectAllUPdateUI()
    {
        SelectAllToggle.sprite = (SelectAllChecked) ? SelectAllCheckedToggle : SelectAllUncheckedToggle;
    }

    public void SelectAllSwitchState()
    {
        SelectAllChecked = !SelectAllChecked;
        SelectAllUPdateUI();

    }




    public void HideAndShowUPdateUI()
    {
        HideAndShowToggle.sprite = (expanded) ? HideAndShowCheckedToggle : HideAndShowUncheckedToggle;
    }

    public void HideAndShowSwitchState()
    {
        expanded = !expanded;
        HideAndShowUPdateUI();

    }

}
