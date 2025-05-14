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

        MenuState _state;
        enum MenuState
        {
            Config,
            // TODO : Component を生成できるメニューで、 MenuItem よりもいい感じにできたらいいな
            // Component,
            Debug,
        }
        Dictionary<MenuState, ITTTConfigWindow> _menus = new(){
            {MenuState.Config, new TTTConfigMenu()},
            {MenuState.Debug, new TTTDebugMenu()},
        };

        Dictionary<MenuState, string> _menuNameCache = new();

        public void OnGUI()
        {
            EditorGUIUtility.labelWidth = 256f;
            using (new EditorGUILayout.HorizontalScope())
                foreach (var menuState in _menus.Keys)
                {
                    if (_menuNameCache.TryGetValue(menuState, out var menuName) is false) { menuName = menuState.ToString(); }
                    if (GUILayout.Button(menuName)) { _state = menuState; }
                }


            if (_menus.TryGetValue(_state, out var tttMenu))
                tttMenu.OnGUI();
        }

        internal interface ITTTConfigWindow
        {
            void OnGUI();
        }
    }
}
