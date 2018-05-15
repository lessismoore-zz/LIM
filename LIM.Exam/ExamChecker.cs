using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using LIM.Exam.Models;

namespace LIM.Exam
{
    public class ExamChecker
    {

        public IEnumerable<ExamQuestion> PopulateExamQuestionsFromXML(IEnumerable<XElement> Qs, bool ShuffleQuestions = true, bool ShuffleQuestionChoices = true)
        {
            Random rdm = new Random();

            return (Qs.OrderBy(x => (ShuffleQuestions ? rdm.Next(10, 100) : 0)).Select((x, intQID) => new ExamQuestion()
            {
                ID = (intQID + 1),
                Text = x.Element("text").Value,
                ExamChoices = (x.Element("answers").Elements("answer").OrderBy(y => (ShuffleQuestionChoices ? rdm.Next(10, 100) : 0)).Select((y, intCID) =>
                {
                    ExamChoice examChoice = new ExamChoice();
                    examChoice.ID = ((intCID + 1) + ((intQID + 1) * 100));
                    examChoice.Text = y.Value;
                    int num = y.Attributes().Any(z => z.Name.LocalName == "correct") ? 1 : 0;
                    examChoice.IsCorrect = num != 0;
                    return examChoice;
                })).ToList()
            })).ToList();

        }
        public static int GradeExam(Models.Exam azureExam, IEnumerable<ExamResponse> examResponses)
        {
            foreach (ExamQuestion examQuestion in azureExam.ExamQuestions)
            {
                foreach (ExamChoice examChoice in examQuestion.ExamChoices)
                    examChoice.IsSelected = examResponses.FirstOrDefault(x => x.ExamChoiceID == examChoice.ID).IsSelected;
            }

            return azureExam.ExamQuestions.Select((x => x.ExamChoices.Where<ExamChoice>((y =>
            {
                return (y.IsSelected) ? y.IsCorrect : false;

            })).Count())).Sum();
        }

        public static IEnumerable<ExamResponse> CollectExamResponses(Models.Exam azureExam, Func<ExamChoice, bool> f)
        {
            List<ExamResponse> lstExamResponses = new List<ExamResponse>();

            foreach (ExamQuestion examQuestion in azureExam.ExamQuestions)
            {
                foreach (ExamChoice examChoice in examQuestion.ExamChoices)
                {
                    lstExamResponses.Add(new ExamResponse()
                    {
                        ExamChoiceID = examChoice.ID,
                        IsSelected = f(examChoice)
                    });
                }
            }

            return lstExamResponses;
        }

    }
}
