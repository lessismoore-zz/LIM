using System;
using LIM.Exam.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.NotificationHubs;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using LIM.TextTranslator.Interfaces;
using System.Linq;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;

namespace LIM.Exam.Web.Controllers
{
    public class BaseController : Controller
    {
        IHttpContextAccessor _context;
        ITextTranslator _TextTranslator;
        AppSettings _Settings;

        public BaseController()
        {
        }
        public BaseController(IHttpContextAccessor context = null, ITextTranslator TextTranslator = null, IOptions<AppSettings> settings = null)
        {
            _context = context;
            _TextTranslator = TextTranslator;
            _Settings = settings !=null ? settings.Value: null;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string txtUserName, string txtPassword)
        {

            if ((txtUserName?.ToLower() == "Thanos") || (txtPassword?.ToLower() == "snap")) {
                return Redirect("/Home/Index");
            }

            return View();
        }
        [HttpPost]
        public IActionResult SaveLangauge(string ddlLangauge)
        {
            if (!new LIM.TextTranslator.Models.SelectedLanguage().Langs.Keys.Any(x => x.ToLower() == ddlLangauge.ToLower())) {
                throw new Exception("Language is not supported");
            }

            _context.HttpContext.Response.Cookies.Append("language", ddlLangauge, new CookieOptions() {
                Expires = DateTime.Now.AddDays(1)
            });

            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo(ddlLangauge);
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo(ddlLangauge);

            //return RedirectToAction("Index");
            return Redirect(_context.HttpContext.Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        [Route("cspreport")]
        public IActionResult CspReport([FromBody] CspReportRequest request)
        {
            // TODO: log request to a datastore somewhere
            //_logger.LogWarning($"CSP Violation: {request.CspReport.DocumentUri}, {request.CspReport.BlockedUri}");

            return Ok();
        }

        [HttpPost]
        [Route("Contact")]
        public async Task<IActionResult> SendEmail(string subject, string message, string UID)
        {
            if (UID == "123456")
            {
                await new LIM.SendGrid.SendGrid(_Settings.SendGridSettings).SendEmailAsync(
                    "",
                    "Certification Request",
                    "noreply@lessismoore.net",
                    string.Format("New Request from {0} at Email: {1}", "Tim", "Moore")
                );
            }

            return Ok();
        }

    }

}
