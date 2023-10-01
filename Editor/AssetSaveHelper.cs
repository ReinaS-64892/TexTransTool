#if UNITY_EDITOR
using System.Linq;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.EditorIsland;
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

        public static string GenerateAssetPath(string Name, string Extension, SaveType saveType = SaveType.Other)
        {
            SaveDirectoryCheck();
            var path = GenerateFullPath(Name, saveType);
            path += Extension;
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
        private static string GenerateFullPath(string Name, SaveType saveType = SaveType.Other)
        {
            var replacedName = Name.Replace("(Clone)", "");
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
                        savePath = GenerateAssetPath(Target.name, ".asset", GetSaveTypeAtAsset(Target));
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
            var saveType = GetSaveTypeAtAsset<T>();
            List<T> LoadedAssets = new List<T>();
            foreach (var path in Directory.GetFiles(GetDirectoryAtType(saveType)))
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
#endif
