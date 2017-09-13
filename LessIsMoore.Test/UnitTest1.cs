using LessIsMoore.Web.Controllers;
using LessIsMoore.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using System;
using Microsoft.DotNet.PlatformAbstractions;
using System.Linq;
using System.Collections.Generic;

namespace LessIsMoore.Test
{

    public class UnitTest1
    {
        private string _strKey = "ec71a6ada12849689b25f26e8f2b9d81";
        private string _strAPI = "https://api.microsofttranslator.com/v2/Http.svc/Translate?text={0}&to={1}";
        IWebDriver _wd;
        IJavaScriptExecutor _js;
        private string _strBaseURL = "http://www.lessismoore.net";


        //public UnitTest1()
        //{
        //    Initialize();
        //}

        //[Theory]
        //[Trait("Category", "Selenium")]
        //[InlineData("Joe Dirt", "Dirt.Joe@Microsoft.com")]

        //public void VerifyExam(string strName, string strEmail)
        //{

        //    //var chromeOptions = new ChromeOptions();
        //    //chromeOptions.AddArguments("test-type");
        //    //chromeOptions.AddArguments("start-maximized");
        //    //chromeOptions.AddArguments("--disable-extensions");
        //    //chromeOptions.AddArguments("no-sandbox");

        //    //wd = new ChromeDriver(chromeOptions);
        //    //_wd = new InternetExplorerDriver();

        //    using (_wd = new PhantomJSDriver())
        //    {
        //        _js = (IJavaScriptExecutor)_wd;

        //        _wd.Navigate().GoToUrl(_strBaseURL + "/exam?sf=8d679ae7-e939-474c-a3ff-8501ee636b12");
        //        _wd.Manage().Timeouts().ImplicitWait = new System.TimeSpan(0, 0, 5);

        //        IWait<IWebDriver> wait = new WebDriverWait(_wd, System.TimeSpan.FromSeconds(5));

        //        _wd.FindElement(By.Id("txtName")).SendKeys(strName);
        //        _wd.FindElement(By.Id("txtEmail")).SendKeys(strEmail);

        //        int[] ctrlIDs = { 101, 202, 303, 401, 504, 604, 703, 801, 902, 1003 };

        //        foreach (int intID in ctrlIDs)
        //        {
        //            _wd.FindElement(By.Id("answer_" + intID.ToString())).Click();
        //            _js.ExecuteScript("window.scrollBy(0,300)");
        //        }

        //        _js.ExecuteScript("window.confirm = function(msg){return true;};");
        //        _wd.FindElement(By.Id("txtSubmit")).Click();

        //        //wait.Until(ExpectedConditions.AlertIsPresent());

        //        //_wd.SwitchTo().Alert().Accept();

        //        //wait.Until(ExpectedConditions.AlertIsPresent());

        //        //_wd.SwitchTo().Alert().Accept();
        //        wait.Until(ExpectedConditions.ElementExists(By.Id("aPassMessage")));
        //        Assert.Contains("great job", _wd.FindElement(By.Id("aPassMessage")).Text.ToLower());
        //    }
        //}

        private Exam PopulateQuestionsFromXML()
        {
            string strXMLPath =
                "<questions><question><text>Favorite Letter</text><answers><answer>A</answer><answer>B</answer><answer>Y</answer><answer>Z</answer></answers></question></questions>";

            Exam azureExam = new Exam();

            System.Xml.Linq.XDocument xdocument = System.Xml.Linq.XDocument.Parse(strXMLPath);

            azureExam.ExamQuestions =
                new BLL().PopulateExamQuestionsFromXML(xdocument.Root.Elements("question"));

            return azureExam;
        }

        [Fact]
        [Trait("Category", "UnitTest")]

        public void VerifyExam_1_PopulateQuestions()
        {
            Exam azureExam = PopulateQuestionsFromXML();
            Assert.True(azureExam.TotalQuestions == 1);
        }

        [Theory]
        [InlineData("b", "b")]
        [Trait("Category", "UnitTest")]

        public void VerifyExam_2_GradeExam(string strAnswer, string strResponse)
        {
            Exam azureExam = PopulateQuestionsFromXML();

            azureExam.ExamQuestions.First()
                .ExamChoices.Where(x => x.Text.ToLower() == strAnswer)
                .First().IsCorrect = true;

            IEnumerable<ExamResponse> rsps =
                BLL.CollectExamResponses(azureExam, x => (x.Text.ToLower() == strResponse));

            int intTotalCorrectQuestions = BLL.GradeExam(azureExam, rsps);

            Assert.True(intTotalCorrectQuestions >= azureExam.TotalQuestions);
        }

        [Theory]
        [InlineData("home")]
        [Trait("Category", "UnitTest")]

        public async void VerifyHomeLoads(string strPageName)
        {
            HomeController homeController = new HomeController();
            ViewResult result = await homeController.Index() as ViewResult;

            Assert.Equal(strPageName, result.ViewData["title"].ToString().ToLower());
        }

        [Fact]
        [Trait("Category", "UnitTest")]

        public async void VerifyVergeNewsFeed()
        {
            NewsFeed[] arrNewsFeeds = await new BLL().FetchVergeNewsFeed();
            Assert.True(arrNewsFeeds.Length > 0);
        }

        [Fact]
        [Trait("Category", "UnitTest")]

        public async void VerifyAzureNewsFeed()
        {
            NewsFeed[] arrNewsFeeds = await new BLL().FetchAzureNewsFeed();
            Xunit.Assert.True(arrNewsFeeds.Length > 0);
        }

        [Theory]
        [InlineData("Less", "Moins", "fr-FR")]
        [InlineData("Less", "Menos", "es-ES")]
        [Trait("Category", "UnitTest")]

        public async void VerifyTranslationAPILogic(string strText, string strExpectedText, string strLangangue)
        {
            Web.Translation.TextTranslator inst_TextTranslator = new Web.Translation.TextTranslator();

            string strAuthToken = await inst_TextTranslator.GetAccessToken(_strKey);
            string strTranslation = await inst_TextTranslator.CallTranslateAPI(_strAPI, strAuthToken, strText, strLangangue);

            Xunit.Assert.Equal(strExpectedText, strTranslation);
        }

        //[Theory]
        //[InlineData("**Test Bug in LessIsMoore.Web**", "UnitTest: VSTS_UpdateWorkItem", null, 339)]
        //[Trait("Category", "UnitTest")]

        ////[InlineData("New Bug", "UnitTest: VSTS_UpdateWorkItem", "Bug", -1)]
        //public void VSTS_UpdateWorkItem(string strTitle, string strError, string strWorkItemType, int intItemID)
        //{
        //    int intID = new BLL().VSTS_SaveWorkItem(strTitle, strError, strWorkItemType, intItemID, "Verified on "+ System.DateTime.Now.ToString());
        //    Xunit.Assert.Equal(intItemID, intID);
        //}
    }
}
