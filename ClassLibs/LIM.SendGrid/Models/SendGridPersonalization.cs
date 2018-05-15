using System.Collections.Generic;

namespace LIM.SendGrid.Models
{
    public class SendGridPersonalization
    {
        public List<SendGridEmail> to { get; set; }
        public string subject { get; set; }
    }
}
