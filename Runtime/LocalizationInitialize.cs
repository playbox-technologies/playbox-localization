using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Playbox.Localization
{
    public static class LocalizationInitialize
    {
        private static Dictionary<string, string> LanguageMap = new Dictionary<string, string>()
        {
            { "en", "English" },
            { "ru", "Russian" },
            { "de", "German" },
            { "fr", "French" },
            { "es", "Spanish" },
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoDetectLanguage()
        {
            string code = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            Debug.Log("Device language code: " + code);

            if (LanguageMap.TryGetValue(code, out string languageName))
                SetLanguage(languageName);
            else
                SetLanguage("English");
        }

        private static void SetLanguage(string lang) => LocalizationStorage.Load(lang);
    }
}
