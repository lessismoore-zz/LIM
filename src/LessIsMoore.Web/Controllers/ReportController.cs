using LessIsMoore.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace LessIsMoore.Web.Controllers
{
    public class ReportController : BaseController
    {
        private IHostingEnvironment _env;
        private AppSettings _AppSettings;
        private IHttpContextAccessor _context;
        private LIM.Certificates.BLL.Certifcates _reportsBLL;

        public ReportController(IHostingEnvironment env, IOptions<AppSettings> settings, IHttpContextAccessor context = null) 
        {
            this._env = env;
            _AppSettings = settings != null ? settings.Value : null;
            _context = context;
            _reportsBLL = new LIM.Certificates.BLL.Certifcates(env.ContentRootPath);
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

            LIM.Certificates.Models.Certificate inst_report = _reportsBLL.FetchCertificateForExam(azureExam);

            if (inst_report == null)
            {
                throw new Exception("Report not Found");
            }

            return File(inst_report.FileContents, "application / pdf", inst_report.FileName);
        }

        [HttpPost]
        [Route("[Controller]/Certificate")]
        public FileContentResult GenerateCertificate(string txtDelimitedStringNames, int ddlRptID)
        {
            txtDelimitedStringNames = System.Net.WebUtility.HtmlEncode(txtDelimitedStringNames);

            //====================================
            string[] arrNames = txtDelimitedStringNames
                                    .Split(Convert.ToChar(";"))
                                    .Select(x=>x.Trim())
                                    .Where(x=>x.Length > 3)
                                    .ToArray();

            if (arrNames?.Length < 1) {
                throw new Exception("No names submitted");
            }

            byte[] compressedBytes = _reportsBLL.FetchZipOfCertificatesForUsers(ddlRptID, arrNames);

            if (compressedBytes == null)
            {
                throw new Exception("Report not Found");
            }

            return File(compressedBytes, "application/zip", "Certificates.zip");
        }



    }
}