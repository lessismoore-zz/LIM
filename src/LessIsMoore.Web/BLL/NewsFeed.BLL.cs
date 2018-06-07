using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;

namespace LessIsMoore.Web.BLL
{
    //Reports is used to generate SSRS reports (PDF, Zip, etc.)
    public class NewsFeed
    {
        private string _strCustomNewsPath = @"\App_Data\NewsFeed.xml";
        private string _strVergeNewsPath = @"http://www.theverge.com/web/rss/index.xml";
        private string _strAzureNewsPath = @"https://azure.microsoft.com/en-us/updates/feed/";

        private IHostingEnvironment _env;
        //private AppSettings _AppSettings;
        //private IHttpContextAccessor _context;

        public NewsFeed()
        {

        }
        public NewsFeed(IHostingEnvironment env)
        {
            _env = env;
        }

        public Models.NewsFeed[] FetchCustomNewsFeed(int intMaxArticlesDisplayed = 5)
        {
            XDocument xdocFeedXML = XDocument.Load(_env.ContentRootPath + _strCustomNewsPath);

            if (xdocFeedXML != null)
            {
                return xdocFeedXML.Root.Elements()
                    //.Where(x => x.Name.LocalName.ToLower() == "entry")
                    .Take(intMaxArticlesDisplayed)
                    .Select(x => new Models.NewsFeed()
                    {
                        Headline = x.Elements().Where(y => y.Name.LocalName == "title").FirstOrDefault().Value,
                        Content = x.Elements().Where(y => y.Name.LocalName == "content").FirstOrDefault().Value,
                        Link = x.Elements().Where(y => y.Name.LocalName == "link").FirstOrDefault().Attribute("href").Value,
                        Source = "LessIsMoore.Net"
                    }).ToArray();
            }

            return null;
        }
        public async System.Threading.Tasks.Task<Models.NewsFeed[]> FetchVergeNewsFeed(int intMaxArticlesDisplayed = 5)
        {
            XDocument xdocFeedXML = null;

            using (var s = new HttpClient())
            {
                xdocFeedXML = XDocument.Parse(await s.GetStringAsync(_strVergeNewsPath));
            }

            if (xdocFeedXML != null)
            {
                return xdocFeedXML.Root.Elements()
                    .Where(x => x.Name.LocalName.ToLower() == "entry")
                    .Take(intMaxArticlesDisplayed)
                    .Select(x => new Models.NewsFeed()
                    {
                        Headline = x.Elements().Where(y => y.Name.LocalName == "title").FirstOrDefault().Value,
                        Content = x.Elements().Where(y => y.Name.LocalName == "content").FirstOrDefault().Value,
                        Link = x.Elements().Where(y => y.Name.LocalName == "link").FirstOrDefault().Attribute("href").Value,
                        Source = "TheVerge.com"
                    }).ToArray();
            }

            return null;
        }

        public async System.Threading.Tasks.Task<Models.NewsFeed[]> FetchAzureNewsFeed(int intMaxArticlesDisplayed = 5)
        {
            XDocument xdocFeedXML = null;

            using (var s = new System.Net.Http.HttpClient())
            {
                xdocFeedXML = XDocument.Parse(await s.GetStringAsync(_strAzureNewsPath));
            }
            if (xdocFeedXML != null)
            {
                return xdocFeedXML.Root.Elements("channel").Elements()
                    .Where(x => x.Name.LocalName.ToLower() == "item")
                    .Take(intMaxArticlesDisplayed)
                    .Select(x => new Models.NewsFeed()
                    {
                        Headline = x.Elements().Where(y => y.Name == "title").FirstOrDefault().Value,
                        Content = x.Elements().Where(y => y.Name == "description").FirstOrDefault().Value,
                        Link = x.Elements().Where(y => y.Name == "link").FirstOrDefault().Value,
                        Source = x.Elements().Where(y => y.Name == "link").FirstOrDefault().Value
                    }).ToArray();
            }

            return null;
        }

    } 
}