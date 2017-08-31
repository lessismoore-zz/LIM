using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LessIsMoore.Web.Models
{
    public class Exam
    {
        public int TotalQuestions
        {
            get
            {
                return this.ExamQuestions.Count;
            }
        }

        public int TotalCorrectQuestions
        {
            get
            {
                return this.ExamQuestions.Select((x => x.ExamChoices.Where<ExamChoice>((y =>
                {
                    if (y.IsSelected)
                        return y.IsCorrect;
                    return false;
                })).Count<ExamChoice>())).Sum();
            }
        }

        public IList<ExamQuestion> ExamQuestions { get; set; }

        public bool HasStarted { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        private bool _ShuffleQuestions = true;
        public bool ShuffleQuestions {
            get { return _ShuffleQuestions; }
            set { _ShuffleQuestions = value; }
        }

        private bool _ShuffleQuestionChoices = true;
        public bool ShuffleQuestionChoices
        {
            get { return _ShuffleQuestionChoices; }
            set { _ShuffleQuestionChoices = value; }
        }

        public int QuizID{ get; set; }

    }
}
