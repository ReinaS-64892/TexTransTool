using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEditor;
using UnityEngine;
using static net.rs64.TexTransTool.TTTConfig;

namespace net.rs64.TexTransTool
{
    internal static class Localize
    {
        const string JA_GUID = "42db3dbd5755c844984648836a49629f";
        const string EN_GUID = "b6008be0d5fa3d242ba93f9a930df3c3";
        [TexTransInitialize]
        internal static void LocalizeInitializer()
        {
            Language = (LanguageEnum)EditorPrefs.GetInt(TTTConfig.LANGUAGE_PREFKEY);
        }
        public static void LoadLocalize()
        {
            LocalizationAssets[LanguageEnum.JA] = AssetDatabase.LoadAssetAtPath<LocalizationAsset>(AssetDatabase.GUIDToAssetPath(JA_GUID));
            LocalizationAssets[LanguageEnum.EN] = AssetDatabase.LoadAssetAtPath<LocalizationAsset>(AssetDatabase.GUIDToAssetPath(EN_GUID));
        }
        internal static readonly Dictionary<LanguageEnum, LocalizationAsset> LocalizationAssets = new();

        public static string GetLocalize(this string str)
        {
            if (!LocalizationAssets.ContainsKey(Language)) { LoadLocalize(); }
            return LocalizationAssets[Language].GetLocalizedString(str);
        }
        public static GUIContent GlcV(this string str)
        {
            var c = str.Glc();
            c.image = TTTImageAssets.VramICon;
            return c;
        }

        public static GUIContent Glc(this string str)
        {
            var tooltipKey = str + ":tooltip";
            var tooltipStr = tooltipKey.GetLocalize();
            if (tooltipKey == tooltipStr) { tooltipStr = ""; }
            return new GUIContent(str.GetLocalize(), tooltipStr);
        }
        public static GUIContent Glf(this string str, params object[] objects)
        {
            var cuiContent = str.Glc();
            cuiContent.text = string.Format(cuiContent.text, objects);
            cuiContent.tooltip = string.Format(cuiContent.tooltip, objects);
            return cuiContent;
        }
    }
}

