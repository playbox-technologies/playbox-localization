using UnityEditor;
using UnityEngine;
using System.IO;

namespace Playbox.Localization.Editor
{
    [InitializeOnLoad]
    public static class LocalizationFolderInitializer
    {
        static LocalizationFolderInitializer() => EditorApplication.delayCall += EnsureFolders;

        private static void EnsureFolders()
        {
            string rootFolder = Path.Combine(Application.dataPath, "LocalizationStorage");

            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
                AssetDatabase.Refresh();
            }
        }
    }
}