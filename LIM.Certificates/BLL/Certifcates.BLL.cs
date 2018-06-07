using System;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using Microsoft.Reporting.WebForms;

namespace LIM.Certificates.BLL
{
    public class Certifcates
    {
        private string _ContentRootPath;
        ////private AppSettings _AppSettings;
        ////private IHttpContextAccessor _context;

        public Certifcates()
        {

        }
        public Certifcates(string ContentRootPath)
        {
            _ContentRootPath = ContentRootPath;
        }

        public Models.Certificate FetchCertificateForExam(LIM.Exam.Models.Exam azureExam)
        {
            Models.Certificate inst_report = FetchCertificatetByID(azureExam.ID);

            if (inst_report == null)
            {
                return null;
            }

            inst_report.Path = Path.Combine(this._ContentRootPath, inst_report.Path);

            using (LocalReport rpt = new LocalReport()
            {
                ReportPath = inst_report.Path,
                DisplayName = inst_report.Title
            })
            {
                rpt.SetParameters(new ReportParameter() { Name = "Name", Values = { azureExam.TakerName } });

                inst_report.FileName = (azureExam.TakerName + " " + rpt.DisplayName).Replace(" ", ".").Replace("/", ".") + ".pdf";

                inst_report.FileContents = rpt.Render("pdf");
            }

            return inst_report;
        }

        public byte[] FetchZipOfCertificatesForUsers(int intRptID, string[] arrNames)
        {
            byte[] compressedBytes = null;

            Models.Certificate inst_report = FetchCertificatetByID(intRptID);

            if (inst_report == null)
            {
                return compressedBytes;
            }

            inst_report.Path = Path.Combine(_ContentRootPath, inst_report.Path);

            //====================================

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

            return compressedBytes;

        }

        public Models.Certificate FetchCertificatetByID(int intRptID)
        {
            XDocument xdocument = XDocument.Load(Path.Combine(_ContentRootPath, "app_data\\Reports.xml"));

            return xdocument.Root.Elements()
                                .Where(x => x.Attribute("id").Value == intRptID.ToString())
                                .Select(x => new Models.Certificate() { ID = intRptID, Path = x.Attribute("path").Value, Title = x.Attribute("title").Value })
                                .FirstOrDefault();

        }
    }
}

