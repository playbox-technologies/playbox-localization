using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Playbox.Localization
{
    public static class LocalizationStorage
    {
        private static Dictionary<string, string> _entries = new();

        private static string _currentLanguage = "English";

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

        public static void Load(string languageCode)
        {
            _currentLanguage = languageCode;

            string path = Path.Combine(Application.dataPath,
                "LocalizationStorage",
                $"{languageCode}.json");

            if (!File.Exists(path))
            {
                Debug.LogError($"[LocalizationStorage] Localization file not found: {path}");
                _entries = new();
                return;
            }

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

                Debug.Log($"[LocalizationStorage] Loaded language  '{languageCode}', keys: {_entries.Count}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LocalizationStorage] Error reading  JSON: {ex.Message}");
                _entries = new();
            }
        }

        public static string Get(string key)
        {
            if (_entries.TryGetValue(key, out var value))
                return value;

            return $"#{key}";
        }

        public static string CurrentLanguage => _currentLanguage;
    }
}