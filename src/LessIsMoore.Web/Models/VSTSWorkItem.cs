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
    public class VSTSWorkItem
    {
        public int id { get; set; }
        public int rev { get; set; }
        public KeyValuePair<string, string> fields{ get; set; }
    }
}