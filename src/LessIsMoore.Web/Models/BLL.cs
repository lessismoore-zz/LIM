using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace LessIsMoore.Web.Models
{
    public class VSTSWorkItem
    {
        public int id { get; set; }
        public int rev { get; set; }
        public KeyValuePair<string, string> fields{ get; set; }
    }
    public class TranslatorSettings
    {
        public string TokenURL { get; set; }
        public string ClientID { get; set; }
        public string ApiURL { get; set; }
        public string ClientSecret { get; set; }
        public string ApiURLParams { get; set; }
        public string AzureKey { get; set; }
    }

    public class AppSettings
    {
        public Core.Models.SendGridSettings SendGridSettings { get; set; }
        public Models.TranslatorSettings TranslatorSettings { get; set; }
        public string NotificationHubConn { get; set; }
        public string NotificationHubName { get; set; }
        public string VSTSToken { get; set; }
    }


    public class NewsFeed
    {
        public string Headline { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
        public string Source { get; set; }


    }

    public class BLL
    {
        public BLL()
        {

        }
        public BLL(string strContentRootPath)
        {
            _strContentRootPath = strContentRootPath;
        }
        private string _strContentRootPath;

        public Exam ExamFactory(int examID)
        {

            Exam azureExam = new Exam();
            string strXMLPath = null;

            azureExam.ID = examID;

            if (azureExam.ID == 1)
            {
                strXMLPath = "app_data\\AzureQuiz.xml";
                azureExam.Name = "Exam: Fast Start – Azure for Modern Web and Mobile Application Development";
            }
            else if (azureExam.ID == 2)
            {
                strXMLPath = "app_data\\DevopsQuiz.xml";
                azureExam.Name = "Exam: Fast Start – Azure for Dev Ops";
            }
            else
            {
                return null;
            }

            XDocument xdocument = XDocument.Load(Path.Combine(_strContentRootPath, strXMLPath));
            azureExam.ExamQuestions = PopulateExamQuestionsFromXML(xdocument.Root.Elements("question"));

            return azureExam;
        }
        public IEnumerable<ExamQuestion> PopulateExamQuestionsFromXML(IEnumerable<XElement> Qs, bool ShuffleQuestions = true, bool ShuffleQuestionChoices = true)
        {
            Random rdm = new Random();

            return (Qs.OrderBy(x => (ShuffleQuestions ? rdm.Next(10, 100) : 0)).Select((x, intQID) => new ExamQuestion()
            {
                ID = (intQID + 1),
                Text = x.Element("text").Value,
                ExamChoices = (x.Element("answers").Elements("answer").OrderBy(y => (ShuffleQuestionChoices ? rdm.Next(10, 100) : 0)).Select((y, intCID) =>
                {
                    ExamChoice examChoice = new ExamChoice();
                    examChoice.ID = ((intCID + 1) + ((intQID + 1) * 100));
                    examChoice.Text = y.Value;
                    int num = y.Attributes().Any(z => z.Name.LocalName == "correct") ? 1 : 0;
                    examChoice.IsCorrect = num != 0;
                    return examChoice;
                })).ToList()
            })).ToList();

        }
        public static int GradeExam(Exam azureExam, IEnumerable<ExamResponse> examResponses)
        {
            foreach (ExamQuestion examQuestion in azureExam.ExamQuestions)
            {
                foreach (ExamChoice examChoice in examQuestion.ExamChoices)
                    examChoice.IsSelected = examResponses.FirstOrDefault(x => x.ExamChoiceID == examChoice.ID).IsSelected;
            }

            return azureExam.ExamQuestions.Select((x => x.ExamChoices.Where<ExamChoice>((y =>
            {
                return (y.IsSelected) ? y.IsCorrect: false;

            })).Count())).Sum();
        }

        public static IEnumerable<ExamResponse> CollectExamResponses(Exam azureExam, Func<ExamChoice, bool> f)
        {
            List<ExamResponse> lstExamResponses = new List<ExamResponse>();

            foreach (ExamQuestion examQuestion in azureExam.ExamQuestions)
            {
                foreach (ExamChoice examChoice in examQuestion.ExamChoices)
                {
                    lstExamResponses.Add(new ExamResponse()
                    {
                        ExamChoiceID = examChoice.ID,
                        IsSelected = f(examChoice)
                    });
                }
            }

            return lstExamResponses;
        }

        public async System.Threading.Tasks.Task<NewsFeed[]> FetchVergeNewsFeed(int intMaxArticlesDisplayed = 5)
        {
            string strNewsFeed = @"http://www.theverge.com/web/rss/index.xml";
            XDocument xdocFeedXML = null;

            using (var s = new HttpClient()) {
                xdocFeedXML = XDocument.Parse(await s.GetStringAsync(strNewsFeed));
            }

            if (xdocFeedXML != null)
            {
                return xdocFeedXML.Root.Elements()
                    .Where(x => x.Name.LocalName.ToLower() == "entry")
                    .Take(intMaxArticlesDisplayed)
                    .Select(x => new NewsFeed()
                    {
                        Headline = x.Elements().Where(y => y.Name.LocalName == "title").FirstOrDefault().Value,
                        Content = x.Elements().Where(y => y.Name.LocalName == "content").FirstOrDefault().Value,
                        Link = x.Elements().Where(y => y.Name.LocalName == "link").FirstOrDefault().Attribute("href").Value,
                        Source = "TheVerge.com"
                    }).ToArray();
            }

            return null;
        }

        public async System.Threading.Tasks.Task<NewsFeed[]> FetchAzureNewsFeed(int intMaxArticlesDisplayed = 5)
        {
            string strNewsFeed = @"https://azure.microsoft.com/en-us/updates/feed/";
            XDocument xdocFeedXML = null;

            using (var s = new System.Net.Http.HttpClient())
            {
                xdocFeedXML = XDocument.Parse(await s.GetStringAsync(strNewsFeed));
            }

            if (xdocFeedXML != null)
            {
                return xdocFeedXML.Root.Elements("channel").Elements()
                    .Where(x => x.Name.LocalName.ToLower() == "item")
                    .Take(intMaxArticlesDisplayed)
                    .Select(x => new NewsFeed()
                    {
                        Headline = x.Elements().Where(y => y.Name == "title").FirstOrDefault().Value,
                        Content = x.Elements().Where(y => y.Name == "description").FirstOrDefault().Value,
                        Link = x.Elements().Where(y => y.Name == "link").FirstOrDefault().Value,
                        Source = "Azure.Microsoft.com"
                    }).ToArray();
            }

            return null;
        }

        public Int32 VSTS_SaveWorkItem(string strTitle, string strSteps, string Type, int intItemID = -1, string strComments = "")
        {
            string _personalAccessToken = "rpudrl2l7pmpwr6zyqyjiwa4xn6ryexqy3f2sis4powtnxhmkoya";
            string _credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken)));

            Object[] objWorkItem = new Object[5];

            objWorkItem[0] = new { op = "add", path = "/fields/System.Title", value = strTitle };
            objWorkItem[1] = new { op = "add", path = "/fields/Microsoft.VSTS.TCM.ReproSteps", value = strSteps };
            objWorkItem[2] = new { op = "add", path = "/fields/Microsoft.VSTS.Common.Priority", value = "1" };
            objWorkItem[3] = new { op = "add", path = "/fields/Microsoft.VSTS.Common.Severity", value = "2 - High" };
            objWorkItem[4] = new { op = "add", path = "/fields/System.History", value = strComments };

            //use the httpclient
            using (var client = new HttpClient())
            {
                //set our headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                //serialize the fields array into a json string
                string strURL = "https://lessismoore.visualstudio.com/";

                if (intItemID > -1)
                    strURL += string.Format("DefaultCollection/_apis/wit/workitems/{0}?api-version=2.2", intItemID.ToString());
                else
                    strURL += string.Format("LessIsMoore.Web/_apis/wit/workitems/${0}?api-version=2.2", Type);


                var request = new HttpRequestMessage(new HttpMethod("PATCH"), strURL) {
                    Content = new StringContent(JsonConvert.SerializeObject(objWorkItem), Encoding.UTF8, "application/json-patch+json")
                };

                var response = client.SendAsync(request).Result;

                //if the response is successfull, set the result to the workitem object
                if (response.IsSuccessStatusCode)
                {
                    VSTSWorkItem workItem =
                        JsonConvert.DeserializeObject<VSTSWorkItem>(response.Content.ReadAsStringAsync().Result);

                    return workItem.id;
                }

                return -1;
            }
        }

    }
}