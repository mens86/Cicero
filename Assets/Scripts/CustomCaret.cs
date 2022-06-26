using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// This class is for creating a custom caret for TextMeshPro InputField GameObjects in Unity.
/// </summary>
public class CustomCaret : MonoBehaviour
{
    #region Variables

    ///<summary> The text object for the caret. </summary>
    public TMP_Text caret;

    ///<summary> The transform, for changing the position.  </summary>
    public RectTransform rect;

    ///<summary>
    ///Number (in seconds) to wait before switching the 
    ///blink state of the caret.
    ///</summary>
    public float blinkRate = 0.5f;

    /// <summary> The original position of the caret. </summary>
    public Vector2 originPos;

    /// <summary> The last character input in the command input string. </summary>
    private char lastChar;

    /// <summary> The current character input in the command input string. </summary>
    private char currChar;

    /// <summary>Boolean to determine whether or not to use the CheckForReset Coroutine.</summary>
    private bool debugCheck = true;

    /// <summary>
    /// An approximate value to convert spacing from TMPro values to default font space values.
    /// This value may not work for all fonts, change as needed.
    /// Default: 5.3f
    /// </summary>
    [SerializeField]
    private float fontSpacing = 5.3f;

    /// <summary> The font object, used for getting the advance distance of each character. </summary>
    [SerializeField]
    private TMP_FontAsset tmpFont;

    /// <summary> The text object of the input field, for getting the text inside the input box. </summary>
    [SerializeField]
    private TMP_Text inputText;

    /// <summary> The input field, needed for getting the char limit and total text input. </summary>
    [SerializeField]
    private TMP_InputField inputField;

    private WaitForSeconds blinkDelay = new WaitForSeconds(0.5f);

    private WaitForSeconds debugDelay = new WaitForSeconds(1.0f);

    private IEnumerator blinkRoutine;
    private IEnumerator debugRoutine;

    /// <summary>
    /// The string that should replace the vertical line as the caret in the TMPro InputField.
    /// Default: "_" (underscore)
    /// </summary>
    public string replaceCaret = "_";
    private string emptyString = "";


    /// <summary>
    /// Boolean to determine whether the caret has been enabled at least once.
    /// This is used to catch errors in case the Reset() method is ever called before the caret is enabled.
    /// </summary>
    private bool initialized = false;

    #endregion

    #region Methods

    /// <summary>
    /// Assign the coroutines in Awake so that
    /// StopRoutine actually does something.
    /// </summary>
    void Awake()
    {
        blinkRoutine = Blink();
        debugRoutine = CheckForReset();
    }

    /// <summary> Run when the command input panel is activated. </summary>
    void OnEnable()
    {
        originPos = rect.anchoredPosition;
        StartCoroutine(blinkRoutine);
        if (debugCheck)
            StartCoroutine(debugRoutine);
        initialized = true;
    }

    /// <summary> Run when the command input panel is deactivated. </summary>
    private void OnDisable()
    {
        Reset();
        StopCoroutine(blinkRoutine);
        if (debugCheck)
            StopCoroutine(debugRoutine);
    }

    /// <summary> Reset the caret to its original position. </summary>
    public void Reset()
    {
        if (initialized)
            rect.anchoredPosition = originPos;
    }





    public void backspace(Text deletedLetter)
    {
        float xAdv = 0f;
        if (inputField.text.Length > 0 && !(inputText.textInfo.characterCount >=
                     inputField.characterLimit + 1))
        {

            //Try to move it to the right, using this in case of weird behaviour.
            try
            {
                //Set xAdv to the distance specified by the font, according to the new character entered.
                //fontSpacing is an approximation to the distance difference between TMPro fonts and regular fonts.
                //The 5.3 value may not work for all fonts, and should be tweaked accordingly.
#if UNITY_2018
                    xAdv = tmpFont.characterDictionary[Input.inputString[0]].xAdvance / fontSpacing;
#else
                char goingBack = deletedLetter.text.Substring(deletedLetter.text.Length - 1)[0];
                xAdv = tmpFont.characterLookupTable[goingBack].glyph.metrics.horizontalAdvance / fontSpacing;

#endif
                //Move the caret's position to the left xAdv distance.
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x - xAdv, rect.anchoredPosition.y);

            }

            //Just in case anything weird happens.
            catch (Exception ex)
            {
                //Output the message and stack trace of the exception.
                Debug.Log(ex.Message + "Stack Trace: " + ex.StackTrace);
            }

        }
    }





    float currentLenght = 0f;
    float lastLenght = 0f;
    bool WasBackSpaceUsed = false;

    /// <summary> Change location based on input. Called by ipFld onValueChanged method. </summary>
    public void ChangeLocation()
    {
        //Distance to move the caret in the x direction.
        float xAdv = 0f;
        //checks if backspace was used
        currentLenght = inputField.text.Length;
        if (currentLenght > lastLenght)
        {
            lastLenght = currentLenght;
            WasBackSpaceUsed = false;
        }
        else
        {
            lastLenght = currentLenght;
            WasBackSpaceUsed = true;
        }



        //If the character count is greater than the max & input is not backspace, then return.
        if (rect.anchoredPosition.x > 500 && !WasBackSpaceUsed)
        {
            rect.anchoredPosition = new Vector2(558, rect.anchoredPosition.y);
        }

        //If the user presses a key that inputs a character, and the limit has not been reached,
        //then attempt to move the caret to the right.
        else if (inputField.text.Length > 0 && !(inputText.textInfo.characterCount >= inputField.characterLimit + 1) && !WasBackSpaceUsed)
        {
            //Try to move it to the right, using this in case of weird behaviour.
            try
            {
                //Set xAdv to the distance specified by the font, according to the new character entered.
                //fontSpacing is an approximation to the distance difference between TMPro fonts and regular fonts.
                //The 5.3 value may not work for all fonts, and should be tweaked accordingly.
#if UNITY_2018
                        xAdv = tmpFont.characterDictionary[Input.inputString[0]].xAdvance / fontSpacing;

#else
                currChar = inputField.text.Substring(inputField.text.Length - 1)[0];
                xAdv = tmpFont.characterLookupTable[currChar].glyph.metrics.horizontalAdvance / fontSpacing;
#endif

                //Move the caret's position to the right xAdv distance.
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + xAdv, rect.anchoredPosition.y);

            }

            //Just in case anything weird happens.
            catch (Exception ex)
            {
                //Output the message and stack trace of the exception.
                Debug.Log(ex.Message + "Stack Trace: " + ex.StackTrace);
            }
        }

        //If the user presses the Enter/Return key, keypad enter, or Escape,
        //then reset the location and stop blink if needed.
        else if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Escape))
        {
            //If the Escape key is pressed, then stop the coroutines.
            if (Input.GetKey(KeyCode.Escape))
            {
                //StopCoroutine(blinkRoutine);
                //StopCoroutine(debugRoutine);
            }

            //Reset the location of the caret.
            Reset();
        }

        //If the user presses any other key that doesn't
        //input a character (i.e. Shift/Alt/Ctrl etc.),
        //then just return.
        else if (Input.inputString.Equals(emptyString, StringComparison.Ordinal))
        {
            return;
        }
        //After input checks, check to see if the input text is empty.
        //If it isn't empty, then set lastChar to the last character in the string.
        if (inputField.text.Length > 0)
            lastChar = inputField.text.Substring(inputField.text.Length - 1)[0];

    }



    /// <summary>
    /// Coroutine that handles the caret blinking behaviour.
    /// </summary>
    IEnumerator Blink()
    {
        while (true)
        {
            if (caret.text.Equals(emptyString, StringComparison.Ordinal))
                caret.text = replaceCaret;
            else
                caret.text = emptyString;
            yield return blinkDelay;
        }
    }

    ///<summary> 
    ///Coroutine that checks if string is empty every
    ///interval of debugDelay (Default 3 seconds).
    ///If so, reset to originPos. 
    ///</summary>
    IEnumerator CheckForReset()
    {
        while (true)
        {
            if (inputText.text.Length == 1)
                rect.anchoredPosition = originPos;
            yield return null;
        }
    }

    #endregion
}