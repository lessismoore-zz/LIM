namespace LIM.Exam.Models
{
    public class ExamChoice
    {
        public int ID { get; set; }

        public string Text { get; set; }

        public bool IsCorrect { get; set; }

        public bool IsSelected { get; set; }
    }
}
