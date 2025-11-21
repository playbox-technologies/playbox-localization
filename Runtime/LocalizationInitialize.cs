using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Playbox.Localization
{
    /// <summary>
    /// Automatically initializes the application's localization.
    /// Detects the device language on startup and loads the corresponding localized data.
    /// </summary>
    public static class LocalizationInitialize
    {
        /// <summary>
        /// Dictionary mapping language codes to their display names.
        /// </summary>
        private static Dictionary<string, string> LanguageMap = new Dictionary<string, string>()
        {
            { "en", "English" },
            { "ru", "Russian" },
            { "de", "German" },
            { "fr", "French" },
            { "es", "Spanish" },
        };

        /// <summary>
        /// Automatically detects the device language and loads the corresponding localization data.
        /// Defaults to English if the language is not supported.
        /// </summary>
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

        /// <summary>
        /// Loads the localization data for the specified language.
        /// </summary>
        /// <param name="lang">The name of the language to load.</param>
        private static void SetLanguage(string lang) => LocalizationStorage.Load(lang);
    }
}