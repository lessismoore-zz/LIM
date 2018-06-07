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