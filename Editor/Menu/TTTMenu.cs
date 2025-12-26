#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
        public static void ShowWindow(Type? openMenu = null)
        {
            var window = GetWindow<TTTMenu>();
            if (openMenu is not null) window.ShowMenu(openMenu);
        }

        private VisualElement? utilityPanel;
        private void ShowMenu(Type openMenu)
        {
            if (utilityPanel is null) { return; }

            var menu = _menus.FirstOrDefault(m => m.GetType() == openMenu);
            if (menu is null) { return; }

            utilityPanel.hierarchy.Clear();
            utilityPanel.hierarchy.Add(menu.CreateGUI());
        }

        static List<ITTTMenuWindow> _globalMenus = new() { new TTTConfigMenu() };
        List<ITTTMenuWindow>? _menus;
        public void InitializeMenuList(IEnumerable<ITTTMenuWindow>? insert = null)
        {
            if (insert is null)
            {
                if (_menus is null)
                {
                    _menus = _globalMenus.ToList();
                }
            }
            else
            {
                if (_menus is null)
                {
                    _menus = insert.Concat(_globalMenus).ToList();
                }
                else
                {
                    _menus.InsertRange(0, insert);
                }
            }
        }
        public void CreateGUI()
        {
            rootVisualElement.Clear();

            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;
            rootVisualElement.hierarchy.Add(root);

            var utilitiesScrollView = new ScrollView();
            utilitiesScrollView.style.flexShrink = 0;
            var scrollViewContainer = utilitiesScrollView.Q<VisualElement>("unity-content-container");
            scrollViewContainer.style.flexDirection = FlexDirection.Row;
            root.hierarchy.Add(utilitiesScrollView);

            utilityPanel = new VisualElement();
            utilityPanel.style.width = Length.Percent(100);
            root.hierarchy.Add(utilityPanel);

            InitializeMenuList();
            utilityPanel.hierarchy.Add(_menus!.First().CreateGUI());
            foreach (var menu in _menus!)
            {
                var button = new Button();
                button.text = menu.MenuName;
                button.clicked += () =>
                {
                    utilityPanel.hierarchy.Clear();
                    utilityPanel.hierarchy.Add(menu.CreateGUI());
                };

                scrollViewContainer.hierarchy.Add(button);
            }
        }

        internal interface ITTTMenuWindow
        {
            string MenuName { get; }
            VisualElement CreateGUI()
            {
                return new IMGUIContainer(OnGUI);
            }
            void OnGUI() { }
        }

        internal static void RegisterMenu(ITTTMenuWindow menuWindow)
        {
            _globalMenus.Add(menuWindow);
        }
    }
}
