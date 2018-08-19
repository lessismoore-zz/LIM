namespace LIM.Exam.Web.Models
{
    public class AppSettings
    {
        public LIM.SendGrid.Models.SendGridSettings SendGridSettings { get; set; }
        public LIM.TextTranslator.Models.TranslatorSettings TranslatorSettings { get; set; }
        public bool FeatureFlag_ShowLanguageKlingon { get; set; }
    }

}