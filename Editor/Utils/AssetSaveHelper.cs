using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.EditorIsland;
namespace net.rs64.TexTransTool
{
    internal static class AssetSaveHelper
    {
        static bool s_isTemporary;
        public static bool IsTemporary
        {
            get => s_isTemporary;
            set
            {
                s_isTemporary = value;
                if (!s_isTemporary)
                {
                    ClearTemp();
                }
                else
                {
                    SaveDirectoryCheck();
                }
            }
        }
        public const string SaveDirectory = "Assets/TexTransToolGenerates";
        public const string IslandCaches = "IslandCaches";
        public const string AvatarDomainAssets = "AvatarDomainAssets";
        public const string Temp = "TempDirectory";

        public enum SaveType
        {
            Other,
            IslandCaches,
            AvatarDomainAssets,
        }

        public static string GenerateAssetPath(string name, string extension, SaveType saveType = SaveType.Other)
        {
            SaveDirectoryCheck();
            var path = GenerateFullPath(name, saveType);
            path += extension;
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            return path;
        }
        private static void SaveDirectoryCheck()
        {
            if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);
            if (!Directory.Exists(Path.Combine(SaveDirectory, Temp))) Directory.CreateDirectory(Path.Combine(SaveDirectory, Temp));
            if (!Directory.Exists(Path.Combine(SaveDirectory, IslandCaches))) Directory.CreateDirectory(Path.Combine(SaveDirectory, IslandCaches));
            if (!Directory.Exists(Path.Combine(SaveDirectory, AvatarDomainAssets))) Directory.CreateDirectory(Path.Combine(SaveDirectory, AvatarDomainAssets));
        }
        private static string GenerateFullPath(string name, SaveType saveType = SaveType.Other)
        {
            var replacedName = name.Replace("(Clone)", "");
            replacedName = string.IsNullOrWhiteSpace(replacedName) ? "GenerateAsset" : replacedName;
            string parentPath;
            if (!IsTemporary)
            {
                parentPath = GetDirectoryAtType(saveType);
            }
            else
            {
                parentPath = Path.Combine(SaveDirectory, Temp);
            }

            return Path.Combine(parentPath, replacedName);
        }
        private static string GetDirectoryAtType(SaveType saveType)
        {
            switch (saveType)
            {
                default:
                case SaveType.Other:
                    return SaveDirectory;
                case SaveType.IslandCaches:
                    return Path.Combine(SaveDirectory, IslandCaches);
                case SaveType.AvatarDomainAssets:
                    return Path.Combine(SaveDirectory, AvatarDomainAssets);
            }
        }
        private static SaveType GetSaveTypeAtAsset<T>(T asset = null) where T : class
        {
            var type = typeof(T);
            if (type == typeof(IslandCache))
            {
                return SaveType.IslandCaches;
            }
            else if (type == typeof(AvatarDomainAsset))
            {
                return SaveType.AvatarDomainAssets;
            }
            else
            {
                return SaveType.Other;
            }

        }
        public static void SaveAssets<T>(IEnumerable<T> targets) where T : UnityEngine.Object
        {
            foreach (var target in targets)
            {
                SaveAsset(target);
            }
        }

        public static void SaveAsset<T>(T target) where T : UnityEngine.Object
        {
            if (target == null) { return; }
            if (AssetDatabase.Contains(target)) { return; }
            string savePath = null;
            switch (target)
            {
                default:
                    {
                        savePath = GenerateAssetPath(target.name, ".asset", GetSaveTypeAtAsset(target));
                        break;
                    }
                case Material Mat:
                    {
                        savePath = GenerateAssetPath(target.name, ".mat");
                        break;
                    }
            }
            AssetDatabase.CreateAsset(target, savePath);
            AssetDatabase.ImportAsset(savePath);
        }

        public static Texture2D SavePng(Texture2D target)
        {
            if (target == null) { return null; }

            var savePath = GenerateAssetPath(target.name, ".png");
            File.WriteAllBytes(savePath, target.EncodeToPNG());
            AssetDatabase.ImportAsset(savePath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);

        }

        public static void DeleteAssets<T>(IEnumerable<T> targets) where T : UnityEngine.Object
        {
            foreach (var target in targets)
            {
                DeleteAsset(target);
            }
        }
        public static void DeleteAsset<T>(T target) where T : UnityEngine.Object
        {
            if (target == null) return;
            var path = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        public static List<T> LoadAssets<T>() where T : UnityEngine.Object
        {
            SaveDirectoryCheck();
            var saveType = GetSaveTypeAtAsset<T>();
            List<T> loadedAssets = new();
            foreach (var path in Directory.GetFiles(GetDirectoryAtType(saveType)))
            {
                if (AssetDatabase.LoadAssetAtPath(path, typeof(T)) is T instants)
                {
                    loadedAssets.Add(instants);
                }
            }
            return loadedAssets;
        }
        private static void ClearTemp()
        {
            SaveDirectoryCheck();
            var tempPath = Path.Combine(SaveDirectory, Temp);
            foreach (var path in Directory.GetFiles(tempPath))
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                AssetDatabase.DeleteAsset(path);
            }

        }
    }
}
