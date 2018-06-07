using System.Collections.Generic;
using System.Linq;

namespace LIM.Exam.Models
{
    public class Exam
    {
        public int TotalQuestions
        {
            get
            {
                return this.ExamQuestions.Count();
            }
        }

        public IEnumerable<ExamQuestion> ExamQuestions { get; set; }

        public bool HasStarted { get; set; }

        public string TakerEmail { get; set; }

        public string TakerName { get; set; }

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

        public int ID{ get; set; }
        public string Name{ get; set; }

    }
}
