using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace LessIsMoore.Web.Models
{
    public class AppSettings
    {
        public LIM.SendGrid.Models.SendGridSettings SendGridSettings { get; set; }
        public LIM.TextTranslator.Models.TranslatorSettings TranslatorSettings { get; set; }
        public string NotificationHubConn { get; set; }
        public string NotificationHubName { get; set; }
        public bool FeatureFlag_ShowLanguageKlingon { get; set; }

        public string VSTSToken { get; set; }
    }

}