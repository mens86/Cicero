using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public enum UserAnswersScenario { AllCorrect, LessThanCorrect, MoreThanCorrect, AllWrong }


public class GameManager : MonoBehaviour
{
    #region Variables

    //private Data data = new Data();
    public MemoryIndex memoryIndex;
    public List<Question> questions = new List<Question>();
    public List<TextAsset> SelectedDecks_names
    {
        get
        {
            return memoryIndex.SelectedDecks_names;
        }
        set
        {
            memoryIndex.SelectedDecks_names = value;
        }
    }

    [SerializeField] GameEvents events = null;
    [SerializeField] UIManager UIManager = null;


    [SerializeField] Animator timerAnimator = null;
    [SerializeField] TextMeshProUGUI timerText = null;
    [SerializeField] Color timerHalfWayOutColor = Color.yellow;
    [SerializeField] Color timerAlmostOutColor = Color.red;
    private Color timerDefaultColor;

    public List<AnswerData> PickedAnswers = new List<AnswerData>();
    public Autocomplete autocomplete;
    private List<int> FinishedQuestions = new List<int>();
    public int currentQuestion = 0;
    private int timerStateParaHash = 0;
    public bool isPaused = false;



    private IEnumerator IE_StartTimer = null;

    public bool IsFinished
    {
        get
        {
            return (FinishedQuestions.Count < questions.Count && FinishedQuestions.Count < 10) ? false : true;
        }
    }

    #endregion

    void OnEnable()
    {
        events.updateQuestionAnswer += UpdateAnswers;
    }

    void OnDisable()
    {
        events.updateQuestionAnswer -= UpdateAnswers;
    }

    void Awake()
    {
        events.CurrentFinalScore = 0;
        InitQuestions_WithSelectedDecks(SelectedDecks_names);
        CreateAutoCompleteList();
    }


    public void InitQuestions_WithSelectedDecks(List<TextAsset> selectedDecks)
    {
        List<Question> questionsToSort = new List<Question>();

        foreach (var question_filename in selectedDecks)
        {
            var selectedDeck_name = question_filename.name;
            List<Question> questions_matching_filename = memoryIndex.persistentQuestionList.Where(q => q.question_filename == selectedDeck_name).ToList();
            questionsToSort.AddRange(questions_matching_filename);
        }

        //sorting questions by date
        var questions_matching_filename_sorted = (from q in questionsToSort orderby q.cardProperties.cardExpDate select q).ToList();
        questions.AddRange(questions_matching_filename_sorted);


    }
    void CreateAutoCompleteList()
    {
        foreach (var question in questions)
        {
            foreach (var answer in question.Answers)
            {
                foreach (var synonym in answer.groupOfSynonyms)
                {
                    autocomplete.allAnswers.Add(synonym);
                }
            }
        }
    }

    void Start()
    {
        events.StartupHighScore = PlayerPrefs.GetFloat(GameUtility.SavePrefKey);
        timerDefaultColor = timerText.color;
        timerStateParaHash = Animator.StringToHash("TimerState");
        Display();
    }




    public void UpdateAnswers(AnswerData newAnswer)
    {
        AnswerData answerRemoved = null;

        bool alreadyPicked = false;
        for (int i = 0; i < PickedAnswers.Count; i++)
        {
            if (PickedAnswers[i].infoTextObject.text == newAnswer.infoTextObject.text)
            {
                alreadyPicked = true;
            }
        }

        if (alreadyPicked)
        {
            int indexToRemove = -1;
            for (int i = 0; i < PickedAnswers.Count; i++)
            {
                if (PickedAnswers[i].infoTextObject.text == newAnswer.infoTextObject.text)
                {
                    indexToRemove = i;
                    answerRemoved = newAnswer;


                }
            }
            PickedAnswers.RemoveAt(indexToRemove);
            UIManager.ShowPickedAnswers(PickedAnswers, answerRemoved, newAnswer);
        }
        else
        {
            answerRemoved = null;
            PickedAnswers.Add(newAnswer);
            GameObject.Find("InputField").GetComponent<Autocomplete>().inputField.text = "";
            UIManager.ShowPickedAnswers(PickedAnswers, answerRemoved, newAnswer);
        }

    }


    public void EraseAnswers()
    {
        PickedAnswers = new List<AnswerData>();
    }


    public void Display()
    {
        UIManager.ErasePickedAnswers();
        EraseAnswers();
        GameObject.Find("InputField").GetComponent<Autocomplete>().inputField.text = "";
        var question = questions[currentQuestion];


        if (events.UpdateQuestionUI != null)
        {
            events.UpdateQuestionUI(question);
        }
        else
        {
            Debug.LogWarning("Ups! Something went wrong while trying to display new Question UI Data. GameEvents.UPdateQuestionUI is null. Issue occurred in GameManager.Display() method");
        }
        if (question.UseTimer)
        {
            UPdateTimer(question.UseTimer);
        }
    }

    public void Accept()
    {
        UPdateTimer(false);
        List<string> pickedAnswers_string = PickedAnswers.Select(ad => ad.infoTextObject.text).ToList();
        (UserAnswersScenario scenario, float multiplier) = questions[currentQuestion].CheckAnswers(pickedAnswers_string);
        FinishedQuestions.Add(currentQuestion);
        int scorePerAnswer = questions[currentQuestion].AddScore;
        float score = Mathf.Round(scorePerAnswer * multiplier);

        switch (scenario)
        {
            case UserAnswersScenario.AllCorrect:
                UIManager.ResolutionDelayTime = 1;
                UpdateScore(score);
                events.DisplayResolutionScreen(UIManager.ResolutionScreenType.Correct, score);
                AudioManager.Instance.PlaySound("CorrectSFX");
                break;
            case UserAnswersScenario.AllWrong:
                UIManager.ResolutionDelayTime = 3;
                UpdateScore(score);
                events.DisplayResolutionScreen(UIManager.ResolutionScreenType.Incorrect, score);
                AudioManager.Instance.PlaySound("IncorrectSFX");
                break;
            case UserAnswersScenario.LessThanCorrect:
                UIManager.ResolutionDelayTime = 3;
                UpdateScore(score);
                events.DisplayResolutionScreen(UIManager.ResolutionScreenType.LessThanCorrect, score);
                AudioManager.Instance.PlaySound("IncorrectSFX");
                break;
            case UserAnswersScenario.MoreThanCorrect:
                UIManager.ResolutionDelayTime = 3;
                UpdateScore(score);
                events.DisplayResolutionScreen(UIManager.ResolutionScreenType.MoreThanCorrect, score);
                AudioManager.Instance.PlaySound("IncorrectSFX");
                break;
        }

        float currentResolutionDelayTime = UIManager.ResolutionDelayTime;
        if (!IsFinished)
        {
            currentQuestion += 1;
        }
        else
        {
            if (scenario == UserAnswersScenario.AllCorrect)
            {
                StartCoroutine(ExecuteAfterTime(currentResolutionDelayTime));
                IEnumerator ExecuteAfterTime(float time)
                {
                    yield return new WaitForSeconds(time);
                    SetHighScore();
                    events.DisplayResolutionScreen(UIManager.ResolutionScreenType.Finish, 0);
                }
            }

        }
    }



    void UPdateTimer(bool state)
    {
        switch (state)
        {
            case true:
                IE_StartTimer = StartTimer();
                StartCoroutine(IE_StartTimer);

                timerAnimator.SetInteger(timerStateParaHash, 2);
                break;

            case false:
                if (IE_StartTimer != null)
                {
                    StopCoroutine(IE_StartTimer);
                }

                timerAnimator.SetInteger(timerStateParaHash, 1);
                break;
        }
    }

    IEnumerator StartTimer()
    {
        var totalTime = questions[currentQuestion].Timer;
        var timeLeft = totalTime;

        timerText.color = timerDefaultColor;
        while (timeLeft > 0)
        {
            timeLeft--;

            if (timeLeft < totalTime / 2)
            {
                AudioManager.Instance.PlaySound("CountdownSFX");
            }

            if (timeLeft < totalTime / 2 && timeLeft > totalTime / 4)
            {
                timerText.color = timerHalfWayOutColor;
            }

            if (timeLeft < totalTime / 4)
            {
                timerText.color = timerAlmostOutColor;
            }

            timerText.text = timeLeft.ToString();
            yield return new WaitForSeconds(1.0f);
        }
        Accept();
    }


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    /// Function that is called to set new highscore if game score is higher
    public void SetHighScore()
    {
        var highscore = PlayerPrefs.GetFloat(GameUtility.SavePrefKey);
        if (highscore < events.CurrentFinalScore)
        {
            PlayerPrefs.SetFloat(GameUtility.SavePrefKey, events.CurrentFinalScore);
        }
    }

    private void UpdateScore(float add)
    {
        events.CurrentFinalScore += add;

        if (events.ScoreUpdated != null)
        {
            events.ScoreUpdated();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void AnotherSessionButton()
    {
        UIManager.gotItButton.SetActive(false);
        UIManager.uIElements.ResolutionScreenAnimator.SetInteger(UIManager.resStateParaHash, 2);
        memoryIndex.Save(memoryIndex.persistentQuestionList);
        events.StartupHighScore = PlayerPrefs.GetFloat(GameUtility.SavePrefKey);
        timerDefaultColor = timerText.color;
        timerStateParaHash = Animator.StringToHash("TimerState");
        currentQuestion = 0;
        questions = new List<Question>();
        FinishedQuestions = new List<int>();
        InitQuestions_WithSelectedDecks(SelectedDecks_names);
        Display();
    }
}

