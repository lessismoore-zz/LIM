using System.Linq;
using System.Xml.Linq;

namespace LessIsMoore.Net.Models
{
    public class TranslatorSettings
    {
        public string TokenURL { get; set; }
        public string ClientID { get; set; }
        public string ApiURL { get; set; }
        public string ClientSecret { get; set; }
        public string ApiURLParams { get; set; }
    }

    public class AppSettings
    {
        public Core.Models.SendGridSettings SendEmailSettings { get; set; }
        public Net.Models.TranslatorSettings TranslatorSettings { get; set; }
    }

    public class NewsArticle
    {
        public string Headline { get; set; }
        public string Content { get; set; }
    }

    public class BLL
    {
        public async System.Threading.Tasks.Task<NewsArticle[]> FetchNewsArcticles()
        {
            string strVergeRSSFeed = @"http://www.theverge.com/web/rss/index.xml";
            XDocument xdocVergeXML = null;
            int _intMaxArticlesDisplayed = 5;

            using (var s = new System.Net.Http.HttpClient()) {
                xdocVergeXML = XDocument.Parse(await s.GetStringAsync(strVergeRSSFeed));
            }

            if (xdocVergeXML != null)
            {
                return xdocVergeXML.Root.Elements()
                    .Where(x => x.Name.LocalName.ToLower() == "entry")
                    .Take(_intMaxArticlesDisplayed)
                    .Select(x => new NewsArticle()
                    {
                        Headline = x.Elements().Where(y => y.Name.LocalName == "title").FirstOrDefault().Value,
                        Content = x.Elements().Where(y => y.Name.LocalName == "content").FirstOrDefault().Value
                    }).ToArray();
            }

            return null;
        }


    }
}