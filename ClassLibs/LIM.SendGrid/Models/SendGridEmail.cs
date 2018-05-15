namespace LIM.SendGrid.Models
{
    public class SendGridEmail
    {
        public string email { get; set; }
        public string name { get; set; }

        public SendGridEmail() { }

        public SendGridEmail(string email, string name = null)
        {
            this.email = email;
            this.name = name;
        }
    }
}
