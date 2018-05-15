using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.SendGrid
{
    public class SendGridPersonalization
    {
        public List<SendGridEmail> to { get; set; }
        public string subject { get; set; }
    }
}
