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
    /// Custom Unity Editor Window for managing localization data.
    /// Allows viewing, adding, editing, and deleting translation keys for different languages.
    /// </summary>
    public class LocalizationEditorWindow : EditorWindow
    {
        private string _selectedLanguage = "English";
        private string[] _languages = new string[] { };
        private LocalizationWrapper _data;
        private Vector2 _scrollPos;

        private string _newKey = "";
        private string _newValue = "";
        private string _searchQuery = "";

        /// <summary>
        /// Adds a menu item in Unity to open the Localization Editor window.
        /// </summary>
        [MenuItem("Playbox/Localization/Localization Editor")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationEditorWindow>("Localization Editor");
        }

        /// <summary>
        /// Updates the list of available languages by reading JSON files from LocalizationStorage folder.
        /// </summary>
        public void UpdateLanguages()
        {
            string folderPath = Path.Combine(Application.dataPath, "LocalizationStorage");

            if (!Directory.Exists(folderPath))
                return;

            var files = Directory.GetFiles(folderPath, "*.json");

            _languages = files
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .ToArray();
        }

        /// <summary>
        /// Draws the GUI for the Localization Editor window.
        /// Handles scrolling, search, adding new words, and displaying localization items.
        /// </summary>
        void OnGUI()
        {
            DrawLanguageSelection();

            if (_data == null || _data._items == null)
                return;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawAddNewWord();
            DrawSearchField();
            DrawLocalizationList();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the dropdown to select a language and the Save button.
        /// Loads the selected language when changed.
        /// </summary>
        private void DrawLanguageSelection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Language:", GUILayout.Width(70));

            int currentIndex = Array.IndexOf(_languages, _selectedLanguage);
            int newIndex = EditorGUILayout.Popup(currentIndex, _languages);

            if (newIndex != currentIndex)
            {
                _selectedLanguage = _languages[newIndex];
                LoadLanguage();
            }

            if (GUILayout.Button("Save"))
                SaveLanguage(_selectedLanguage);

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws UI fields for adding a new translation key and value.
        /// </summary>
        private void DrawAddNewWord()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add New Word", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key", GUILayout.Width(50));
            _newKey = EditorGUILayout.TextField(_newKey, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value", GUILayout.Width(50));
            _newValue = EditorGUILayout.TextField(_newValue, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Add"))
            {
                AddNewWord();
            }
        }

        /// <summary>
        /// Draws the search field to filter localization items by key or value.
        /// </summary>
        private void DrawSearchField()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Search", EditorStyles.boldLabel);
            _searchQuery = EditorGUILayout.TextField(_searchQuery, GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// Displays all localization keys and values in a scrollable list.
        /// Allows editing values and deleting keys.
        /// </summary>
        private void DrawLocalizationList()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Localization Data", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key", GUILayout.Width(250));
            EditorGUILayout.LabelField("Value", GUILayout.Width(400));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            var filteredItems = _data._items
                .Where(x =>
                    string.IsNullOrEmpty(_searchQuery) ||
                    x._key.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x._value.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0
                )
                .ToList();

            var itemsToDelete = new List<TranslationItem>();

            foreach (var item in filteredItems)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item._key, GUILayout.Width(250));
                item._value = EditorGUILayout.TextField(item._value, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    itemsToDelete.Add(item);

                EditorGUILayout.EndHorizontal();
            }

            foreach (var item in itemsToDelete)
            {
                if (EditorUtility.DisplayDialog(
                    "Delete Localization Key",
                    $"Are you sure you want to delete '{item._key}'?",
                    "Yes", "No"))
                {
                    _data._items.Remove(item);
                }
            }

            if (itemsToDelete.Count > 0)
                SaveLanguage(_selectedLanguage);
        }

        /// <summary>
        /// Adds a new translation key and value to the current language.
        /// Ensures the key is not empty and does not already exist.
        /// </summary>
        private void AddNewWord()
        {
            if (string.IsNullOrEmpty(_newKey))
            {
                Debug.LogWarning("Key cannot be empty!");
                return;
            }

            if (_data._items.Any(x => x._key == _newKey))
            {
                Debug.LogWarning($"Key '{_newKey}' already exists!");
                return;
            }

            _data._items.Add(new TranslationItem { _key = _newKey, _value = _newValue });

            _newKey = "";
            _newValue = "";

            SaveLanguage(_selectedLanguage);
        }

        /// <summary>
        /// Called when the Editor Window is enabled.
        /// Updates the language list and loads the selected language.
        /// </summary>
        void OnEnable()
        {
            UpdateLanguages();
            LoadLanguage();
        }

        /// <summary>
        /// Loads localization data from a JSON file for the currently selected language.
        /// </summary>
        public void LoadLanguage()
        {
            string path = $"Assets/LocalizationStorage/{_selectedLanguage}.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                _data = JsonConvert.DeserializeObject<LocalizationWrapper>(json);
            }
            else
            {
                _data = new LocalizationWrapper { _items = new List<TranslationItem>() };
                Debug.LogWarning("JSON not found: " + path);
            }
        }

        /// <summary>
        /// Saves the current localization data to a JSON file for the specified language.
        /// </summary>
        /// <param name="language">The language to save (e.g., "English").</param>
        void SaveLanguage(string language)
        {
            if (_data == null || _data._items == null) return;

            string path = $"Assets/LocalizationStorage/{language}.json";
            string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Represents a single translation key-value pair.
        /// </summary>
        [Serializable]
        public class TranslationItem
        {
            public string _key;
            public string _value;
        }

        /// <summary>
        /// Wrapper for all translation items in a language.
        /// </summary>
        [Serializable]
        public class LocalizationWrapper
        {
            public List<TranslationItem> _items;
        }
    }
}