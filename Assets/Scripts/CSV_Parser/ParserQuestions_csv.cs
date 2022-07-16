using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ParserQuestions_csv : MonoBehaviour
{

    public Autocomplete autocomplete;
    private char delimiter = ';';


    //TODO: this gets a list of string, where the arg is the actual file content, not the file name
    public List<Question> ParseQuestionsFile(List<TextAsset> decks_textAssets)
    {
        List<Question> result = new List<Question>();
        for (int i = 0; i < decks_textAssets.Count; i++)
        {
            TextAsset txt = decks_textAssets[i];
            Debug.Assert(txt != null, "Il file csv deve essere messo nella cartella Assets/Mazzi CSV. Il nome del file non deve includere l'estensione");

            string filecontent = txt.text;
            filecontent = filecontent.Replace("\r", "");

            string[] lines = filecontent.Split('\n');
            for (int u = 2; u < lines.Length; u++)
            {

                Question parsedQuestion = ParseQuestion(lines[u]);
                if (parsedQuestion != null)
                {
                    parsedQuestion.question_filename = txt.name;
                    result.Add(parsedQuestion);
                }


            }

        }

        return result;
    }

    private Question ParseQuestion(string txt)
    {
        bool isEmptyLine = txt.Replace(";", "").Length == 0;
        if (isEmptyLine)
        {
            return null;
        }

        var newQuestion = CreateQuestion(txt);
        return newQuestion;

    }

    private Question CreateQuestion(string currLine)
    {
        string[] cells = currLine.Split(delimiter);
        Question question = null;

        question = new Question();
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

            List<string> synonyms = currAnswer.Split('$').ToList();
            newAnswer.groupOfSynonyms = synonyms;
            questionAnswers.Add(newAnswer);


            //create the autocomplete list with all the answers (with all synonyms)
            /*
            foreach (var synonym in synonyms)
            {
                autocomplete.allAnswers.Add(synonym);
            }
            */
        }
        return questionAnswers;
    }


}