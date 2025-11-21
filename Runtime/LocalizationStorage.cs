using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Playbox.Localization
{
    /// <summary>
    /// Runtime storage for localization data.
    /// Loads JSON files from the "LocalizationStorage" folder and provides access to localized strings.
    /// </summary>
    public static class LocalizationStorage
    {
        private static Dictionary<string, string> _entries = new();
        private static string _currentLanguage = "English";
        private static List<string> _languages = new();

        /// <summary>
        /// List of available languages.
        /// </summary>
        public static IReadOnlyList<string> AvailableLanguages => _languages.AsReadOnly();

        [System.Serializable]
        private class LocalizationItem
        {
            public string _key;
            public string _value;
        }

        [System.Serializable]
        private class LocalizationFile
        {
            public List<LocalizationItem> _items;
        }

        /// <summary>
        /// Returns the number of available languages.
        /// </summary>
        /// <returns>The count of languages detected in the LocalizationStorage folder.</returns>
        public static int GetLanguagesCount()
        {
            RefreshLanguages();
            return _languages.Count;
        }

        /// <summary>
        /// Scans the "LocalizationStorage" folder for available language JSON files and refreshes the internal list.
        /// </summary>
        private static void RefreshLanguages()
        {
            _languages.Clear();
            string folderPath = Path.Combine(Application.dataPath, "LocalizationStorage");

            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[LocalizationStorage] Folder not found: {folderPath}");
                return;
            }

            string[] files = Directory.GetFiles(folderPath, "*.json");
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                _languages.Add(fileName);
            }

            Debug.Log($"[LocalizationStorage] Available languages: {string.Join(", ", _languages)}");
        }

        /// <summary>
        /// Loads the localization data for the specified language.
        /// Defaults to English if the language is not found.
        /// </summary>
        /// <param name="languageCode">The name of the language to load (e.g., "English", "Russian").</param>
        public static void Load(string languageCode)
        {
            RefreshLanguages();

            if (!_languages.Contains(languageCode))
            {
                Debug.Log($"[LocalizationStorage] Language '{languageCode}' not found. Using English as default.");
                languageCode = "English";
            }

            _currentLanguage = languageCode;

            string path = Path.Combine(Application.dataPath, "LocalizationStorage", $"{languageCode}.json");

            try
            {
                string json = File.ReadAllText(path);
                var localizationFile = JsonConvert.DeserializeObject<LocalizationFile>(json);
                _entries = new Dictionary<string, string>();

                if (localizationFile?._items != null)
                {
                    foreach (var item in localizationFile._items)
                    {
                        if (!string.IsNullOrEmpty(item._key))
                            _entries[item._key] = item._value ?? string.Empty;
                    }
                }

                Debug.Log($"[LocalizationStorage] Loaded language '{_currentLanguage}', keys: {_entries.Count}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LocalizationStorage] Error reading JSON: {ex.Message}");
                _entries = new();
            }
        }

        /// <summary>
        /// Returns the localized string for the specified key.
        /// If the key is not found, returns "#key".
        /// </summary>
        /// <param name="key">The localization key to retrieve.</param>
        public static string Get(string key)
        {
            if (_entries.TryGetValue(key, out var value))
                return value;
            return $"#{key}";
        }

        /// <summary>
        /// Returns the name of the currently loaded language.
        public static string CurrentLanguage => _currentLanguage;
    }
}