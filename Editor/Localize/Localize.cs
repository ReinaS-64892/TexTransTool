#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public static class Localize
    {
        const string JP_GUID = "139e527c0dc01364b97daf9bdbaeb365";
        public const string LANGUAGE_PREFKEY = "net.rs64.tex-trans-tool.language";
        public const string LANGUAGE_MENU_PATH = TTTConfig.TTT_MENU_PATH + "/Language";
        private static LanguageEnum _Language;
        public static LanguageEnum Language
        {
            get => _Language;
            private set
            {
                Menu.SetChecked(LANGUAGE_MENU_PATH + "/" + _Language.ToString(), false);
                _Language = value;
                EditorPrefs.SetInt(LANGUAGE_PREFKEY, (int)_Language);
                Menu.SetChecked(LANGUAGE_MENU_PATH + "/" + _Language.ToString(), true);
            }
        }
        public enum LanguageEnum
        {
            EN,
            JP,
        }
        [InitializeOnLoadMethod]
        static void Init()
        {
            Language = (LanguageEnum)EditorPrefs.GetInt(LANGUAGE_PREFKEY);
        }

        static Dictionary<string, string> JP;

        public static string GetLocalize(this string Str)
        {
            switch (Language)
            {
                default:
                case LanguageEnum.EN:
                    {
                        return Str;
                    }

                case LanguageEnum.JP:
                    {
                        if (JP == null) { JP = ParseCSV(JP_GUID); }
                        if (JP.TryGetValue(Str, out var jpStr))
                        { return jpStr; }
                        else { return Str; }
                    }
            }
        }

#if UNITY_EDITOR

        private static Dictionary<string, string> ParseCSV(string GUID)
        {
            var path = AssetDatabase.GUIDToAssetPath(GUID);
            var strPears = File.ReadAllLines(path);
            var strDict = new Dictionary<string, string>();
            foreach (var strPear in strPears)
            {
                var Pear = strPear.Split(',');
                strDict.Add(Pear[0], Pear[1]);
            }
            return strDict;
        }
        public static GUIContent ToGUIContent(this string str) => new GUIContent(str);
        public static GUIContent GetLC(this string str) => str.GetLocalize().ToGUIContent();

        [MenuItem(LANGUAGE_MENU_PATH + "/EN")]
        public static void SwitchEN() => Language = LanguageEnum.EN;
        [MenuItem(LANGUAGE_MENU_PATH + "/JP")]
        public static void SwitchJP() => Language = LanguageEnum.JP;
#endif
    }
}
#endif