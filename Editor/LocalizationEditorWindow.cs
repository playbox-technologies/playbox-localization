using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Playbox.Localization
{
    /// <summary>
    /// Unity Editor window for localization management.
    /// Allows adding, editing, searching, and saving translations for multiple languages.
    /// </summary>
    public class LocalizationEditorWindow : EditorWindow
    {
        private Dictionary<string, LocalizationWrapper> _allLanguages = new Dictionary<string, LocalizationWrapper>();
        private List<string> _languages = new List<string>();
        private Vector2 _scrollPos;

        private string _newKey = "";
        private string _searchQuery = "";

        private const float ButtonHeight = 30f;

        /// <summary>
        /// Shows the Localization Editor window in Unity Editor.
        /// </summary>
        [MenuItem("Playbox/Localization/Localization Editor")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationEditorWindow>("Localization Editor");
        }

        /// <summary>
        /// Loads all language JSON files from the LocalizationStorage folder.
        /// </summary>
        private void OnEnable()
        {
            LoadAllLanguages();
        }

        /// <summary>
        /// Loads all language JSON files into a dictionary.
        /// Each language is stored in a separate JSON file under Assets/LocalizationStorage.
        /// </summary>
        public void LoadAllLanguages()
        {
            string folderPath = Path.Combine(Application.dataPath, "LocalizationStorage");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            _allLanguages.Clear();
            _languages.Clear();

            var files = Directory.GetFiles(folderPath, "*.json");
            foreach (var file in files)
            {
                string lang = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);
                var wrapper = JsonConvert.DeserializeObject<LocalizationWrapper>(json) ?? new LocalizationWrapper { _items = new List<TranslationItem>() };
                _allLanguages[lang] = wrapper;
                _languages.Add(lang);
            }
        }

        /// <summary>
        /// Draws the main GUI of the Localization Editor window.
        /// </summary>
        private void OnGUI()
        {
            DrawAddNewKey();
            DrawSearchField();

            EditorGUILayout.Space();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));

            DrawLocalizationMatrix();
            EditorGUILayout.Space();
            DrawEmptyValuesHelpBox();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            DrawSaveAndResetButtons();
        }

        /// <summary>
        /// Draws the section to add a new translation key.
        /// </summary>
        private void DrawAddNewKey()
        {
            EditorGUILayout.LabelField("Add New Key", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key", GUILayout.Width(50));
            _newKey = EditorGUILayout.TextField(_newKey, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            GUI.enabled = !string.IsNullOrEmpty(_newKey);
            if (GUILayout.Button("Add"))
            {
                AddNewKeyToAllLanguages(_newKey);
                _newKey = "";
            }
            GUI.enabled = true;
        }

        /// <summary>
        /// Adds a new key to all loaded languages if it doesn't already exist.
        /// </summary>
        /// <param name="key">The translation key to add.</param>
        private void AddNewKeyToAllLanguages(string key)
        {
            foreach (var lang in _languages)
            {
                var wrapper = _allLanguages[lang];
                if (!wrapper._items.Any(i => i._key == key))
                    wrapper._items.Add(new TranslationItem { _key = key, _value = "" });
            }
        }

        /// <summary>
        /// Draws the search input field for filtering translation keys.
        /// </summary>
        private void DrawSearchField()
        {
            EditorGUILayout.LabelField("Search", EditorStyles.boldLabel);
            _searchQuery = EditorGUILayout.TextField(_searchQuery);
        }

        /// <summary>
        /// Draws the main localization matrix with all keys and translations.
        /// </summary>
        private void DrawLocalizationMatrix()
        {
            if (_languages.Count == 0) return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key", GUILayout.Width(200));
            foreach (var lang in _languages)
                EditorGUILayout.LabelField(lang, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            var allKeys = _allLanguages.Values.SelectMany(w => w._items.Select(i => i._key)).Distinct().ToList();
            if (!string.IsNullOrEmpty(_searchQuery))
                allKeys = allKeys.Where(k => k.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            var keysToDelete = new List<string>();

            foreach (var key in allKeys)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(key, GUILayout.Width(200));

                foreach (var lang in _languages)
                {
                    var wrapper = _allLanguages[lang];
                    var item = wrapper._items.FirstOrDefault(x => x._key == key);
                    string value = item?._value ?? "";

                    string newValue = EditorGUILayout.TextField(value, GUILayout.Width(150));

                    if (item != null)
                        item._value = newValue;
                    else if (!string.IsNullOrEmpty(newValue))
                        wrapper._items.Add(new TranslationItem { _key = key, _value = newValue });
                }

                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    keysToDelete.Add(key);

                EditorGUILayout.EndHorizontal();
            }

            DeleteKeys(keysToDelete);
        }

        /// <summary>
        /// Deletes selected keys from all languages after confirmation.
        /// </summary>
        /// <param name="keysToDelete">List of keys to delete.</param>
        private void DeleteKeys(List<string> keysToDelete)
        {
            foreach (var key in keysToDelete)
            {
                if (EditorUtility.DisplayDialog(
                    "Delete Localization Key",
                    $"Are you sure you want to delete '{key}' in all languages?",
                    "Yes", "No"))
                {
                    foreach (var lang in _languages)
                    {
                        var wrapper = _allLanguages[lang];
                        wrapper._items.RemoveAll(x => x._key == key);
                    }
                }
            }
        }

        /// <summary>
        /// Draws Save and Reset buttons with appropriate enabling/disabling logic.
        /// </summary>
        private void DrawSaveAndResetButtons()
        {
            bool hasEmptyValue = _allLanguages.Values
                .SelectMany(w => w._items)
                .Any(i => string.IsNullOrWhiteSpace(i._value));

            GUI.enabled = !hasEmptyValue;

            if (GUILayout.Button("Save All", GUILayout.Height(ButtonHeight), GUILayout.ExpandWidth(true)))
            {
                SaveAllLanguages();
                EditorUtility.DisplayDialog("Saved", "All translations have been saved successfully.", "OK");
            }

            GUI.enabled = true;

            if (GUILayout.Button("Reset", GUILayout.Height(ButtonHeight), GUILayout.ExpandWidth(true)))
            {
                if (EditorUtility.DisplayDialog("Reset", "Do you want to discard all changes and reload from JSON?", "Yes", "No"))
                {
                    LoadAllLanguages();
                    GUI.FocusControl(null);
                }
            }
        }

        /// <summary>
        /// Displays a HelpBox with a list of keys that have empty translations.
        /// </summary>
        private void DrawEmptyValuesHelpBox()
        {
            var emptyTranslations = new List<string>();
            foreach (var lang in _allLanguages.Keys)
            {
                var wrapper = _allLanguages[lang];
                var emptyKeys = wrapper._items
                    .Where(i => string.IsNullOrWhiteSpace(i._value))
                    .Select(i => i._key)
                    .ToList();

                if (emptyKeys.Count > 0)
                    emptyTranslations.Add($"{lang}: {string.Join(", ", emptyKeys)}");
            }

            if (emptyTranslations.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    "The following keys are empty:\n" + string.Join("\n", emptyTranslations),
                    MessageType.Warning
                );
            }
        }

        /// <summary>
        /// Saves all language translations to their respective JSON files.
        /// </summary>
        private void SaveAllLanguages()
        {
            string folderPath = Path.Combine(Application.dataPath, "LocalizationStorage");

            foreach (var kvp in _allLanguages)
            {
                string path = Path.Combine(folderPath, kvp.Key + ".json");
                string json = JsonConvert.SerializeObject(kvp.Value, Formatting.Indented);
                File.WriteAllText(path, json);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Represents a single translation item with key and value.
        /// </summary>
        [Serializable]
        public class TranslationItem
        {
            public string _key;
            public string _value;
        }

        /// <summary>
        /// Wrapper for a list of translation items, representing a single language.
        /// </summary>
        [Serializable]
        public class LocalizationWrapper
        {
            public List<TranslationItem> _items;
        }
    }
}