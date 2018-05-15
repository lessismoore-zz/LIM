namespace LIM.SendGrid.Models
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
