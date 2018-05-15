using System;
using System.Linq;
using LessIsMoore.Web.Models;
using System.Xml.Linq;
//using StackExchange.Profiling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using LIM.TextTranslator;
using Microsoft.Reporting.WebForms;

namespace LessIsMoore.Web.Controllers
{
    public class HomeController : BaseController
    {
        ITextTranslator _tl;
        IHostingEnvironment _env;
        IMemoryCache _memoryCache;
        AppSettings _AppSettings;

        public HomeController(IMemoryCache memoryCache = null, IHostingEnvironment env = null, IHttpContextAccessor context = null,
                                ITextTranslator _TextTranslator = null, IOptions<AppSettings> settings = null) : base(context, _TextTranslator, settings)
        {
            _env = env;
            _tl = _TextTranslator;
            _memoryCache = memoryCache;
            _AppSettings = settings != null ? settings.Value : null;
        }

        public void DoSomething() {
            int intNumber = 1;

        }

        // GET: Home
        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
            //bool b = User.IsInRole("Admin");

            NewsFeed[] arrNewsFeeds = null;

            if (_memoryCache != null) {
                if (!_memoryCache.TryGetValue<NewsFeed[]>("arrNewsFeeds", out arrNewsFeeds))
                {
                    BLL inst_BLL = new BLL(_env.ContentRootPath);

                    arrNewsFeeds = inst_BLL.FetchCustomNewsFeed(2);
                    arrNewsFeeds = arrNewsFeeds.Concat(await inst_BLL.FetchAzureNewsFeed(2)).ToArray();
                    //arrNewsFeeds = arrNewsFeeds.Concat(await inst_BLL.FetchVergeNewsFeed(2)).ToArray();

                    _memoryCache.Set<NewsFeed[]>("arrNewsFeeds", arrNewsFeeds, TimeSpan.FromMinutes(20));
                }
            }

            ViewData["title"] = "Home";

            return View(arrNewsFeeds);
        }

        [Authorize]
        public IActionResult Workout()
        {
            return View();
        }

        public IActionResult Demo()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SaveWorkout(string type, string excercise, int wovalue, DateTime wodate)
        {
            string strXMLPath = System.IO.Path.Combine(_env.ContentRootPath, @"\App_Data\workouts.xml");

            XDocument xDoc = XDocument.Load(strXMLPath);
            xDoc.Root.Add(new XElement("workout",
                    new XElement("type", type),
                    new XElement("excercise", excercise),
                    new XElement("value", wovalue),
                    new XElement("date", wodate.ToString())
                ));

            using (var fileStream = System.IO.File.Create(strXMLPath)) {
                xDoc.Save(fileStream);
            }

            return new JsonResult(xDoc);
        }

        [HttpGet]
        public JsonResult GetWorkouts()
        {
            string strXMLPath = System.IO.Path.Combine(_env.ContentRootPath, @"App_Data\workouts.xml");

            XDocument xDoc = XDocument.Load(strXMLPath);

            var data = 
                xDoc.Root.Elements("workout").Select(x => new {
                    type = _tl.TranslateText(x.Element("type").Value),
                    excercise = _tl.TranslateText(x.Element("excercise").Value),
                    value = Convert.ToInt32(x.Element("value").Value),
                    date = Convert.ToDateTime(x.Element("date").Value)
                }).ToArray();

            return new JsonResult(data);
        }


    }
}