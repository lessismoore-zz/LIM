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

        public IActionResult Index()
        {
            _context.HttpContext.Session.Remove("AzureExam");
            XDocument xdocument = XDocument.Load(Path.Combine(this._env.ContentRootPath, "app_data\\AzureQuiz.xml"));
            Random rdm = new Random();
            Exam azureExam = new Exam();

            var Qs = xdocument.Root.Elements("question");

            if (_context.HttpContext.Request.Query["sf"] == "8d679ae7-e939-474c-a3ff-8501ee636b12")
            {
                azureExam.ShuffleQuestions = false;
                azureExam.ShuffleQuestionChoices = false;
            }

            azureExam.ExamQuestions = (Qs.OrderBy(x => (azureExam.ShuffleQuestions ? rdm.Next(10, 100) : 0)).Select((x, intQID)=> new ExamQuestion()
            {
                ID = (intQID + 1),
                Text = x.Element("text").Value,
                ExamChoices = (x.Element("answers").Elements("answer").OrderBy(y => (azureExam.ShuffleQuestionChoices ? rdm.Next(10, 100): 0)).Select((y, intCID) =>
                {
                    ExamChoice examChoice = new ExamChoice();
                    examChoice.ID = ((intCID + 1 ) + ((intQID + 1) * 100));
                    examChoice.Text = y.Value;
                    int num = y.Attributes().Any(z => z.Name.LocalName == "correct") ? 1 : 0;
                    examChoice.IsCorrect = num != 0;
                    return examChoice;
                })).ToList()
            })).ToList();

            _context.HttpContext.Session.SetString("AzureExam", JsonConvert.SerializeObject(azureExam));

            return  View(azureExam);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(string txtEmail, string txtName)
        {
            Exam azureExam = JsonConvert.DeserializeObject<Exam>(_context.HttpContext.Session.GetString("AzureExam"));
            azureExam.HasStarted = true;
            azureExam.Email = txtEmail;
            azureExam.Name = txtName;

            foreach (ExamQuestion examQuestion in azureExam.ExamQuestions)
            {
                foreach (ExamChoice examChoice in examQuestion.ExamChoices)
                    examChoice.IsSelected = this.Request.Form["answer_" + examChoice.ID.ToString()] == "on";
            }

            if (azureExam.TotalCorrectQuestions < azureExam.TotalQuestions)
                return View("Index", azureExam);

            await new Core.Models.SendGrid(_AppSettings.SendGridSettings).SendEmailAsync(
                "moore.tim@microsoft.com",
                "Workshop Certification Request",
                azureExam.Name,
                string.Format("New Request from {0} at Email: {1}", azureExam.Name, azureExam.Email)
            );

            this.TempData["HasPassed"] = "YES";
            this.TempData["Name"] = azureExam.Name;
            this.TempData["Email"] = azureExam.Email;

            return RedirectToAction("Index");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}