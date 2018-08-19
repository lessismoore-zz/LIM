using LessIsMoore.Web.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;

namespace LessIsMoore.Web.BLL
{
    public class VSTS
    {
        private string _personalAccessToken;
        private string _strRepoPath;

        public VSTS(string personalAccessToken, string repoPath)
        {
            _personalAccessToken = personalAccessToken;
            _strRepoPath = repoPath;
        }

        public VSTSWorkItem SaveWorkItem(VSTSWorkItem workItem)
        {
            return SaveWorkItems(new VSTSWorkItem[] { workItem }).FirstOrDefault();
        }

        public IEnumerable<VSTSWorkItem> SaveWorkItems(IEnumerable<VSTSWorkItem> workItems)
        {
            string _credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken)));

            List<VSTSWorkItem> lstWorkItems = new List<VSTSWorkItem>();
                        
            //use the httpclient
            using (var client = new HttpClient())
            {
                //set our headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                //=================================================
                foreach (VSTSWorkItem workItem in workItems)
                {
                    Object[] objWorkItem = new Object[]
                    {
                        new { op = "add", path = "/fields/System.Title", value = workItem.Title },
                        new { op = "add", path = "/fields/System.Description", value = workItem.Comments },
                        new { op = "add", path = "/fields/Microsoft.VSTS.TCM.ReproSteps", value = workItem.Steps },
                        new { op = "add", path = "/fields/Microsoft.VSTS.Common.Priority", value = workItem.Priority },
                        new { op = "add", path = "/fields/Microsoft.VSTS.Common.Severity", value = "2 - High" },
                        new { op = "add", path = "/fields/System.History", value = DateTime.Now.ToString() }
                    };

                    //serialize the fields array into a json string
                    string strURL = "";

                    if ((workItems.Count() == 1) && (workItems.First().id > -1)) //-1 is a new entry
                        strURL = string.Format("DefaultCollection/_apis/wit/workitems/{0}?api-version=2.2", workItem.id.ToString());
                    else
                        strURL = string.Format("/{0}/_apis/wit/workitems/${1}?api-version=2.2", workItem.ProjectName, workItem.Type);

                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), _strRepoPath + strURL)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(objWorkItem), Encoding.UTF8, "application/json-patch+json")
                    };

                    var response = client.SendAsync(request).Result;
                    
                    //if the response is successfull, set the result to the workitem object
                    if (response.IsSuccessStatusCode)
                    {
                        lstWorkItems.Add(JsonConvert.DeserializeObject<VSTSWorkItem>(response.Content.ReadAsStringAsync().Result));
                    }

                }
                //=================================================

                return lstWorkItems;
            }
        }

    }
}