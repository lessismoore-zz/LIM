using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LIM.Exam.Models
{
    public class ExamQuestion
    {
        public int ID { get; set; }

        public string Text { get; set; }

        public IEnumerable<ExamChoice> ExamChoices { get; set; }
    }
}
