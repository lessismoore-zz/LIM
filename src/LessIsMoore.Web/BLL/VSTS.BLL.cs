using LessIsMoore.Web.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace LessIsMoore.Web.BLL
{
    public class VSTS
    {
        private string _personalAccessToken;
       
        public VSTS(string personalAccessToken)
        {
            _personalAccessToken = personalAccessToken;
        }

        public Int32 SaveWorkItem(VSTSWorkItem workItem)
        {

            string _credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken)));

            Object[] objWorkItem = new Object[5];

            objWorkItem[0] = new { op = "add", path = "/fields/System.Title", value = workItem.Title };
            objWorkItem[1] = new { op = "add", path = "/fields/Microsoft.VSTS.TCM.ReproSteps", value = workItem.Steps };
            objWorkItem[2] = new { op = "add", path = "/fields/Microsoft.VSTS.Common.Priority", value = "1" };
            objWorkItem[3] = new { op = "add", path = "/fields/Microsoft.VSTS.Common.Severity", value = "2 - High" };
            objWorkItem[4] = new { op = "add", path = "/fields/System.History", value = workItem.Comments };

            //use the httpclient
            using (var client = new HttpClient())
            {
                //set our headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                //serialize the fields array into a json string
                string strURL = "https://lessismoore.visualstudio.com/";

                if (workItem.id > -1)
                    strURL += string.Format("DefaultCollection/_apis/wit/workitems/{0}?api-version=2.2", workItem.id.ToString());
                else
                    strURL += string.Format("LessIsMoore.Web/_apis/wit/workitems/${0}?api-version=2.2", workItem.Type);


                var request = new HttpRequestMessage(new HttpMethod("PATCH"), strURL) {
                    Content = new StringContent(JsonConvert.SerializeObject(objWorkItem), Encoding.UTF8, "application/json-patch+json")
                };

                var response = client.SendAsync(request).Result;

                //if the response is successfull, set the result to the workitem object
                if (response.IsSuccessStatusCode)
                {
                    VSTSWorkItem returnedWorkItem =
                        JsonConvert.DeserializeObject<VSTSWorkItem>(response.Content.ReadAsStringAsync().Result);

                    return returnedWorkItem.id;
                }

                return -1;
            }
        }

    }
}