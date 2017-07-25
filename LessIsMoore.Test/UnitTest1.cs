using LessIsMoore.Web.Controllers;
using LessIsMoore.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;

namespace LessIsMoore.Test
{

    public class UnitTest1
    {
        private string _strKey = "ec71a6ada12849689b25f26e8f2b9d81";
        private string _strAPI = "https://api.microsofttranslator.com/v2/Http.svc/Translate?text={0}&to={1}";
        IWebDriver _wd;
        IJavaScriptExecutor _js;
        private string _strBaseURL = "http://www.lessismoore.net";
        public UnitTest1()
        {
            //var chromeOptions = new ChromeOptions();
            //chromeOptions.AddArguments("test-type");
            //chromeOptions.AddArguments("start-maximized");
            //chromeOptions.AddArguments("--disable-extensions");
            //chromeOptions.AddArguments("no-sandbox");

            //wd = new ChromeDriver(chromeOptions);

            _wd = new InternetExplorerDriver();
            _js = (IJavaScriptExecutor)_wd;
        }

        [Theory]
        [Trait("Category", "Selenium")]

        [InlineData("Joe Dirt", "Dirt.Joe@Microsoft.com")]

        public void VerifyExam(string strName, string strEmail)
        {
            using (_wd)
            {
                _wd.Navigate().GoToUrl(_strBaseURL+ "/exam?sf=8d679ae7-e939-474c-a3ff-8501ee636b12");
                //_wd.Manage().Timeouts().PageLoad = new System.TimeSpan(0, 0, 0, 3);

                IWait<IWebDriver> wait = new WebDriverWait(_wd, System.TimeSpan.FromSeconds(2.00));

                _wd.FindElement(By.Id("txtName")).SendKeys(strName);
                _wd.FindElement(By.Id("txtEmail")).SendKeys(strEmail);

                string[] ctrlIDs = { "answer_101", "answer_202", "answer_303", "answer_401", "answer_504", "answer_604",
                "answer_703", "answer_801", "answer_902", "answer_1003" };

                foreach (string strID in ctrlIDs)
                {
                    _wd.FindElement(By.Id(strID)).Click();
                    _js.ExecuteScript("window.scrollBy(0,300)");
                }

                _wd.FindElement(By.Id("txtSubmit")).Click();

                wait.Until(ExpectedConditions.AlertIsPresent());

                _wd.SwitchTo().Alert().Accept();

                wait.Until(ExpectedConditions.AlertIsPresent());

                _wd.SwitchTo().Alert().Accept();

                Xunit.Assert.Contains("great job", _wd.FindElement(By.Id("divPassMessage")).Text.ToLower());

            }
        }

        [Theory]
        [InlineData("home")]
        [Trait("Category", "Unit")]

        public async void VerifyHomeLoads(string strPageName)
        {
            HomeController homeController = new HomeController();
            ViewResult result = await homeController.Index() as ViewResult;

            Xunit.Assert.Equal(strPageName, result.ViewData["title"].ToString().ToLower());
        }

        [Fact]
        [Trait("Category", "Unit")]

        public async void VerifyVergeNewsFeed()
        {
            NewsFeed[] arrNewsFeeds = await new BLL().FetchVergeNewsFeed();
            Xunit.Assert.True(arrNewsFeeds.Length > 0);
        }

        [Fact]
        [Trait("Category", "Unit")]

        public async void VerifyAzureNewsFeed()
        {
            NewsFeed[] arrNewsFeeds = await new BLL().FetchAzureNewsFeed();
            Xunit.Assert.True(arrNewsFeeds.Length > 0);
        }

        [Theory]
        [InlineData("Less", "Moins", "fr-FR")]
        [InlineData("Less", "Menos", "es-ES")]
        [Trait("Category", "Unit")]

        public async void VerifyTranslationAPILogic(string strText, string strExpectedText, string strLangangue)
        {
            Web.Translation.TextTranslator inst_TextTranslator = new Web.Translation.TextTranslator();

            string strAuthToken = await inst_TextTranslator.GetAccessToken(_strKey);
            string strTranslation = await inst_TextTranslator.CallTranslateAPI(_strAPI, strAuthToken, strText, strLangangue);

            Xunit.Assert.Equal(strExpectedText, strTranslation);
        }

        [Theory]
        [InlineData("**Test Bug in LessIsMoore.Web**", "UnitTest: VSTS_UpdateWorkItem", null, 339)]
        [Trait("Category", "Unit")]

        //[InlineData("New Bug", "UnitTest: VSTS_UpdateWorkItem", "Bug", -1)]
        public void VSTS_UpdateWorkItem(string strTitle, string strError, string strWorkItemType, int intItemID)
        {
            int intID = new BLL().VSTS_SaveWorkItem(strTitle, strError, strWorkItemType, intItemID, "Verified on "+ System.DateTime.Now.ToString());
            Xunit.Assert.Equal(intItemID, intID);
        }
    }
}
