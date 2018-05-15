using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LIM.TextTranslator
{

    public class TextTranslator : ITextTranslator
    {
        private string _strTranslations_XMLPath;
        private string _strAuthToken = null;
        private TextInfo _textInfo;
        private Dictionary<string, string> _dictCurrentSavedTranslations;

        private IMemoryCache _memoryCache;
        private IHttpContextAccessor _context;
        private IHostingEnvironment _env;
        private TranslatorSettings _Settings;

        public TextTranslator()
        {
        }

        public TranslatorSettings SetSettings
        {
            set { _Settings = value; }
        }

        public TextTranslator(TranslatorSettings settings = null, IMemoryCache memoryCache = null, IHostingEnvironment env = null, IHttpContextAccessor context = null)
        {
            _memoryCache = memoryCache;
            _env = env;
            _context = context;
            _strTranslations_XMLPath = Path.Combine(_env.ContentRootPath, @"App_Data\translations.xml");
            this.DefaultCulture = DefaultCulture;

            _textInfo = new CultureInfo(CurrentTextCulture).TextInfo;
            this.SetSettings = settings;
        }

        //public TextTranslator(string DefaultCulture)
        //{
        //    this.DefaultCulture = DefaultCulture;
        //    _textInfo = new CultureInfo(CurrentTextCulture, false).TextInfo;
        //}

        private string _DefaultCulture = "en-US";

        public string DefaultCulture
        {
            get { return _DefaultCulture; }
            set { _DefaultCulture = value; }
        }

        public string CurrentTextCulture
        {
            get
            {
                return CultureInfo.CurrentCulture.Name;
            }
        }

        public string FetchJSonStoredTranslations()
        {
            string strCurrentTextCulture = CurrentTextCulture;

            var objs = FetchStoredTranslations(strCurrentTextCulture)
                        .Select(x => new { k = x.Key, v = x.Value }).ToArray();

            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Culture = CurrentTextCulture,
                StoredTranslations = objs
            },
                Newtonsoft.Json.Formatting.None,
                new Newtonsoft.Json.JsonSerializerSettings
                {
                    StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.EscapeHtml
                }
            );
        }

        private Dictionary<string, string> FetchStoredTranslations(string strCurrentTextCulture, bool boolNoCache = false)
        {
            //during same request
            if ((_dictCurrentSavedTranslations != null) && (_dictCurrentSavedTranslations["__CurrentTextCulture__"].ToLower() == strCurrentTextCulture.ToLower()) && (!boolNoCache))
                return _dictCurrentSavedTranslations;

            //btween requests

            _memoryCache.TryGetValue<Dictionary<string, string>>("_savedTranslations_" + strCurrentTextCulture, out _dictCurrentSavedTranslations);

            if ((_dictCurrentSavedTranslations != null) && (!boolNoCache))
                return _dictCurrentSavedTranslations;

            else
            {
                XDocument xDoc = XDocument.Load(_strTranslations_XMLPath);

                if (xDoc.Root.Elements("translation").Count() == 0)
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                else
                    return xDoc.Root.Elements("translation")
                                            .Where(x => x.Element("language").Value.ToLower() == strCurrentTextCulture.ToLower())
                                            .GroupBy(x => x.Element("original").Value, StringComparer.OrdinalIgnoreCase)
                                            .OrderBy(x => x.Key)
                                            .ToDictionary(x => x.First().Element("original").Value, x => x.First().Element("converted").Value);
            }
        }

        public Dictionary<string, string> SaveTranslation(string strCurrentTextCulture, string strKey, string strValue)
        {
            return SaveTranslations(strCurrentTextCulture, new Dictionary<string, string>() { { strKey, strValue } });
        }
        private Dictionary<string, string> SaveTranslations(string strCurrentTextCulture, Dictionary<string, string> dictTranslations)
        {
            XDocument xDoc2 = XDocument.Load(_strTranslations_XMLPath);

            string[] arrStoredTranslations = FetchStoredTranslations(strCurrentTextCulture, true)
                                                .Select(x => x.Key.ToLowerInvariant()).ToArray();

            var test =
                dictTranslations.Where(x => !arrStoredTranslations.Contains(x.Key.ToLowerInvariant()))
                .Select(x => new XElement("translation",
                    new XElement("original", x.Key.ToLowerInvariant()),
                    new XElement("converted", x.Value.ToLowerInvariant()),
                    new XElement("language", strCurrentTextCulture)
                )).ToArray();

            if (test.Count() > 0)
            {

                Int32 intTotal = 0;

                if (xDoc2.Root.Attribute("total") != null)
                    int.TryParse(xDoc2.Root.Attribute("total").Value, out intTotal);

                xDoc2.Root.SetAttributeValue("total", (intTotal + test.Length).ToString());
                xDoc2.Root.SetAttributeValue("modified", DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"));

                xDoc2.Root.Add(test);

                using (var fileStream = System.IO.File.Create(_strTranslations_XMLPath))
                {
                    xDoc2.Save(fileStream);
                }

            }

            return dictTranslations;
        }

        private string StripHTMLFromText(string strText)
        {
            if ((strText.IndexOf("<") > -1) && (strText.IndexOf(">") > -1))
            {
                while (strText.IndexOf("<") > -1)
                {
                    int intTagStart = strText.IndexOf("<");
                    int intTagEnd = strText.IndexOf(">");
                    string strTag = strText.Substring(intTagStart, (intTagEnd - (intTagStart - 1)));

                    strText = strText.Replace(strTag, " ");
                }
            }

            return strText;
        }

        public string TranslateText(string strText, string strTextCulture = null)
        {
            string strCurrentTextCulture = (strTextCulture != null ? strTextCulture : this.CurrentTextCulture);
            string strUpdatedText = strText.Trim();
            string strUpdatedText2 = strUpdatedText.Replace(Environment.NewLine, " ").Replace("  ", " ");

            if (this.DefaultCulture.ToLower() == strCurrentTextCulture.ToLower())
            {
                return strUpdatedText;
            }

            //bool boolAPI_Added = false;
            _dictCurrentSavedTranslations = FetchStoredTranslations(strCurrentTextCulture);
            _dictCurrentSavedTranslations["__CurrentTextCulture__"] = strCurrentTextCulture;
            //================================

            strUpdatedText2 = StripHTMLFromText(strUpdatedText2);

            string[] arrSplitWords = strUpdatedText2.Split(new char[] { Convert.ToChar(" "), Convert.ToChar("/"), Convert.ToChar("-") });
            arrSplitWords = arrSplitWords.Distinct().ToArray();

            foreach (string word in arrSplitWords)
            {
                //if (string.IsNullOrEmpty(word.Trim()))
                //    continue;

                //trim and remove any spcecial chars 
                string[] arrAllowedChars = { "'" };

                string strWord = new string(word.Where(c => char.IsLetter(c) || arrAllowedChars.Contains(c.ToString())).ToArray());

                if (string.IsNullOrEmpty(strWord.Trim()))
                    continue;

                string strFoundTranslation = "";

                //System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
                //w.Start();

                strFoundTranslation = _dictCurrentSavedTranslations
                                        .Where(x => x.Key.ToLowerInvariant() == strWord.ToLowerInvariant())
                                        .Select(x => x.Value)
                                        .FirstOrDefault();
                //w.Stop();


                if (string.IsNullOrEmpty(strFoundTranslation))
                {
                    //Update with latest source
                    try
                    {
                        if (_strAuthToken == null)
                        {
                            _strAuthToken = GetAccessToken(_Settings.AzureKey).Result;
                        }

                        strFoundTranslation = CallTranslateAPI(_Settings.ApiURL, _strAuthToken, strWord, strCurrentTextCulture).Result;
                    }
                    catch (Exception)
                    {
                        strFoundTranslation = null;
                    }

                    if (!string.IsNullOrEmpty(strFoundTranslation))
                        _dictCurrentSavedTranslations.Add(_textInfo.ToLower(strWord.ToLowerInvariant()), strFoundTranslation);
                    else
                        strFoundTranslation = strWord;
                }

                //strFoundTranslation = _textInfo.ToTitleCase(strFoundTranslation.ToLowerInvariant());

                if (char.IsUpper(strWord[0]))
                    strFoundTranslation = char.ToUpperInvariant(strFoundTranslation[0]) + strFoundTranslation.Substring(1, (strFoundTranslation.Length - 1)).ToLowerInvariant();

                strUpdatedText = new Regex(String.Format(@"\b{0}\b", strWord), RegexOptions.CultureInvariant).Replace(strUpdatedText, strFoundTranslation);
            }
            //================================

            _memoryCache.Set<Dictionary<string, string>>("_savedTranslations_" + strCurrentTextCulture,
                                                            SaveTranslations(strCurrentTextCulture, _dictCurrentSavedTranslations));

            return strUpdatedText;
        }

        public async Task<string> GetAccessToken(string strKey)
        {
            return await new AzureAuthToken(strKey).GetAccessTokenAsync();
        }

        public async Task<string> CallTranslateAPI(string strAPI, string strAuthToken, string strText, string strTo)
        {
            using (var httpClient = new HttpClient())
            {
                //get an access token for each translation because they expire after 10 minutes.
                //header value is the "Bearer plus the token from ADM
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", strAuthToken);

                using (HttpResponseMessage responseMessage =
                    await httpClient.GetAsync(string.Format(strAPI, WebUtility.UrlEncode(strText), strTo)))
                {
                    using (StreamReader translatedStream = new StreamReader(await responseMessage.Content.ReadAsStreamAsync(), Encoding.GetEncoding("utf-8")))
                    {
                        System.Xml.XmlDocument xTranslation = new System.Xml.XmlDocument();
                        xTranslation.LoadXml(translatedStream.ReadToEnd());

                        if ((responseMessage.StatusCode == HttpStatusCode.OK) && (xTranslation.DocumentElement.ChildNodes.Count > 0))
                            return xTranslation.DocumentElement.FirstChild.InnerText;
                        else
                            return strText;
                    }
                }

            }


        }



    }
}

