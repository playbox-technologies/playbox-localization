using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace Playbox.Localization
{
    /// <summary>
    /// Editor window for updating localization data from a Google Sheets URL.
    /// Downloads the sheet data, converts it to JSON, and saves it in the LocalizationStorage folder.
    /// </summary>
    public class LocalizationUpdaterWindow : EditorWindow
    {
        private const string EditorPrefsKey = "LocalizationUpdater_SheetUrl";
        private string _sheetUrl;

        private string _localizationFolder = Path.Combine(Application.dataPath, "LocalizationStorage");

        /// <summary>
        /// Opens the localization updater window.
        /// </summary>
        [MenuItem("Playbox/Localization/LocalizationLoader")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationUpdaterWindow>("Localization Update");
        }

        private void OnEnable()
        {
            _sheetUrl = EditorPrefs.GetString(EditorPrefsKey, "");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Google Sheets URL", EditorStyles.boldLabel);

            string newUrl = EditorGUILayout.TextField("Google Sheets URL", _sheetUrl);

            if (newUrl != _sheetUrl)
            {
                _sheetUrl = newUrl;
                EditorPrefs.SetString(EditorPrefsKey, _sheetUrl);
            }

            bool hasData = !string.IsNullOrEmpty(newUrl);
            GUI.enabled = hasData;

            GUILayout.Space(10);

            if (GUILayout.Button("Update Localization"))
            {
                UpdateLocalization();
            }
            GUI.enabled = true;
        }

        /// <summary>
        /// Downloads the sheet data, converts it to JSON, and saves all languages to the LocalizationStorage folder.
        /// </summary>
        private void UpdateLocalization()
        {
            Directory.CreateDirectory(_localizationFolder);
            ClearLocalizationFolder();

            string exportUrl = ConvertToCsvExportUrl(_sheetUrl);

            string data = DownloadData(exportUrl);

            if (string.IsNullOrEmpty(data))
            {
                Debug.LogError("Downloaded data is empty.");
                return;
            }

            string[] lines = data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
            {
                Debug.LogError("Sheet has no data rows.");
                return;
            }

            string[] headers = lines[0].Split(',');
            int languageCount = headers.Length - 1;

            LocalizationFile[] files = new LocalizationFile[languageCount];
            for (int i = 0; i < languageCount; i++)
            {
                files[i] = new LocalizationFile();
                files[i]._items = new List<LocalizationItem>();
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] cols = lines[i].Split(',');
                if (cols.Length < 2) continue;

                string key = cols[0].Trim();
                if (string.IsNullOrEmpty(key)) continue;

                for (int j = 0; j < languageCount; j++)
                {
                    string value = (j + 1 < cols.Length) ? cols[j + 1].Trim() : "";
                    files[j]._items.Add(new LocalizationItem { _key = key, _value = value });
                }
            }

            for (int i = 0; i < languageCount; i++)
            {
                string languageCode = headers[i + 1].Trim();
                string path = Path.Combine(_localizationFolder, languageCode + ".json");
                string json = JsonConvert.SerializeObject(files[i], Formatting.Indented);
                File.WriteAllText(path, json);
                Debug.Log($"Saved {languageCode}.json in {path}");
            }

            AssetDatabase.Refresh();
            Debug.Log("Localization updated successfully.");

            var locWindow = Resources.FindObjectsOfTypeAll<LocalizationEditorWindow>().FirstOrDefault();
            if (locWindow != null)
            {
                locWindow.LoadLanguage();
                locWindow.UpdateLanguages();
                locWindow.Repaint();
            }
        }

        /// <summary>
        /// Converts a Google Sheets edit URL to a CSV export URL.
        /// </summary>
        private string ConvertToCsvExportUrl(string editUrl)
        {
            try
            {
                int startIndex = editUrl.IndexOf("/d/") + 3;
                int endIndex = editUrl.IndexOf("/", startIndex);

                if (endIndex == -1)
                    endIndex = editUrl.Length;

                string documentId = editUrl.Substring(startIndex, endIndex - startIndex);

                return $"https://docs.google.com/spreadsheets/d/{documentId}/export?format=csv";
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert URL: {ex.Message}");
                return editUrl;
            }
        }

        [Serializable]
        public class LocalizationFile
        {
            public List<LocalizationItem> _items;
        }

        [Serializable]
        public class LocalizationItem
        {
            public string _key;
            public string _value;
        }

        /// <summary>
        /// Deletes all JSON files from the LocalizationStorage folder.
        /// </summary>
        private void ClearLocalizationFolder()
        {
            if (!Directory.Exists(_localizationFolder))
                return;

            var files = Directory.GetFiles(_localizationFolder, "*.json");

            foreach (var file in files)
            {
                File.Delete(file);
            }

            Debug.Log("Localization folder cleared.");
        }

        /// <summary>
        /// Downloads the data from the specified URL.
        /// </summary>
        private string DownloadData(string url)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    return client.DownloadString(url);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed downloading data: {e.Message}");
                    return null;
                }
            }
        }
    }
}