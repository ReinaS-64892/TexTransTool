#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public static class Localize
    {
        const string EN_GUID = "bf0328b82be68d248b7472948048a317";
        const string JP_GUID = "139e527c0dc01364b97daf9bdbaeb365";
        public static LanguageEnum Language;
        public enum LanguageEnum
        {
            EN,
            JP,
        }
        [InitializeOnLoadMethod]
        static void Init()
        {
            Language = (LanguageEnum)EditorPrefs.GetInt(PrefKey);
        }

        static string[] EN;
        static string[] JP;

        public static string GetLocalize(this string Str)
        {
            switch (Language)
            {
                default:
                case LanguageEnum.EN:
                    {
                        return Str;
                    }

#if UNITY_EDITOR
                case LanguageEnum.JP:
                    {
                        if (JP == null) { InitLanguage(Language); }
                        var index = Array.IndexOf(EN, Str);
                        if (index == -1 || JP.Length <= index) { return Str; }
                        return JP[index];
                    }
#endif
            }
        }

#if UNITY_EDITOR
        static void InitLanguage(LanguageEnum language)
        {
            switch (language)
            {
                default:
                case LanguageEnum.EN:
                    { return; }
                case LanguageEnum.JP:
                    {
                        if (EN == null) { EN = File.ReadAllLines(AssetDatabase.GUIDToAssetPath(EN_GUID)); }
                        JP = File.ReadAllLines(AssetDatabase.GUIDToAssetPath(JP_GUID));
                        return;
                    }
            }

        }

        public static GUIContent ToGUIContent(this string str) => new GUIContent(str);
        public static GUIContent GetLC(this string str) => str.GetLocalize().ToGUIContent();
        public const string PrefKey = "net.rs64.tex-trans-tool.language";
        [MenuItem("Tools/TexTransTool/Language/EN")]
        public static void SwitchEN()
        {
            Language = LanguageEnum.EN;
            EditorPrefs.SetInt(PrefKey, (int)Language);
        }
        [MenuItem("Tools/TexTransTool/Language/JP")]
        public static void SwitchJP()
        {
            Language = LanguageEnum.JP;
            EditorPrefs.SetInt(PrefKey, (int)Language);
        }
#endif
    }
}
#endif