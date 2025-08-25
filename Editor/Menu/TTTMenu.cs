#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [EditorWindowTitle(title = "TTT Menu")]
    internal sealed class TTTMenu : EditorWindow
    {

        [MenuItem("Tools/TexTransTool/Menu")]
        public static void ShowWindow()
        {
            GetWindow<TTTMenu>();
        }

        int _state = 0;
       static List<ITTTMenuWindow> _menus = new() { new TTTConfigMenu(), new TTTDebugMenu() };

        public void OnGUI()
        {
            EditorGUIUtility.labelWidth = 256f;
            using (new EditorGUILayout.HorizontalScope())
                for (var i = 0; _menus.Count > i; i += 1)
                {
                    if (GUILayout.Button(_menus[i].MenuName)) { _state = i; }
                }

            _menus[_state].OnGUI();
        }

        internal interface ITTTMenuWindow
        {
            string MenuName { get; }
            void OnGUI();
        }

        internal static void RegisterMenu(ITTTMenuWindow menuWindow)
        {
            _menus.Add(menuWindow);
        }
    }
}
