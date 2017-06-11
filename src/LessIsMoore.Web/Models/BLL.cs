using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LessIsMoore.Web.Models
{
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
        public Core.Models.SendGridSettings SendEmailSettings { get; set; }
        public Models.TranslatorSettings TranslatorSettings { get; set; }
        public string NotificationHubConn { get; set; }
        public string NotificationHubName { get; set; }

    }


    public class NewsFeed
    {
        public string Headline { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }

    }

    public class BLL
    {
        public async System.Threading.Tasks.Task<NewsFeed[]> FetchVergeNewsFeed(int intMaxArticlesDisplayed = 5)
        {
            string strNewsFeed = @"http://www.theverge.com/web/rss/index.xml";
            XDocument xdocFeedXML = null;

            using (var s = new System.Net.Http.HttpClient()) {
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
                        Link = x.Elements().Where(y => y.Name.LocalName == "link").FirstOrDefault().Attribute("href").Value
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
                        Link = x.Elements().Where(y => y.Name == "link").FirstOrDefault().Value
                    }).ToArray();
            }

            return null;
        }


    }
}