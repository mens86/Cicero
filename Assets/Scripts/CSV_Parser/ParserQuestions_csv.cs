using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ParserQuestions_csv : MonoBehaviour
{

    private char delimiter = ';';


    public List<Question> ParseQuestionsFile(string fileName)
    {
        List<Question> result = new List<Question>();

        TextAsset txt = (TextAsset)Resources.Load(fileName, typeof(TextAsset));
        Debug.Assert(txt != null, "Il file csv deve essere messo nella cartella Assets/Resources. Il nome del file non deve includere l'estensione");

        string filecontent = txt.text;
        filecontent = filecontent.Replace("\r", "");

        string[] lines = filecontent.Split('\n');
        foreach (var currLine in lines)
        {
            bool isEmptyLine = currLine.Replace(";", "").Length == 0;
            if (isEmptyLine)
            {
                continue;
            }

            var newQuestion = CreateQuestion(currLine);
            result.Add(newQuestion);
        }

        return result;
    }

    private Question CreateQuestion(string currLine)
    {
        Question question = new Question();
        string[] cells = currLine.Split(delimiter);

        question.Info = cells[1];
        question.Answers = CreateAnswers(cells[6]).ToArray();
        if (cells[8] == "sì")
        {
            question.UseTimer = true;
        }
        int timer = Convert.ToInt32(cells[9]);
        question.Timer = timer;
        int score = Convert.ToInt32(cells[10]);
        question.AddScore = score;



        return question;
    }

    private List<Answer> CreateAnswers(string line)
    {
        List<Answer> questionAnswers = new List<Answer>();
        string[] answers = line.Split('|');
        foreach (var currAnswer in answers)
        {
            Answer newAnswer = new Answer();
            newAnswer.Info = currAnswer;
            newAnswer.IsCorrect = true;
            questionAnswers.Add(newAnswer);

            //create the autocomplete list with all the answers
            GameObject.Find("InputField").GetComponent<Autocomplete>().allAnswers.Add(currAnswer);
        }
        return questionAnswers;
    }


}