#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal static class TTTConfig
    {
        public const string TTT_MENU_PATH = "Tools/TexTransTool";
        public const string EXPERIMENTAL_MENU_PATH = TTT_MENU_PATH + "/Experimental";
        public const string DEBUG_MENU_PATH = TTT_MENU_PATH + "/Debug";


        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.delayCall += () =>
            {
                isObjectReplaceInvoke = EditorPrefs.GetBool(OBJECT_REPLACE_INVOKE_PREFKEY);
            };
        }


        #region ObjectReplaceInvoke
        public const string OBJECT_REPLACE_INVOKE_MENU_PATH = EXPERIMENTAL_MENU_PATH + "/ObjectReplaceInvoke";
        public const string OBJECT_REPLACE_INVOKE_PREFKEY = "net.rs64.tex-trans-tool.ObjectReplaceInvoke";
        private static bool s_isObjectReplaceInvoke;
        public static bool isObjectReplaceInvoke { get => s_isObjectReplaceInvoke; private set { s_isObjectReplaceInvoke = value; ObjectReplaceInvokeConfigUpdate(); } }

        [MenuItem(OBJECT_REPLACE_INVOKE_MENU_PATH)]
        static void ToggleObjectReplaceInvoke()
        {
            isObjectReplaceInvoke = !isObjectReplaceInvoke;
        }

        private static void ObjectReplaceInvokeConfigUpdate()
        {
            EditorPrefs.SetBool(OBJECT_REPLACE_INVOKE_PREFKEY, isObjectReplaceInvoke);
            Menu.SetChecked(OBJECT_REPLACE_INVOKE_MENU_PATH, isObjectReplaceInvoke);
        }
        #endregion
    }
}
#endif