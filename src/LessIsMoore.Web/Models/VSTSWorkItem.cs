using System.Collections.Generic;

namespace LessIsMoore.Web.Models
{
    public class VSTSWorkItem
    {
        public string Title { get; set; }
        public string Steps { get; set; }
        public string Type { get; set; }

        public string Comments { get; set; }


        public int id { get; set; }
        public int rev { get; set; }
        public KeyValuePair<string, string> fields{ get; set; }
    }
}