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
            HomeController homeController = new HomeController();
            ViewResult result = await homeController.Index() as ViewResult;

            Assert.Equal(strPageName, result.ViewData["title"].ToString().ToLower());
        }

        [Fact]
        public async void VerifyVergeNewsFeed()
        {
            NewsFeed[] arrNewsFeeds = await new BLL().FetchVergeNewsFeed();
            Assert.True(arrNewsFeeds.Length > 0);
        }
        [Fact]
        public async void VerifyAzureNewsFeed()
        {
            NewsFeed[] arrNewsFeeds = await new BLL().FetchAzureNewsFeed();
            Assert.True(arrNewsFeeds.Length > 0);
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

        [Theory]
        [InlineData("**Test Bug in LessIsMoore.Web**", "UnitTest: VSTS_UpdateWorkItem", null, 339)]
       //[InlineData("New Bug", "UnitTest: VSTS_UpdateWorkItem", "Bug", -1)]
        public void VSTS_UpdateWorkItem(string strTitle, string strError, string strWorkItemType, int intItemID)
        {
            int intID = new BLL().VSTS_SaveWorkItem(strTitle, strError, strWorkItemType, intItemID, "Verified on "+ System.DateTime.Now.ToString());
            Assert.Equal(intItemID, intID);
        }
    }
}
