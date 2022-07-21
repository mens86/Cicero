using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;


[Serializable()]
public struct UIManagerParameters
{
    [Header("Answers Options")]
    [SerializeField] float margins;
    public float Margins { get { return margins; } }

    [Header("Resolution Screen Options")]
    [SerializeField] Color correctBGColor;
    public Color CorrectBGColor { get { return correctBGColor; } }
    [SerializeField] Color incorrectBGColor;
    public Color IncorrectBGColor { get { return incorrectBGColor; } }
    [SerializeField] Color finalBGColor;
    public Color FinalBGColor { get { return finalBGColor; } }
    [SerializeField] Color intermediateBGColor;
    public Color IntermediateBGColor { get { return intermediateBGColor; } }
}


[Serializable()]
public struct UIElements
{



    [SerializeField] RectTransform pickedanswersContentArea;
    public RectTransform PickedAnswersContentArea { get { return pickedanswersContentArea; } }

    [SerializeField] TextMeshProUGUI questionInfoTextObject;
    public TextMeshProUGUI QuestionInfoTextObject { get { return questionInfoTextObject; } }

    [SerializeField] TextMeshProUGUI scoreText;
    public TextMeshProUGUI ScoreText { get { return scoreText; } }

    [Space]

    [SerializeField] Animator resolutionScreenAnimator;
    public Animator ResolutionScreenAnimator { get { return resolutionScreenAnimator; } }

    [SerializeField] Image resolutionBG;
    public Image ResolutionBG { get { return resolutionBG; } }

    [SerializeField] TextMeshProUGUI resolutionStateInfoText;
    public TextMeshProUGUI ResolutionStateInfoText { get { return resolutionStateInfoText; } }

    [SerializeField] TextMeshProUGUI rightAnswerWasText;
    public TextMeshProUGUI RightAnswerWasText { get { return rightAnswerWasText; } }

    [SerializeField] TextMeshProUGUI rightAnswerText;
    public TextMeshProUGUI RightAnswerText { get { return rightAnswerText; } }

    [SerializeField] TextMeshProUGUI resolutionScoreText;
    public TextMeshProUGUI ResolutionScoreText { get { return resolutionScoreText; } }

    [Space]

    [SerializeField] TextMeshProUGUI highScoreText;
    public TextMeshProUGUI HighScoreText { get { return highScoreText; } }

    [SerializeField] CanvasGroup mainCanvasGroup;
    public CanvasGroup MainCanvasGroup { get { return mainCanvasGroup; } }

    [SerializeField] RectTransform finishUIElements;
    public RectTransform FinishUIElements { get { return finishUIElements; } }

}



public class UIManager : MonoBehaviour
{
    public enum ResolutionScreenType { Correct, Incorrect, LessThanCorrect, MoreThanCorrect, Finish }

    [Header("References")]
    [SerializeField] GameEvents events;

    [Header("UI Elements (Prefabs)")]

    [SerializeField] AnswerData answerPrefab;
    [SerializeField] AnswerData pickedAnswerPrefab;

    [SerializeField] UIElements uIElements;


    [Space]
    [SerializeField] UIManagerParameters parameters;

    private int resStateParaHash = 0;

    public float ResolutionDelayTime = 1;

    private IEnumerator IE_DisplayTimedResolution;

    void OnEnable()
    {
        events.UpdateQuestionUI += UpdateQuestionUI;
        events.DisplayResolutionScreen += DisplayResolution;
        events.ScoreUpdated += UpdateScoreUI;
    }
    void OnDisable()
    {
        events.UpdateQuestionUI -= UpdateQuestionUI;
        events.DisplayResolutionScreen -= DisplayResolution;
        events.ScoreUpdated -= UpdateScoreUI;
    }

    void Start()
    {
        UpdateScoreUI();
        resStateParaHash = Animator.StringToHash("ScreenState");
    }

    void UpdateQuestionUI(Question question)
    {
        uIElements.QuestionInfoTextObject.text = question.Info;

    }

    void DisplayResolution(ResolutionScreenType type, float score)
    {
        UpdateResUI(type, score);
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 2);
        uIElements.MainCanvasGroup.blocksRaycasts = false;

        if (type != ResolutionScreenType.Finish)
        {
            if (IE_DisplayTimedResolution != null)
            {
                StopCoroutine(IE_DisplayTimedResolution);
            }
            IE_DisplayTimedResolution = DisplayTimedResolution();
            StartCoroutine(IE_DisplayTimedResolution);
        }
    }

    IEnumerator DisplayTimedResolution()
    {

        yield return new WaitForSeconds(ResolutionDelayTime);
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 1);
        uIElements.MainCanvasGroup.blocksRaycasts = true;
    }

    void UpdateResUI(ResolutionScreenType type, float score)
    {
        var highscore = PlayerPrefs.GetFloat(GameUtility.SavePrefKey);


        var answersToCurrentQuestion = GameObject.Find("Managers").GetComponent<GameManager>().questions[GameObject.Find("Managers").GetComponent<GameManager>().currentQuestion].Answers;
        int scoreForEachAnswer = GameObject.Find("Managers").GetComponent<GameManager>().questions[GameObject.Find("Managers").GetComponent<GameManager>().currentQuestion].AddScore;
        string CorrectAnswers = "";
        int maxScoreForCurrentAnswer = answersToCurrentQuestion.Length * scoreForEachAnswer;
        foreach (var a in answersToCurrentQuestion)
        {
            CorrectAnswers += "- " + a.groupOfSynonyms[0] + "\n"; // Questo si può fare meglio e mostrare tutti i sinonimi con un forlooppino. Si tratta di decidere.
        }

        switch (type)
        {
            case ResolutionScreenType.Correct:
                uIElements.ResolutionBG.color = parameters.CorrectBGColor;
                uIElements.ResolutionStateInfoText.text = "Exactum!";
                uIElements.RightAnswerWasText.text = "";
                uIElements.RightAnswerText.text = "";
                uIElements.ResolutionScoreText.text = "puncta: " + score + "/" + maxScoreForCurrentAnswer;
                break;

            case ResolutionScreenType.Incorrect:
                uIElements.ResolutionBG.color = parameters.IncorrectBGColor;
                uIElements.ResolutionStateInfoText.text = "Erratum!";
                uIElements.RightAnswerWasText.text = "Emendata responsio est: ";
                uIElements.RightAnswerText.text = CorrectAnswers;
                uIElements.ResolutionScoreText.text = "puncta: " + score + "/" + maxScoreForCurrentAnswer;
                break;

            case ResolutionScreenType.LessThanCorrect:
                uIElements.ResolutionBG.color = parameters.IntermediateBGColor;
                uIElements.ResolutionStateInfoText.text = "Paene!";
                uIElements.RightAnswerWasText.text = "Emendata responsio est: ";
                uIElements.RightAnswerText.text = CorrectAnswers;
                uIElements.ResolutionScoreText.text = "puncta: " + score + "/" + maxScoreForCurrentAnswer;
                break;

            case ResolutionScreenType.MoreThanCorrect:
                uIElements.ResolutionBG.color = parameters.IntermediateBGColor;
                uIElements.ResolutionStateInfoText.text = "Nimis!";
                uIElements.RightAnswerWasText.text = "Emendata responsio est: ";
                uIElements.RightAnswerText.text = CorrectAnswers;
                uIElements.ResolutionScoreText.text = "puncta: " + score + "/" + maxScoreForCurrentAnswer;
                break;

            case ResolutionScreenType.Finish:
                uIElements.ResolutionBG.color = parameters.FinalBGColor;
                uIElements.ResolutionStateInfoText.text = "Ludus conclusus!";
                uIElements.ResolutionStateInfoText.rectTransform.anchoredPosition = new Vector3(0, -600, 0);
                uIElements.RightAnswerWasText.text = "";
                uIElements.RightAnswerText.text = "";
                uIElements.ResolutionScoreText.rectTransform.anchoredPosition = new Vector3(0, -1200, 0);


                StartCoroutine(CalculateScore());
                uIElements.FinishUIElements.gameObject.SetActive(true);
                uIElements.HighScoreText.gameObject.SetActive(true);
                uIElements.HighScoreText.text = ((highscore > events.StartupHighScore ? "<color=yellow>new </color>" : string.Empty) + "Puncta altiora: " + highscore);
                break;
        }
    }

    IEnumerator CalculateScore()
    {
        if (events.CurrentFinalScore == 0) { uIElements.ResolutionScoreText.text = 0.ToString(); yield break; }

        var scoreValue = 0;
        var scoreMoreThanZero = events.CurrentFinalScore > 0;
        while ((scoreMoreThanZero) ? scoreValue < events.CurrentFinalScore : scoreValue > events.CurrentFinalScore)
        {
            scoreValue += scoreMoreThanZero ? 1 : -1;
            uIElements.ResolutionScoreText.text = "puncta: " + scoreValue.ToString();

            yield return null;
        }
    }



    void UpdateScoreUI()
    {
        uIElements.ScoreText.text = "Puncta: " + events.CurrentFinalScore;
    }




    public void ShowPickedAnswers(List<AnswerData> picked)
    {
        //foreach (var i in picked){Debug.Log(i.infoTextObject.text.ToString());}
        //Debug.Log("--------");

        float offset = 0 - parameters.Margins;

        ErasePickedAnswers(picked);

        for (int i = 0; i < picked.Count; i++)
        {
            AnswerData newAnswer = (AnswerData)Instantiate(pickedAnswerPrefab, uIElements.PickedAnswersContentArea);
            newAnswer.UpdateData(picked[i].infoTextObject.text.ToString(), i);

            newAnswer.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (newAnswer.Rect.sizeDelta.y + parameters.Margins);
            uIElements.PickedAnswersContentArea.sizeDelta = new Vector2(uIElements.PickedAnswersContentArea.sizeDelta.x, offset * -1);
        }
    }

    public void ErasePickedAnswers(List<AnswerData> picked)
    {
        var clones = GameObject.FindGameObjectsWithTag("PickedAnswer");
        foreach (var clone in clones)
        {
            //Debug.Log("vediamo un p0'");
            Destroy(clone);
        }
    }






    public void ContinueGameButtonUIReset()
    {
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 1);
        uIElements.MainCanvasGroup.blocksRaycasts = true;
        UpdateScoreUI();
    }


}
