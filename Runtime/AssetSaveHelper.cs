#if UNITY_EDITOR
using System.Linq;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace net.rs64.TexTransTool
{
    public static class AssetSaveHelper
    {
        static bool _IsTmplaly;
        public static bool IsTmplaly
        {
            get => _IsTmplaly;
            set
            {
                _IsTmplaly = value;
                if (!_IsTmplaly)
                {
                    ClearTemp();
                }
            }
        }
        public const string SaveDirectory = "Assets/TexTransToolGanareats";
        public const string TempDirName = "TempDirectory";
        public static string GenerateAssetPath(string Name, string Exttion)
        {
            SaveDirectoryCheck();
            var Base = GenerateFullPath(Name);
            Base += Exttion;
            Base = AssetDatabase.GenerateUniqueAssetPath(Base);
            return Base;
        }
        private static void SaveDirectoryCheck()
        {
            if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);
            if (!Directory.Exists(Path.Combine(SaveDirectory, TempDirName))) Directory.CreateDirectory(Path.Combine(SaveDirectory, TempDirName));
        }
        private static string GenerateFullPath(string Name)
        {
            var replacedname = Name.Replace("(Clone)", "");
            replacedname = string.IsNullOrWhiteSpace(replacedname) ? "GanaraetAsset" : replacedname;
            var parentpath = !IsTmplaly ? SaveDirectory : Path.Combine(SaveDirectory, TempDirName);
            return Path.Combine(parentpath, replacedname);
        }
        public static void SaveAssets<T>(IEnumerable<T> Targets) where T : UnityEngine.Object
        {
            foreach (var Target in Targets)
            {
                SaveAsset(Target);
            }
        }

        public static void SaveAsset<T>(T Target) where T : UnityEngine.Object
        {
            if (Target == null) { return; }
            if (AssetDatabase.Contains(Target)) { return; }
            string SavePath = null;
            switch (Target)
            {
                default:
                    {
                        SavePath = GenerateAssetPath(Target.name, ".asset");
                        AssetDatabase.CreateAsset(Target, SavePath);
                        break;
                    }
                case Material Mat:
                    {
                        SavePath = GenerateAssetPath(Target.name, ".mat");
                        AssetDatabase.CreateAsset(Target, SavePath);
                        break;
                    }
            }
            AssetDatabase.ImportAsset(SavePath);
        }

        public static Texture2D SavePng(Texture2D Target)
        {
            if (Target == null) { return null; }

            var SavePath = GenerateAssetPath(Target.name, ".png");
            File.WriteAllBytes(SavePath, Target.EncodeToPNG());
            AssetDatabase.ImportAsset(SavePath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(SavePath);

        }

        public static void DeletAssets<T>(IEnumerable<T> Targets) where T : UnityEngine.Object
        {
            foreach (var target in Targets)
            {
                DeletAsset(target);
            }
        }
        public static void DeletAsset<T>(T Target) where T : UnityEngine.Object
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
                if (AssetDatabase.LoadAssetAtPath(path, typeof(T)) is T tinstans)
                {
                    LoadedAssets.Add(tinstans);
                }
            }
            return LoadedAssets;
        }
        private static void ClearTemp()
        {
            var temppath = Path.Combine(SaveDirectory, TempDirName);
            foreach (var path in Directory.GetFiles(temppath))
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                AssetDatabase.DeleteAsset(path);
            }

        }
    }
}
#endif
