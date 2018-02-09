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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

namespace LessIsMoore.Web.Controllers
{
    public class ExamController : BaseController
    {
        private IHostingEnvironment _env;
        private AppSettings _AppSettings;
        private IHttpContextAccessor _context;

        public ExamController(IHostingEnvironment env, IOptions<AppSettings> settings, IHttpContextAccessor context = null)
        {
            this._env = env;
            _AppSettings = settings != null ? settings.Value : null;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(int ID)
        {
            _context.HttpContext.Session.Remove("AzureExam");
            Exam azureExam = new BLL(this._env.ContentRootPath).ExamFactory(ID);

            //if (_context.HttpContext.Request.Query["sf"] == "8d679ae7-e939-474c-a3ff-8501ee636b12")
            //{
            //    azureExam.ShuffleQuestions = false;
            //    azureExam.ShuffleQuestionChoices = false;
            //}

            _context.HttpContext.Session.SetString("AzureExam", JsonConvert.SerializeObject(azureExam));
            _context.HttpContext.Session.SetString("AzureExamStartTime", DateTime.Now.ToShortTimeString() );

            return View(azureExam);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(string txtEmail, string txtName)
        {
            Exam azureExam = JsonConvert.DeserializeObject<Exam>(_context.HttpContext.Session.GetString("AzureExam"));

            azureExam.HasStarted = true;
            azureExam.TakerEmail = System.Net.WebUtility.HtmlEncode(txtEmail);
            azureExam.TakerName = System.Net.WebUtility.HtmlEncode(txtName);

            IEnumerable<ExamResponse> rsps =
                BLL.CollectExamResponses(azureExam, x => (Request.Form["answer_" + x.ID.ToString()] == "on"));

            int intTotalCorrectQuestions = BLL.GradeExam(azureExam, rsps);
            TempData["TotalCorrectQuestions"] = intTotalCorrectQuestions;

            //========================================================
            DateTime dteAzureExamStartTime;
            DateTime.TryParse(_context.HttpContext.Session.GetString("AzureExamStartTime"), out dteAzureExamStartTime);

            TelemetryClient appInsights = new TelemetryClient();
            Dictionary<string, string> properties = new Dictionary<string, string>();
            var metrics = new Dictionary<string, double>();

            properties["Name: LessIsMoore Exam"] = azureExam.Name;
            metrics["Score: LessIsMoore Exam"] = intTotalCorrectQuestions;
            metrics["Duration(min): LessIsMoore Exam"] = (DateTime.Now - dteAzureExamStartTime).TotalMinutes;

            appInsights.TrackEvent("LessIsMoore Exam", properties, metrics);
            //========================================================

            if (intTotalCorrectQuestions < azureExam.TotalQuestions)
                return View("Index", azureExam);

            //report/exam
            _context.HttpContext.Session.SetString("ExamReport", JsonConvert.SerializeObject(azureExam));
            await SendCertificationEmail(azureExam);

            this.TempData["HasPassed"] = "YES";
            this.TempData["Name"] = azureExam.TakerName;
            this.TempData["Email"] = azureExam.TakerEmail;

            return RedirectToAction("Index", new { ID = azureExam.ID });
        }

        private async Task SendCertificationEmail(Exam azureExam)
        {
            await new Core.Models.SendGrid(_AppSettings.SendGridSettings).SendEmailAsync(
                            "moore.tim@microsoft.com",
                            "Certification Request",
                            azureExam.Name,
                            string.Format("New Request from {0} at Email: {1}", azureExam.TakerName, azureExam.TakerEmail)
                        );
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}