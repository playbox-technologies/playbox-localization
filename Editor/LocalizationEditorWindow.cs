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
        private string selectedLanguage = "English";
        private string[] languages = new string[] {};
        private LocalizationWrapper data;
        private Vector2 scrollPos;

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

            languages = files
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .ToArray();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Language:", GUILayout.Width(70));

            int currentIndex = System.Array.IndexOf(languages, selectedLanguage);
            int newIndex = EditorGUILayout.Popup(currentIndex, languages);

            if (newIndex != currentIndex)
            {
                selectedLanguage = languages[newIndex];
                LoadLanguage();
            }

            if (GUILayout.Button("Save"))
                SaveLanguage(selectedLanguage);

            EditorGUILayout.EndHorizontal();

            if (data != null && data._items != null)
            {
                EditorGUILayout.Space();
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Key", GUILayout.Width(250));
                EditorGUILayout.LabelField("Value", GUILayout.Width(400));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                foreach (var item in data._items)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(item._key, GUILayout.Width(250));
                    item._value = EditorGUILayout.TextField(item._value, GUILayout.Width(400));

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        void OnEnable()
        {
            UpdateLanguages();
            LoadLanguage();
        }

        public void LoadLanguage()
        {
            string path = $"Assets/LocalizationStorage/{selectedLanguage}.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                data = JsonConvert.DeserializeObject<LocalizationWrapper>(json);
            }
            else
            {
                data = new LocalizationWrapper { _items = new List<TranslationItem>() };
                Debug.LogWarning("JSON not found: " + path);
            }
        }

        void SaveLanguage(string language)
        {
            if (data == null || data._items == null) return;

            string path = $"Assets/LocalizationStorage/{language}.json";
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
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
