#nullable enable
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
        static List<ITTTMenuWindow> _menus = new() { new TTTConfigMenu() };
        public void CreateGUI()
        {
            rootVisualElement.Clear();

            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;
            rootVisualElement.hierarchy.Add(root);

            var utilitiesScrollView = new ScrollView();
            var scrollViewContainer = utilitiesScrollView.Q<VisualElement>("unity-content-container");
            scrollViewContainer.style.flexDirection = FlexDirection.Row;
            root.hierarchy.Add(utilitiesScrollView);

            var utilityPanel = new VisualElement();
            utilityPanel.style.width = Length.Percent(100);
            root.hierarchy.Add(utilityPanel);


            utilityPanel.hierarchy.Add(_menus.First().CreateGUI());
            foreach (var menu in _menus)
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
            _menus.Add(menuWindow);
        }
    }
}
