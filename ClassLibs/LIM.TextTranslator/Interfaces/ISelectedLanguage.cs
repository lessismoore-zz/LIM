using System.Collections.Generic;

namespace LIM.TextTranslator.Interfaces
{
    public interface ISelectedLanguage
    {
        string Name { get; }
        string Locale { get; set; }
        string Flag { get; }
        Dictionary<string, string> Langs { get; }
    }
}

