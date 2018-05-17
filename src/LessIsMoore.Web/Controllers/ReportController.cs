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

            Models.Report inst_report = FetchReportByID(azureExam.ID);

            if (inst_report == null) {
                throw new Exception("Report not Found");
            }

            inst_report.Path = Path.Combine(this._env.ContentRootPath, inst_report.Path);

            using (LocalReport rpt = new LocalReport()
            {
                ReportPath = inst_report.Path,
                DisplayName = inst_report.Title
            })
            {
                rpt.SetParameters(new ReportParameter() { Name = "Name", Values = { azureExam.TakerName } });

                string strFileName = (azureExam.TakerName + " " + rpt.DisplayName).Replace(" ", ".").Replace("/", ".") + ".pdf";

                byte[] b = rpt.Render("pdf");

                return File(b, "application / pdf", strFileName);
            }

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

            Models.Report inst_report = FetchReportByID(ddlRptID);

            if (inst_report == null)
            {
                throw new Exception("Report not Found");
            }

            inst_report.Path = Path.Combine(this._env.ContentRootPath, inst_report.Path);

            //====================================

            byte[] compressedBytes = null;

            using (LocalReport rpt = new LocalReport()
            {
                ReportPath = inst_report.Path,
                DisplayName = inst_report.Title
            })
            using (MemoryStream zipStream = new MemoryStream())
            {
                using (System.IO.Compression.ZipArchive zip = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (string strName in arrNames)
                    {
                        rpt.SetParameters(new ReportParameter() { Name = "Name", Values = { strName } });

                        string strFileName = (strName + " " + rpt.DisplayName).Replace(" ", ".").Replace("/", ".") + ".pdf";

                        System.IO.Compression.ZipArchiveEntry entry = zip.CreateEntry(strFileName.ToLower());

                        using (Stream entryStream = entry.Open())
                        using (MemoryStream rptStream = new MemoryStream(rpt.Render("pdf")))
                        {
                            rptStream.CopyTo(entryStream);
                        }
                    }
                }

                compressedBytes = zipStream.ToArray();
            }

            return File(compressedBytes, "application/zip", "Certificates.zip");
        }

        private Models.Report FetchReportByID(int intRptID)
        {
            XDocument xdocument = XDocument.Load(Path.Combine(this._env.ContentRootPath, "app_data\\Reports.xml"));

            return xdocument.Root.Elements()
                                .Where(x => x.Attribute("id").Value == intRptID.ToString())
                                .Select(x => new Models.Report() { ID = intRptID, Path = x.Attribute("path").Value, Title = x.Attribute("title").Value })
                                .FirstOrDefault();

        }

    }
}