using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.SendGrid
{

    public class SendGridMessage
    {
        public const string TYPE_TEXT = "text";
        public const string TYPE_HTML = "text/html";

        public List<SendGridPersonalization> personalizations { get; set; }
        public SendGridEmail from { get; set; }
        public List<SendGridContent> content { get; set; }

        public SendGridMessage() { }

        public SendGridMessage(SendGridEmail to, string subject, SendGridEmail from, string message, string type = TYPE_TEXT)
        {
            personalizations = new List<SendGridPersonalization> { new SendGridPersonalization { to = new List<SendGridEmail> { to }, subject = subject } };
            this.from = from;
            content = new List<SendGridContent> { new SendGridContent(type, message) };
        }
    }
   
}
