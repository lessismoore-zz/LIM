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

    public class NewsFeed
    {
        public string Headline { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
        public string Source { get; set; }

    }
}