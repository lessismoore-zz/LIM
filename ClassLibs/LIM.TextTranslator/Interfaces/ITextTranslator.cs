using System.Collections.Generic;
using System.Threading.Tasks;

namespace LIM.TextTranslator.Interfaces
{
    public interface ITextTranslator
    {
        Task<string> CallTranslateAPI(string strAPI, string strAuthToken, string strText, string strTo);
        Task<string> GetAccessToken(string strKey);
        string TranslateText(string strText, string strTextCulture = null);
        Dictionary<string, string> SaveTranslation(string strCurrentTextCulture, string strKey, string strValue);
        string FetchJSonStoredTranslations();

        string CurrentTextCulture { get; }
        Models.TranslatorSettings SetSettings { set; }

    }
}

