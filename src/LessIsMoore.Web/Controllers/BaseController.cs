using System;
using LessIsMoore.Net.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LessIsMoore.Net.Controllers
{
    public class BaseController : Controller
    {
        IHttpContextAccessor _context;
        ITextTranslator _TextTranslator;

        public BaseController()
        {
        }
        public BaseController(IHttpContextAccessor context, ITextTranslator TextTranslator)
        {
            _context = context;
            _TextTranslator = TextTranslator;
        }

        [HttpPost]
        public IActionResult SaveLangauge()
        {
            string strLanguage = Request.Form["ddlLangauge"];
            _context.HttpContext.Response.Cookies.Append("language", strLanguage, new CookieOptions() { Expires = DateTime.Now.AddDays(10) });

            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo(strLanguage);
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo(strLanguage);

            //return RedirectToAction("Index");
            return Redirect(_context.HttpContext.Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<JsonResult> GetTranslation(string text)
        {
            string strFoundTranslation = null;

            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    string strToken = await _TextTranslator.GetAccessToken();
                    strFoundTranslation = await _TextTranslator.CallTranslateAPI(strToken, text, _TextTranslator.CurrentTextCulture);
                }
                catch (Exception)
                {
                    strFoundTranslation = null;
                }

                if (!string.IsNullOrEmpty(strFoundTranslation))
                {
                    _TextTranslator.SaveTranslation(
                        _TextTranslator.CurrentTextCulture,
                        new System.Globalization.CultureInfo(_TextTranslator.CurrentTextCulture).TextInfo.ToLower(text.ToLowerInvariant()),
                        strFoundTranslation);
                }
                else
                    strFoundTranslation = text;
            }
            else {
                strFoundTranslation = text;
            }

            return new JsonResult(new { k=text, v= strFoundTranslation });
        }


    }

}