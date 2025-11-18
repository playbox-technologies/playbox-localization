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
    public class LocalizationUpdaterWindow : EditorWindow
    {
        private const string EditorPrefsKey = "LocalizationUpdater_CSVUrl";
        private string _googleSheetCsvUrl;

        private string _localizationFolder = Path.Combine(Application.dataPath, "LocalizationStorage");

        [MenuItem("Tools/Localization/LocalizationLoader")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationUpdaterWindow>("Localization Update");
        }

        private void OnEnable()
        {
            _googleSheetCsvUrl = EditorPrefs.GetString(EditorPrefsKey, "");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Google Sheets CSV URL", EditorStyles.boldLabel);

            string newUrl = EditorGUILayout.TextField("CSV URL", _googleSheetCsvUrl);

            if (newUrl != _googleSheetCsvUrl)
            {
                _googleSheetCsvUrl = newUrl;
                EditorPrefs.SetString(EditorPrefsKey, _googleSheetCsvUrl);
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

        private void UpdateLocalization()
        {
            Directory.CreateDirectory(_localizationFolder);
            ClearLocalizationFolder();

            string csvData = DownloadCsv(_googleSheetCsvUrl);

            if (string.IsNullOrEmpty(csvData))
            {
                Debug.LogError("CsvData is Empty");
                return;
            }

            string[] lines = csvData.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
            {
                Debug.LogError("CSV has no data rows");
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

        private string DownloadCsv(string url)
        {
            using (WebClient client = new WebClient())
            {
                {
                    try
                    {
                        return client.DownloadString(url);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed downloading CSV: {e.Message}");
                        return null;
                    }
                }
            }

        }
    }
}
