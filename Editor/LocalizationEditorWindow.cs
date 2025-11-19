using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Playbox.Localization
{
    public class LocalizationEditorWindow : EditorWindow
    {
        private string _selectedLanguage = "English";
        private string[] _languages = new string[] {};
        private LocalizationWrapper _data;
        private Vector2 _scrollPos;

        private string _newKey = "";
        private string _newValue = "";

        [MenuItem("Tools/Localization/Localization Editor")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationEditorWindow>("Localization Editor");
        }

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

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Language:", GUILayout.Width(70));

            int currentIndex = System.Array.IndexOf(_languages, _selectedLanguage);
            int newIndex = EditorGUILayout.Popup(currentIndex, _languages);

            if (newIndex != currentIndex)
            {
                _selectedLanguage = _languages[newIndex];
                LoadLanguage();
            }

            if (GUILayout.Button("Save"))
                SaveLanguage(_selectedLanguage);

            EditorGUILayout.EndHorizontal();

            if (_data != null && _data._items != null)
            {
                EditorGUILayout.Space();
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

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

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Localization Data", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Key", GUILayout.Width(250));
                EditorGUILayout.LabelField("Value", GUILayout.Width(400));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                foreach (var item in _data._items)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(item._key, GUILayout.Width(250));
                    item._value = EditorGUILayout.TextField(item._value, GUILayout.ExpandWidth(true));

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
        }


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

        void OnEnable()
        {
            UpdateLanguages();
            LoadLanguage();
        }

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

        void SaveLanguage(string language)
        {
            if (_data == null || _data._items == null) return;

            string path = $"Assets/LocalizationStorage/{language}.json";
            string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }

        [System.Serializable]
        public class TranslationItem
        {
            public string _key;
            public string _value;
        }

        [System.Serializable]
        public class LocalizationWrapper
        {
            public List<TranslationItem> _items;
        }
    }
}
