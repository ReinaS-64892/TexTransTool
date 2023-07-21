#if UNITY_EDITOR
using System.Linq;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Rs64.TexTransTool
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
        public static void SaveDirectoryCheck()
        {
            if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);
            if (!Directory.Exists(Path.Combine(SaveDirectory, TempDirName))) Directory.CreateDirectory(Path.Combine(SaveDirectory, TempDirName));
        }
        public static string GenereatFullPath(string Name)
        {
            var replacedname = Name.Replace("(Clone)", "");
            var parentpaht = !IsTmplaly ? SaveDirectory : Path.Combine(SaveDirectory, TempDirName);
            return Path.Combine(parentpaht, replacedname);
        }
        public static List<T> SaveAssets<T>(IEnumerable<T> Targets) where T : UnityEngine.Object
        {
            SaveDirectoryCheck();
            List<T> SavedTextures = new List<T>();
            foreach (var Target in Targets)
            {
                SavedTextures.Add(SaveAsset(Target));
            }
            return SavedTextures;
        }

        public static T SaveAsset<T>(T Target) where T : UnityEngine.Object
        {
            SaveDirectoryCheck();
            if (Target == null)
            {
                return null;
            }
            if (AssetDatabase.Contains(Target))
            {
                return Target;
            }
            var SavePath = GenereatFullPath(Target.name);
            switch (Target)
            {
                default:
                    {
                        SavePath += ".asset";
                        SavePath = AssetDatabase.GenerateUniqueAssetPath(SavePath);
                        AssetDatabase.CreateAsset(Target, SavePath);
                        break;
                    }
                case Texture2D Tex2d:
                    {
                        SavePath += ".png";
                        SavePath = AssetDatabase.GenerateUniqueAssetPath(SavePath);
                        File.WriteAllBytes(SavePath, Tex2d.EncodeToPNG());
                        break;
                    }
                case Material Mat:
                    {
                        SavePath += ".mat";
                        SavePath = AssetDatabase.GenerateUniqueAssetPath(SavePath);
                        AssetDatabase.CreateAsset(Target, SavePath);
                        break;
                    }
            }
            AssetDatabase.ImportAsset(SavePath);
            switch (Target)
            {
                default:
                    {
                        return Target;
                    }
                case Texture2D Tex2d:
                    {
                        return AssetDatabase.LoadAssetAtPath<Texture2D>(SavePath) as T;
                    }

            }
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

        public static void SaveSubAsset<T>(UnityEngine.Object MainAsset, T SubAssets) where T : UnityEngine.Object
        {
            AssetDatabase.AddObjectToAsset(SubAssets, MainAsset);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(MainAsset));
        }
        public static void SaveSubAssets<T>(UnityEngine.Object MainAsset, IEnumerable<T> SubAssets) where T : UnityEngine.Object
        {
            foreach (var SubAsset in SubAssets)
            {
                SaveSubAsset(MainAsset, SubAsset);
            }
        }
        public static void ClearSubAssets(UnityEngine.Object MainAsset)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(MainAsset)))
            {
                DeletSubAsset(asset);
            }
        }
        public static void DeletSubAsset(UnityEngine.Object asset)
        {
            if (AssetDatabase.IsSubAsset(asset))
            {
                UnityEngine.Object.DestroyImmediate(asset, true);
            }
        }
        public static void DeletSubAssets(IEnumerable<UnityEngine.Object> assets)
        {
            foreach (var asset in assets)
            {
                DeletSubAsset(asset);
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
        public static void SaveUnsavedAssets<T>(IEnumerable<T> assets) where T : UnityEngine.Object
        {
            foreach (var asset in assets)
            {
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset)))
                {
                    SaveAsset(asset);
                }
            }
        }

        public static void SaveAsset(string Name, string String)
        {
            var path = GenereatFullPath(Name) + ".txt";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            File.WriteAllText(path, String);
        }

        public static void ClearTemp()
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