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
    public string QuestionsFileName;

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

    private IEnumerator IE_WaitTillNextRound = null;
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
        questions = FindObjectOfType<ParserQuestions_csv>().ParseQuestionsFile(QuestionsFileName);
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
        /*
        if (data.Questions[currentQuestion].Type == AnswerType.Single)
        {
            
            foreach (var answer in PickedAnswers)
            {
                if (answer != newAnswer)
                {
                    answer.Reset();
                }
            }
            
            PickedAnswers.Clear();
            PickedAnswers.Add(newAnswer);
            GameObject.Find("InputField").GetComponent<Autocomplete>().inputField.text = "";
            GameObject.Find("Managers").GetComponent<UIManager>().ShowPickedAnswers(PickedAnswers);
        }
        else
        */
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
    }


    public void EraseAnswers()
    {
        PickedAnswers = new List<AnswerData>();
    }


    void Display()
    {
        GameObject.Find("Managers").GetComponent<UIManager>().ErasePickedAnswers(PickedAnswers);
        EraseAnswers();
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
        bool isCorrect = CheckAnswers();
        FinishedQuestions.Add(currentQuestion);

        UpdateScore((isCorrect) ? questions[currentQuestion].AddScore : -questions[currentQuestion].AddScore);

        if (IsFinished)
        {
            //carica il livello successivo quando finisci quello che stai facendo
            events.level++;
            if (events.level > GameEvents.maxlevel)
            {
                events.level = 1;
            }
            //
            SetHighScore();
        }


        var type = (IsFinished) ? UIManager.ResolutionScreenType.Finish : (isCorrect) ? UIManager.ResolutionScreenType.Correct : UIManager.ResolutionScreenType.Incorrect;

        if (events.DisplayResolutionScreen != null)
        {
            events.DisplayResolutionScreen(type, questions[currentQuestion].AddScore);
        }

        AudioManager.Instance.PlaySound((isCorrect) ? "CorrectSFX" : "IncorrectSFX");

        if (type != UIManager.ResolutionScreenType.Finish)
        {
            if (IE_WaitTillNextRound != null)
            {
                StopCoroutine(IE_WaitTillNextRound);
            }
            IE_WaitTillNextRound = WaitTillNextRound();
            StartCoroutine(IE_WaitTillNextRound);

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


    IEnumerator WaitTillNextRound()
    {
        yield return new WaitForSeconds(GameObject.Find("Managers").GetComponent<UIManager>().ResolutionDelayTime);
        Display();
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

    bool CheckAnswers()
    {
        if (!CompareAnswers())
        {
            return false;
        }
        return true;
    }

    /*
        bool CompareAnswersOLD()
        {
            if (PickedAnswers.Count > 0)
            {
                List<int> c  = data.Questions[currentQuestion].GetCorrectAnswers();
                List<int> p = PickedAnswers.Select(x => x.AnswerIndex).ToList();


                foreach( var x in p) {
                Debug.Log( x);}

                var f = c.Except(p).ToList();
                var s = p.Except(c).ToList();

                return !f.Any() && !s.Any();       
            }
            return false;

        }
    */

    bool CompareAnswers()
    {
        if (PickedAnswers.Count > 0)
        {
            List<string> c = questions[currentQuestion].GetCorrectAnswers();
            List<string> p = PickedAnswers.Select(x => x.infoTextObject.text).ToList();

            /*
            Debug.Log("Risposte giuste");
            foreach( var x in p) {
            Debug.Log( x);}
            Debug.Log("Risposte selezionate");
            foreach( var x in c) {
            Debug.Log( x);}
            */

            var f = c.Except(p).ToList();
            var s = p.Except(c).ToList();

            return !f.Any() && !s.Any();
        }
        return false;
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
