using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    #region Variables

    //private Data data = new Data();
    public List<Question> questions;
    public List<TextAsset> QuestionsFileNames;

    [SerializeField] GameEvents events = null;

    [SerializeField] Animator timerAnimator = null;
    [SerializeField] TextMeshProUGUI timerText = null;
    [SerializeField] Color timerHalfWayOutColor = Color.yellow;
    [SerializeField] Color timerAlmostOutColor = Color.red;
    private Color timerDefaultColor;

    public List<AnswerData> PickedAnswers = new List<AnswerData>();
    private List<int> FinishedQuestions = new List<int>();
    public int currentQuestion = 0;

    private int timerStateParaHash = 0;
    public enum UserAnswersScenario { AllCorrect, LessThanCorrect, MoreThanCorrect, AllWrong }


    private IEnumerator IE_StartTimer = null;

    private bool IsFinished
    {
        get
        {
            return (FinishedQuestions.Count < questions.Count) ? false : true;
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
        questions = FindObjectOfType<ParserQuestions_csv>().ParseQuestionsFile(QuestionsFileNames);
    }

    void Start()
    {
        events.StartupHighScore = PlayerPrefs.GetInt(GameUtility.SavePrefKey);

        timerDefaultColor = timerText.color;

        timerStateParaHash = Animator.StringToHash("TimerState");

        var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed);

        Display();
    }

    public void UpdateAnswers(AnswerData newAnswer)
    {
        bool alreadyPicked = false;
        for (int i = 0; i < PickedAnswers.Count; i++)
        {
            if (PickedAnswers[i].infoTextObject.text.Contains(newAnswer.infoTextObject.text))
            {
                alreadyPicked = true;
            }
        }

        if (alreadyPicked)
        {

            int indexToRemove = -1;
            for (int i = 0; i < PickedAnswers.Count; i++)
            {
                if (PickedAnswers[i].infoTextObject.text.Contains(newAnswer.infoTextObject.text))
                {
                    indexToRemove = i;
                }
            }
            PickedAnswers.RemoveAt(indexToRemove);

            GameObject.Find("Managers").GetComponent<UIManager>().ShowPickedAnswers(PickedAnswers);
        }

        else
        {
            PickedAnswers.Add(newAnswer);
            GameObject.Find("InputField").GetComponent<Autocomplete>().inputField.text = "";
            GameObject.Find("Managers").GetComponent<UIManager>().ShowPickedAnswers(PickedAnswers);
        }

    }


    public void EraseAnswers()
    {
        PickedAnswers = new List<AnswerData>();
    }


    void Display()
    {
        GameObject.Find("Managers").GetComponent<UIManager>().ErasePickedAnswers(PickedAnswers);
        EraseAnswers();
        GameObject.Find("InputField").GetComponent<Autocomplete>().inputField.text = "";
        var question = GetRandomQuestion();

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
        UserAnswersScenario scenario = CheckAnswers();
        FinishedQuestions.Add(currentQuestion);

        switch (scenario)
        {
            case UserAnswersScenario.AllCorrect:
                GameObject.Find("Managers").GetComponent<UIManager>().ResolutionDelayTime = 1;
                UpdateScore(questions[currentQuestion].AddScore);
                events.DisplayResolutionScreen(UIManager.ResolutionScreenType.Correct, questions[currentQuestion].AddScore);
                AudioManager.Instance.PlaySound("CorrectSFX");
                break;
            case UserAnswersScenario.AllWrong:
                GameObject.Find("Managers").GetComponent<UIManager>().ResolutionDelayTime = 3;
                UpdateScore(-questions[currentQuestion].AddScore);
                events.DisplayResolutionScreen(UIManager.ResolutionScreenType.Incorrect, questions[currentQuestion].AddScore);
                AudioManager.Instance.PlaySound("IncorrectSFX");
                break;
            case UserAnswersScenario.LessThanCorrect:
                GameObject.Find("Managers").GetComponent<UIManager>().ResolutionDelayTime = 3;
                UpdateScore(questions[currentQuestion].AddScore / 2);
                events.DisplayResolutionScreen(UIManager.ResolutionScreenType.LessThanCorrect, questions[currentQuestion].AddScore / 2);
                AudioManager.Instance.PlaySound("IncorrectSFX");
                break;
            case UserAnswersScenario.MoreThanCorrect:
                GameObject.Find("Managers").GetComponent<UIManager>().ResolutionDelayTime = 3;
                UpdateScore(questions[currentQuestion].AddScore / 2);
                events.DisplayResolutionScreen(UIManager.ResolutionScreenType.MoreThanCorrect, questions[currentQuestion].AddScore / 2);
                AudioManager.Instance.PlaySound("IncorrectSFX");
                break;
        }

        float currentResolutionDelayTime = GameObject.Find("Managers").GetComponent<UIManager>().ResolutionDelayTime;
        if (!IsFinished)
        {
            StartCoroutine(ExecuteAfterTime(currentResolutionDelayTime));
            IEnumerator ExecuteAfterTime(float time)
            {
                yield return new WaitForSeconds(time);
                Display();
            }
        }
        else
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






    Question GetRandomQuestion()
    {
        var randomIndex = GetRandomQuestionIndex();
        currentQuestion = randomIndex;

        return questions[currentQuestion];
    }
    int GetRandomQuestionIndex()
    {
        var random = 0;
        if (FinishedQuestions.Count < questions.Count)
        {
            do
            {
                random = UnityEngine.Random.Range(0, questions.Count);
            } while (FinishedQuestions.Contains(random) || random == currentQuestion);
        }
        return random;
    }





    UserAnswersScenario CheckAnswers()
    {
        if (PickedAnswers.Count > 0)
        {
            List<string> q = questions[currentQuestion].GetCorrectAnswers();
            List<string> p = PickedAnswers.Select(x => x.infoTextObject.text).ToList();

            /*
            Debug.Log("Risposte giuste");
            foreach( var x in p) {
            Debug.Log( x);}
            Debug.Log("Risposte selezionate");
            foreach( var x in c) {
            Debug.Log( x);}
            */

            var qq = q.Except(p).ToList();
            var pp = p.Except(q).ToList();



            //meno delle giuste: 
            if (qq.Any() && !pp.Any())
            {
                return UserAnswersScenario.LessThanCorrect;
            }
            //più delle giuste: 
            if (!qq.Any() && pp.Any())
            {
                return UserAnswersScenario.MoreThanCorrect;
            }

            //tutte giuste
            if (!qq.Any() && !pp.Any())
            {
                return UserAnswersScenario.AllCorrect;
            }

            //tutte sbagliate
            if (qq.Any() && pp.Any())
            {
                return UserAnswersScenario.AllWrong;
            }
        }
        return UserAnswersScenario.AllWrong;
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
    private void SetHighScore()
    {
        var highscore = PlayerPrefs.GetInt(GameUtility.SavePrefKey);
        if (highscore < events.CurrentFinalScore)
        {
            PlayerPrefs.SetInt(GameUtility.SavePrefKey, events.CurrentFinalScore);
        }
    }

    private void UpdateScore(int add)
    {
        events.CurrentFinalScore += add;
        //se vuoi che il minimo punteggio sia 0
        /*
        if(events.CurrentFinalScore < 0)
        {
            events.CurrentFinalScore = 0;
        }
        */
        if (events.ScoreUpdated != null)
        {
            events.ScoreUpdated();
        }
    }

}
