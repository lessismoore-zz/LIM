using LessIsMoore.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LessIsMoore.Web.Controllers
{
    public class ReportController : BaseController
    {
        private IHostingEnvironment _env;
        private AppSettings _AppSettings;
        private IHttpContextAccessor _context;

        public ReportController(IHostingEnvironment env, IOptions<AppSettings> settings, IHttpContextAccessor context = null) 
        {
            this._env = env;
            _AppSettings = settings != null ? settings.Value : null;
            _context = context;
        }

        [Route("[Controller]/Exam")]
        public FileContentResult Index()
        {
            LIM.Exam.Models.Exam azureExam = JsonConvert.DeserializeObject<LIM.Exam.Models.Exam>(_context.HttpContext.Session.GetString("ExamReport"));

            if (azureExam == null)
            {
                throw new Exception("Exam not Found");
            }

            //_context.HttpContext.Session.Remove("ExamReport");

            XDocument xdocument = XDocument.Load(Path.Combine(this._env.ContentRootPath, "app_data\\Reports.xml"));
            XElement xElement = xdocument.Root.Elements().Where(x => x.Attribute("id").Value == azureExam.ID.ToString()).FirstOrDefault();

            if (xElement == null) {
                throw new Exception("Report not Found");
            }

            string strReportPath = xElement.Attribute("path").Value;
            string strReportTitle = xElement.Attribute("title").Value;

            LocalReport rpt = new LocalReport()
            {
                ReportPath = Path.Combine(this._env.ContentRootPath, strReportPath),
                DisplayName = strReportTitle
            };

            rpt.SetParameters(new ReportParameter() { Name = "Name", Values = { azureExam.TakerName } });

            string strFileName = (azureExam.TakerName + " " +rpt.DisplayName).Replace(" ", ".").Replace("/", ".") + ".pdf";

            byte[] b = rpt.Render("pdf");

            return File(b, "application / pdf", strFileName);
        }

    }
}