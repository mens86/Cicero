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
    public bool expanded = true;


}
