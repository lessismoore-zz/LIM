using System;
using LessIsMoore.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.NotificationHubs;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using LessIsMoore.Web.Translation;
using System.Linq;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;

namespace LessIsMoore.Web.Controllers
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

        [HttpPost]
        public IActionResult SaveLangauge(string ddlLangauge)
        {
            if (!new SelectedLanguage().Langs.Keys.Any(x => x.ToLower() == ddlLangauge.ToLower())) {
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
        public async Task<IActionResult> SendNotification(string txtNotifyUsers)
        {
            string strMessage = "{\"data\": {\"message\": \""+ txtNotifyUsers + "\"}}";
            
            NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString(_Settings.NotificationHubConn, _Settings.NotificationHubName);

            //var googleResult =
            //    await hub.SendGcmNativeNotificationAsync(payload, tags);
            //var windowsResult =
            //  await hub.SendWindowsNativeNotificationAsync(toast, tags;
            //var appleResult =
            //  await hub.SendAppleNativeNotificationAsync(alert, tags);

            await hub.SendGcmNativeNotificationAsync(strMessage);

            return Redirect(_context.HttpContext.Request.Headers["Referer"].ToString());
        }

        //[HttpPost]
        //public async System.Threading.Tasks.Task<JsonResult> GetTranslation(string text)
        //{
        //    string strFoundTranslation = null;

        //    if (!string.IsNullOrEmpty(text))
        //    {
        //        try
        //        {
        //            string strToken = await _TextTranslator.GetAccessToken();
        //            strFoundTranslation = await _TextTranslator.CallTranslateAPI(strToken, text, _TextTranslator.CurrentTextCulture);
        //        }
        //        catch (Exception)
        //        {
        //            strFoundTranslation = null;
        //        }

        //        if (!string.IsNullOrEmpty(strFoundTranslation))
        //        {
        //            _TextTranslator.SaveTranslation(
        //                _TextTranslator.CurrentTextCulture,
        //                new System.Globalization.CultureInfo(_TextTranslator.CurrentTextCulture).TextInfo.ToLower(text.ToLowerInvariant()),
        //                strFoundTranslation);
        //        }
        //        else
        //            strFoundTranslation = text;
        //    }
        //    else {
        //        strFoundTranslation = text;
        //    }

        //    return new JsonResult(new { k=text, v= strFoundTranslation });
        //}


    }

}