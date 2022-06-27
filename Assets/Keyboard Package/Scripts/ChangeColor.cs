using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColor : MonoBehaviour
{


    private IEnumerator waitHalfSec;

    public void ChangeColorWhenClicked(GameObject Fill)
    {

        Color pressedButtonColor = new Color32(90, 114, 192, 255);

        Fill.GetComponent<Image>().color = pressedButtonColor;
        //GameObject.Find("Fill").GetComponent<Image>().color = pressedButtonColor;


        waitHalfSec = PleasewaitHalfSec();
        StartCoroutine(waitHalfSec);


    }


    IEnumerator PleasewaitHalfSec()
    {
        yield return new WaitForSeconds(0.1f);
        Color releasedButtonColor = new Color32(0, 0, 0, 0);
        GameObject.Find("Fill").GetComponent<Image>().color = releasedButtonColor;
    }
}