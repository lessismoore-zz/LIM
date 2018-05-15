using LIM.TextTranslator.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LIM.TextTranslator.Models
{
    public class SelectedLanguage : ISelectedLanguage
    {
        private IHostingEnvironment _env;
        public SelectedLanguage()
        {
        }
        public SelectedLanguage(IHostingEnvironment env)
        {
            _env = env;
        }
        public SelectedLanguage(string Locale, IHostingEnvironment env = null)
        {
            this.Locale = Locale;
            _env = env;
        }


        private Dictionary<string, string> _langs = new Dictionary<string, string>()  {
                { "en-us", "English" }, { "es-es" , "Spanish"}, { "fr-fr", "French" }, { "de-de","German" }, { "zh-cn","Chinese" }
            };

        public Dictionary<string, string> Langs
        {
            get
            {
                //XDocument xDoc = XDocument.Load(_strTranslations_XMLPath);

                //return xDoc.Root.Elements("translation")
                //                            .GroupBy(x => x.Element("language").Value, StringComparer.OrdinalIgnoreCase)
                //                            .ToDictionary(x => x.First().Element("language").Value, x => x.First().Element("converted").Value);
                return _langs;
            }
        }

        public string Name { get { return _langs[Locale.ToLower()]; } }
        public string Locale { get; set; }
        public string Flag
        {
            get
            {
                XDocument xDoc = XDocument.Load(Path.Combine(_env.ContentRootPath, @"App_Data\Flags.xml"));

                return xDoc.Root.Elements("Flag")
                    .Where(x => x.Attribute("locale").Value.ToLower() == this.Locale.ToLower())
                    .Select(x => x.Element("data").Value).FirstOrDefault().ToString(); ;
            }
        }
    }

}
