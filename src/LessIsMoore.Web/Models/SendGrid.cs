using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;

namespace LessIsMoore.Core.Models
{

    public class SendGrid
    {
        private SendGridSettings _settings;

        public SendGrid(SendGridSettings settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string to, string subject, string fromName, string message)
        {
            try
            {
                var data = new SendGridMessage(new SendGridEmail(to), subject, new SendGridEmail(_settings.FromEmail, fromName), message);

                string strJSON = JsonConvert.SerializeObject(data);

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);

                    using (HttpResponseMessage responseMessage =
                        await httpClient.PostAsync(new Uri(_settings.ApiURL), new StringContent(strJSON, System.Text.Encoding.UTF8, "application/json")))
                    {

                        // this is just a rough example of handling errors
                        if (!responseMessage.IsSuccessStatusCode)
                        {
                            // see if we can read the response for more information, then log the error
                            string errorJson = await responseMessage.Content.ReadAsStringAsync();
                            throw new Exception($"SendGrid indicated failure, code: {responseMessage.StatusCode}, reason: {errorJson}");
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                // example using a fictional object "_logger" that can log exceptions for you
                //await _logger.LogExceptionAsync(ex);
            }
        }

    }

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

    public class SendGridPersonalization
    {
        public List<SendGridEmail> to { get; set; }
        public string subject { get; set; }
    }
    public class SendGridSettings
    {
        public string FromEmail { get; set; }
        public string ApiKey { get; set; }
        public string ApiURL { get; set; }
    }

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