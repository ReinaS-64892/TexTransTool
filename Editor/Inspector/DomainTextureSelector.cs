using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace net.rs64.TexTransTool.Editor
{
    internal class DomainTextureSelector : EditorWindow
    {
        [SerializeField] StyleSheet styleSheet;
        public static void OpenSelector(SerializedProperty textureProperty, Component component)
        {
            var window = GetWindow<DomainTextureSelector>();
            if (window == null) { return; }

            window.initialize(textureProperty, component);
        }

        private void initialize(SerializedProperty textureProperty, Component component)
        {
            rootVisualElement.Clear();

            TextureProperty = textureProperty;
            var domainObject = DomainMarkerFinder.FindMarker(component.gameObject);
            DomainContainsTextures = domainObject.GetComponentsInChildren<Renderer>(true)
            .SelectMany(i => i.sharedMaterials).SelectMany(i => i.GetAllTexture2D()).GroupBy(i => i.Key).ToDictionary(i => i.Key, i => i.Select(k => k.Value).Distinct().ToList());

            rootVisualElement.styleSheets.Add(styleSheet);

            var button = new Button(Close);
            button.text = "Close";
            rootVisualElement.Add(button);

            var scrollView = new ScrollView();
            var content = scrollView.Q<VisualElement>("unity-content-container");
            foreach (var texKV in DomainContainsTextures)
            {
                var box = new VisualElement();
                box.AddToClassList("TextureView");
                foreach (var tex in texKV.Value)
                {
                    var imageBox = new VisualElement();
                    imageBox.focusable = true;
                    imageBox.RegisterCallback<ClickEvent>(_ => SetTextureCall(tex));

                    imageBox.AddToClassList("TextureBox");
                    var texVE = new Image();
                    texVE.image = tex;
                    texVE.AddToClassList("Texture");
                    imageBox.hierarchy.Add(texVE);
                    imageBox.hierarchy.Add(new Label(tex.name));
                    box.hierarchy.Add(imageBox);


                }
                var propLabel = new Label(texKV.Key);
                propLabel.AddToClassList("PropertyText");
                content.hierarchy.Add(propLabel);
                content.hierarchy.Add(box);
            }
            rootVisualElement.Add(scrollView);

        }
        SerializedProperty TextureProperty;
        Dictionary<string, List<Texture2D>> DomainContainsTextures;

        void SetTextureCall(Texture2D texture2D)
        {
            TextureProperty.serializedObject.Update();
            TextureProperty.objectReferenceValue = texture2D;
            TextureProperty.serializedObject.ApplyModifiedProperties();

            if (doubleClickTime > 0) { Close(); return; }
            else { doubleClickTime = doubleClickWaitTime; }
        }
        const float doubleClickWaitTime = 1f;
        float doubleClickTime;
        private void Update()
        {
            doubleClickTime = Mathf.Max(doubleClickTime - Time.deltaTime, 0);
        }

    }
}
