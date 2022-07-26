using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;




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


    [SerializeField] RectTransform effectsOnScreen;
    public RectTransform EffectsOnScreen { get { return effectsOnScreen; } }

    [SerializeField] RectTransform pickedanswersContentArea;
    public RectTransform PickedAnswersContentArea { get { return pickedanswersContentArea; } }

    [SerializeField] TextMeshProUGUI questionInfoTextObject;
    public TextMeshProUGUI QuestionInfoTextObject { get { return questionInfoTextObject; } }

    [SerializeField] TextMeshProUGUI scoreText;
    public TextMeshProUGUI ScoreText { get { return scoreText; } }

    [SerializeField] TextMeshProUGUI gameFinishedText;
    public TextMeshProUGUI GameFinishedText { get { return gameFinishedText; } }

    [SerializeField] TextMeshProUGUI finalscoreText;
    public TextMeshProUGUI FinalscoreText { get { return finalscoreText; } }

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

    [SerializeField] CanvasGroup gameCanvasGroup;
    public CanvasGroup GameCanvasGroup { get { return gameCanvasGroup; } }

    [SerializeField] RectTransform finishUIElements;
    public RectTransform FinishUIElements { get { return finishUIElements; } }

}



public class UIManager : MonoBehaviour
{
    public enum ResolutionScreenType { Correct, Incorrect, LessThanCorrect, MoreThanCorrect, Finish }

    [Header("References")]
    [SerializeField] GameEvents events;
    [SerializeField] GameManager gameManager;
    [SerializeField] public GameObject gotItButton;
    [SerializeField] public RectTransform resolutionBGRect;

    [Header("Animations")]
    [SerializeField] public GameObject smokeAnimation;
    [SerializeField] public GameObject FireworksAnimation;

    [Header("UI Elements (Prefabs)")]

    [SerializeField] AnswerData answerPrefab;
    [SerializeField] AnswerData pickedAnswerPrefab;

    [SerializeField] public UIElements uIElements;


    [Space]
    [SerializeField] UIManagerParameters parameters;

    public int resStateParaHash = 0;

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
        uIElements.GameCanvasGroup.blocksRaycasts = false;

        if (type == ResolutionScreenType.Correct)
        {
            gotItButton.SetActive(false);
            if (IE_DisplayTimedResolution != null)
            {
                StopCoroutine(IE_DisplayTimedResolution);
            }
            IE_DisplayTimedResolution = DisplayTimedResolution();
            StartCoroutine(IE_DisplayTimedResolution);
        }
        else
        {
            gotItButton.SetActive(true);
        }

    }

    IEnumerator DisplayTimedResolution()
    {

        yield return new WaitForSeconds(ResolutionDelayTime);
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 1);
        uIElements.GameCanvasGroup.blocksRaycasts = true;
        if (!gameManager.IsFinished)
        {
            gameManager.Display();
        }
    }

    void UpdateResUI(ResolutionScreenType type, float score)
    {
        var highscore = PlayerPrefs.GetFloat(GameUtility.SavePrefKey);


        var answersToCurrentQuestion = gameManager.questions[gameManager.currentQuestion].Answers;
        int scoreForEachAnswer = gameManager.questions[gameManager.currentQuestion].AddScore;
        string coloredCorrectAnswers = "";
        string coloredChosenAnswers = "\n\n delegisti: \n\n";
        int maxScoreForCurrentAnswer = answersToCurrentQuestion.Length * scoreForEachAnswer;

        List<string> pickedAnsw = gameManager.PickedAnswers.Select(ad => ad.infoTextObject.text).ToList();
        List<string> actualAnswers = new List<string>();
        foreach (var a in answersToCurrentQuestion)
        {
            string correctAnswer = a.GetCorrectAnswer(pickedAnsw);
            if (correctAnswer != "")
            {
                actualAnswers.Add(correctAnswer);
            }
        }
        List<string> actualAnswersWithSynonyms = new List<string>();
        foreach (var a in answersToCurrentQuestion)
        {
            string correctAnswer = "";
            for (int i = 0; i < a.groupOfSynonyms.Count; i++)
            {
                if (0 < i && i < a.groupOfSynonyms.Count)
                {
                    correctAnswer += " / ";
                }
                correctAnswer += a.groupOfSynonyms[i];
            }
            actualAnswersWithSynonyms.Add(correctAnswer);
        }

        for (int i = 0; i < actualAnswers.Count; i++)
        {
            if (!pickedAnsw.Contains(actualAnswers[i]))
            {
                coloredCorrectAnswers += "<color=grey>" + actualAnswersWithSynonyms[i] + "</color > \n";
            }
            else
            {
                coloredCorrectAnswers += "<color=green>" + actualAnswersWithSynonyms[i] + "</color > \n";
            }

        }

        foreach (var pa in pickedAnsw)
        {
            if (actualAnswers.Contains(pa))
            {
                coloredChosenAnswers += "<color=green>" + pa + "</color > \n";
            }
            else
            {
                coloredChosenAnswers += "<color=red>" + pa + "</color > \n";
            }
        }
        if (pickedAnsw.Count == 0)
        {
            coloredChosenAnswers = "\n\n\n non delegisti responsum!";
        }





        switch (type)
        {
            case ResolutionScreenType.Correct:
                uIElements.ResolutionBG.color = parameters.CorrectBGColor;
                uIElements.ResolutionStateInfoText.text = "<color=green> \n\n\n\n\n\n\nExactum! </color>";
                uIElements.RightAnswerWasText.text = "";
                uIElements.RightAnswerText.text = "";
                uIElements.ResolutionScoreText.text = "puncta: " + score + "/" + maxScoreForCurrentAnswer;
                GameObject fireworks = Instantiate(FireworksAnimation, resolutionBGRect.transform) as GameObject;
                ParticleSystem ps = fireworks.GetComponent<ParticleSystem>();
                var main = ps.main;
                main.simulationSpeed = 0.5f;

                break;

            case ResolutionScreenType.Incorrect:
                uIElements.ResolutionBG.color = parameters.IncorrectBGColor;
                uIElements.ResolutionStateInfoText.text = "<color=red> Erratum! </color>";
                uIElements.RightAnswerWasText.text = "-" + gameManager.questions[gameManager.currentQuestion].Info + "-\n\n" + "Emendatum responsum est: ";
                uIElements.RightAnswerText.text = coloredCorrectAnswers + coloredChosenAnswers;
                uIElements.ResolutionScoreText.text = "puncta: " + score + "/" + maxScoreForCurrentAnswer;

                break;

            case ResolutionScreenType.LessThanCorrect:
                uIElements.ResolutionBG.color = parameters.IntermediateBGColor;
                uIElements.ResolutionStateInfoText.text = "<color=orange> Paene! </color>";
                uIElements.RightAnswerWasText.text = "-" + gameManager.questions[gameManager.currentQuestion].Info + "-\n\n" + "Emendatum responsum est: ";
                uIElements.RightAnswerText.text = coloredCorrectAnswers + coloredChosenAnswers;
                uIElements.ResolutionScoreText.text = "puncta: " + score + "/" + maxScoreForCurrentAnswer;
                break;

            case ResolutionScreenType.MoreThanCorrect:
                uIElements.ResolutionBG.color = parameters.IntermediateBGColor;
                uIElements.ResolutionStateInfoText.text = "<color=orange> Nimis! </color>";
                uIElements.RightAnswerWasText.text = "-" + gameManager.questions[gameManager.currentQuestion].Info + "-\n\n" + "Emendatum responsum est: ";
                uIElements.RightAnswerText.text = coloredCorrectAnswers + coloredChosenAnswers;
                uIElements.ResolutionScoreText.text = "puncta: " + score + "/" + maxScoreForCurrentAnswer;
                break;

            case ResolutionScreenType.Finish:
                uIElements.ResolutionBG.color = parameters.FinalBGColor;
                uIElements.GameFinishedText.text = "Ludus conclusus!";
                uIElements.ResolutionStateInfoText.text = "";
                uIElements.RightAnswerWasText.text = "";
                uIElements.RightAnswerText.text = "";
                uIElements.ResolutionScoreText.text = "";



                StartCoroutine(CalculateScore());
                uIElements.FinishUIElements.gameObject.SetActive(true);
                uIElements.HighScoreText.gameObject.SetActive(true);
                uIElements.HighScoreText.text = ((highscore > events.StartupHighScore ? "<color=yellow>new </color>" : string.Empty) + "Puncta altiora: " + highscore);
                break;
        }
    }

    IEnumerator CalculateScore()
    {
        if (events.CurrentFinalScore == 0) { uIElements.FinalscoreText.text = "puncta: " + 0.ToString(); yield break; }

        var scoreValue = 0;

        var scoreMoreThanZero = events.CurrentFinalScore > 0;
        while ((scoreMoreThanZero) ? scoreValue < events.CurrentFinalScore : scoreValue > events.CurrentFinalScore)
        {
            scoreValue += scoreMoreThanZero ? 1 : -1;
            uIElements.FinalscoreText.text = "puncta:\n " + scoreValue.ToString();

            yield return null;
        }
    }



    void UpdateScoreUI()
    {
        uIElements.ScoreText.text = "puncta: " + events.CurrentFinalScore;
    }




    public void ShowPickedAnswers(List<AnswerData> pickedAnswers, AnswerData answerRemoved, AnswerData answerPicked)
    {
        float offset = 0 - parameters.Margins;

        var pickedAnswersClone = GameObject.FindGameObjectsWithTag("PickedAnswer");
        if (answerRemoved != null)
        {
            foreach (var pickedAnswerClone in pickedAnswersClone)
            {
                if (pickedAnswerClone.GetComponent<AnswerData>().infoTextObject.text == answerRemoved.infoTextObject.text)
                {
                    //create temporary object for smoke effect
                    GameObject temp = new GameObject();
                    temp.transform.parent = uIElements.EffectsOnScreen.transform;
                    temp.AddComponent<RectTransform>();
                    temp.GetComponent<RectTransform>().position = new Vector3(pickedAnswerClone.GetComponent<RectTransform>().position.x - 800, pickedAnswerClone.GetComponent<RectTransform>().position.y - 1050, 0);
                    StartCoroutine(ExecuteAfterTime(0.3f));
                    IEnumerator ExecuteAfterTime(float time)
                    {
                        yield return new WaitForSeconds(time);
                        Destroy(temp);
                    }
                    //

                    GameObject smoke = Instantiate(smokeAnimation, temp.GetComponent<RectTransform>(), false) as GameObject;

                    Destroy(pickedAnswerClone);
                }
            }
        }
        else
        {
            AnswerData answer = (AnswerData)Instantiate(pickedAnswerPrefab, uIElements.PickedAnswersContentArea);
            answer.infoTextObject.text = answerPicked.infoTextObject.text;
        }

        //vecchio modo di mettere e togliere le pickedanswer, sostituito con uno un pelo più efficiente che permette anche di piazzare l'effetto fumo. 
        //Se fra un po' vedo che funzia ancora, posso cancellare questo

        //ErasePickedAnswers();
        /*
        for (int i = 0; i < pickedAnswers.Count; i++)
        {
            AnswerData newAnswer = (AnswerData)Instantiate(pickedAnswerPrefab, uIElements.PickedAnswersContentArea);
            newAnswer.UpdateData(pickedAnswers[i].infoTextObject.text.ToString(), i);

            newAnswer.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (newAnswer.Rect.sizeDelta.y + parameters.Margins);
            uIElements.PickedAnswersContentArea.sizeDelta = new Vector2(uIElements.PickedAnswersContentArea.sizeDelta.x, offset * -1);
        }
        */


    }

    public void ErasePickedAnswers()
    {
        var pickedAnswersClone = GameObject.FindGameObjectsWithTag("PickedAnswer");

        foreach (var pickedAnswer in pickedAnswersClone)
        {
            Destroy(pickedAnswer);
        }
    }


    public void AnotherSessionButtonUIReset()
    {
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 1);
        uIElements.GameCanvasGroup.blocksRaycasts = true;
        UpdateScoreUI();
    }

    public void ContinueGame()
    {
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 1);
        uIElements.GameCanvasGroup.blocksRaycasts = true;
        if (!gameManager.IsFinished)
        {
            gameManager.Display();
        }
        else
        {
            gameManager.SetHighScore();
            events.DisplayResolutionScreen(UIManager.ResolutionScreenType.Finish, 0);
        }

    }
}
