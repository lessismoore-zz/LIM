using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.SendGrid
{
    public class SendGridContent
    {
        public string type { get; set; }
        public string value { get; set; }

        public SendGridContent() { }

        public SendGridContent(string type, string content)
        {
            this.type = type;
            value = content;
        }
    }
}
