using LIM.SendGrid.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LIM.SendGrid
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

}
