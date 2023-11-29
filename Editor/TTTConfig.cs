#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public static class TTTConfig
    {
        public const string TTT_MENU_PATH = "Tools/TexTransTool";
        public const string EXPERIMENTAL_MENU_PATH = TTT_MENU_PATH + "/Experimental";


        [InitializeOnLoadMethod]
        static void Init()
        {
            isObjectReplaceInvoke = EditorPrefs.GetBool(OBJECT_REPLACE_INVOKE_PREFKEY);
        }


#region ObjectReplaceInvoke
        public const string OBJECT_REPLACE_INVOKE_MENU_PATH = EXPERIMENTAL_MENU_PATH + "/ObjectReplaceInvoke";
        public const string OBJECT_REPLACE_INVOKE_PREFKEY = "net.rs64.tex-trans-tool.ObjectReplaceInvoke";
        private static bool _isObjectReplaceInvoke;
        public static bool isObjectReplaceInvoke { get => _isObjectReplaceInvoke; private set { _isObjectReplaceInvoke = value; ObjectReplaceInvokeConfigUpdate(); } }

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