using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ParserQuestions_csv : MonoBehaviour
{
    public string fileName;
    public char delimiter = '$';

    [ContextMenu("test")]
    public void Test()
    {
        ParseQuestionsFile(fileName);
    }

    public List<Question> ParseQuestionsFile(string fileName)
    {
        TextAsset txt = (TextAsset)Resources.Load(fileName, typeof(TextAsset));
        string filecontent = txt.text;

        Question question = new Question();
        string[] lines = filecontent.Split('\n');
        string currLine = lines[16];

        string[] cells = currLine.Split(delimiter);
        question.Info = cells[1];

        question.Answers = CreateAnswers(cells[6]).ToArray();

        return null;
    }

    private List<Answer> CreateAnswers(string line)
    {
        List<Answer> questionAnswers = new List<Answer>();
        string[] answers = line.Split(',');
        foreach (var currAnswer in answers)
        {
            Answer newAnswer = new Answer();
            newAnswer.Info = currAnswer;
            newAnswer.IsCorrect = true;
            questionAnswers.Add(newAnswer);
        }
        return questionAnswers;
    }


}