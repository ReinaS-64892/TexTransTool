#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class Localize
    {
        const string JP_GUID = "139e527c0dc01364b97daf9bdbaeb365";
        public const string LANGUAGE_PREFKEY = "net.rs64.tex-trans-tool.language";
        public const string LANGUAGE_MENU_PATH = TTTConfig.TTT_MENU_PATH + "/Language";
        private static LanguageEnum s_language;
        public static LanguageEnum Language
        {
            get => s_language;
            private set
            {
                Menu.SetChecked(LANGUAGE_MENU_PATH + "/" + s_language.ToString(), false);
                s_language = value;
                EditorPrefs.SetInt(LANGUAGE_PREFKEY, (int)s_language);
                Menu.SetChecked(LANGUAGE_MENU_PATH + "/" + s_language.ToString(), true);
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

        public static string GetLocalize(this string str)
        {
            switch (Language)
            {
                default:
                case LanguageEnum.EN:
                    {
                        return str;
                    }

                case LanguageEnum.JP:
                    {
                        if (JP == null) { JP = ParseCSV(JP_GUID); }
                        if (JP.TryGetValue(str, out var jpStr))
                        { return jpStr; }
                        else { return str; }
                    }
            }
        }

#if UNITY_EDITOR

        private static Dictionary<string, string> ParseCSV(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
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