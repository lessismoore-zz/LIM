using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.SendGrid
{
    public class SendGridSettings
    {
        public string FromEmail { get; set; }
        public string ApiKey { get; set; }
        public string ApiURL { get; set; }
    }

}
