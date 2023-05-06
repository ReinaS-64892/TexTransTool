#if UNITY_EDITOR
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
        public const string SaveDirectory = "Assets/TexTransToolGanareats";
        public static void SaveDirectoryCheck()
        {
            if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);
        }
        public static string GeneretNewSavePath(string Name)
        {
            return SaveDirectory + "/" + Name + "_" + Guid.NewGuid().ToString();
        }
        public static List<T> SaveAssets<T>(IEnumerable<T> Targets) where T : UnityEngine.Object
        {
            SaveDirectoryCheck();
            List<T> SavedTextures = new List<T>();
            foreach (var Target in Targets)
            {
                SavedTextures.Add(SaveAsset<T>(Target));
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
            var SavePath = GeneretNewSavePath(Target.name);
            switch (Target)
            {
                default:
                    {
                        SavePath += ".asset";
                        AssetDatabase.CreateAsset(Target, SavePath);
                        break;
                    }
                case Texture2D Tex2d:
                    {
                        SavePath += ".png";
                        File.WriteAllBytes(SavePath, Tex2d.EncodeToPNG());
                        break;
                    }
                case Material Mat:
                    {
                        SavePath += ".mat";
                        AssetDatabase.CreateAsset(Target, SavePath);
                        break;
                    }
            }
            AssetDatabase.ImportAsset(SavePath);
            return AssetDatabase.LoadAssetAtPath<T>(SavePath);
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
            var path = AssetDatabase.GetAssetPath(Target);
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

    }
}
#endif