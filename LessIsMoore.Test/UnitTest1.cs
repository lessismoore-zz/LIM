using LessIsMoore.Web.Controllers;
using LessIsMoore.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace LessIsMoore.Test
{
    public class UnitTest1
    {
        private string _strKey = "ec71a6ada12849689b25f26e8f2b9d81";
        private string _strAPI = "https://api.microsofttranslator.com/v2/Http.svc/Translate?text={0}&to={1}";

        [Theory]
        [InlineData("home")]
        public async void VerifyHomeLoads(string strPageName)
        {
            HomeController homeController = new HomeController(null, null, null, null, null);
            ViewResult result = await homeController.Index() as ViewResult;

            Assert.Equal(strPageName, result.ViewData["title"].ToString().ToLower());
        }

        [Fact]
        public async void VerifyNewsFeedLoads()
        {
            NewsArticle[] arrNewsArticles = await new BLL().FetchNewsArcticles();
            Assert.True(arrNewsArticles.Length > 0);
        }

        [Theory]
        [InlineData("Less", "Moins", "fr-FR")]
        [InlineData("Less", "Menos", "es-ES")]
        public async void VerifyTranslationAPILogic(string strText, string strExpectedText, string strLangangue)
        {
            Web.Translation.TextTranslator inst_TextTranslator = new Web.Translation.TextTranslator();

            string strAuthToken = await inst_TextTranslator.GetAccessToken(_strKey);
            string strTranslation = await inst_TextTranslator.CallTranslateAPI(_strAPI, strAuthToken, strText, strLangangue);

            Assert.Equal(strExpectedText, strTranslation);
        }
    }
}
