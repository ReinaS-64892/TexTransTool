using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace net.rs64.TexTransTool
{
    public static class AssetSaveHelper
    {
        static bool _IsTemporary;
        public static bool IsTemporary
        {
            get => _IsTemporary;
            set
            {
                _IsTemporary = value;
                if (!_IsTemporary)
                {
                    ClearTemp();
                }
            }
        }
        public const string SaveDirectory = "Assets/TexTransToolGenerates";
        public const string TempDirName = "TempDirectory";
        public static string GenerateAssetPath(string Name, string Extension)
        {
            SaveDirectoryCheck();
            var path = GenerateFullPath(Name);
            path += Extension;
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            return path;
        }
        private static void SaveDirectoryCheck()
        {
            if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);
            if (!Directory.Exists(Path.Combine(SaveDirectory, TempDirName))) Directory.CreateDirectory(Path.Combine(SaveDirectory, TempDirName));
        }
        private static string GenerateFullPath(string Name)
        {
            var replacedName = Name.Replace("(Clone)", "");
            replacedName = string.IsNullOrWhiteSpace(replacedName) ? "GenerateAsset" : replacedName;
            var parentPath = !IsTemporary ? SaveDirectory : Path.Combine(SaveDirectory, TempDirName);
            return Path.Combine(parentPath, replacedName);
        }
        public static void SaveAssets<T>(IEnumerable<T> Targets) where T : UnityEngine.Object
        {
            foreach (var target in Targets)
            {
                SaveAsset(target);
            }
        }

        public static void SaveAsset<T>(T Target) where T : UnityEngine.Object
        {
            if (Target == null) { return; }
            if (AssetDatabase.Contains(Target)) { return; }
            string savePath = null;
            switch (Target)
            {
                default:
                    {
                        savePath = GenerateAssetPath(Target.name, ".asset");
                        break;
                    }
                case Material Mat:
                    {
                        savePath = GenerateAssetPath(Target.name, ".mat");
                        break;
                    }
            }
            AssetDatabase.CreateAsset(Target, savePath);
            AssetDatabase.ImportAsset(savePath);
        }

        public static Texture2D SavePng(Texture2D Target)
        {
            if (Target == null) { return null; }

            var savePath = GenerateAssetPath(Target.name, ".png");
            File.WriteAllBytes(savePath, Target.EncodeToPNG());
            AssetDatabase.ImportAsset(savePath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);

        }

        public static void DeleteAssets<T>(IEnumerable<T> Targets) where T : UnityEngine.Object
        {
            foreach (var target in Targets)
            {
                DeleteAsset(target);
            }
        }
        public static void DeleteAsset<T>(T Target) where T : UnityEngine.Object
        {
            if (Target == null) return;
            var path = AssetDatabase.GetAssetPath(Target);
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        public static List<T> LoadAssets<T>() where T : UnityEngine.Object
        {
            SaveDirectoryCheck();
            List<T> LoadedAssets = new List<T>();
            foreach (var path in Directory.GetFiles(SaveDirectory))
            {
                if (AssetDatabase.LoadAssetAtPath(path, typeof(T)) is T instants)
                {
                    LoadedAssets.Add(instants);
                }
            }
            return LoadedAssets;
        }
        private static void ClearTemp()
        {
            var tempPath = Path.Combine(SaveDirectory, TempDirName);
            foreach (var path in Directory.GetFiles(tempPath))
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                AssetDatabase.DeleteAsset(path);
            }

        }
    }
}