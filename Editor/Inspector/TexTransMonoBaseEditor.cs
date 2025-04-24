using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransTool.Preview.RealTime;
using UnityEngine.UIElements;

namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TexTransMonoBase), true)]
    internal class TexTransMonoBaseEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var ttMonoBase = target as TexTransMonoBase;
            if (ttMonoBase == null) { return new VisualElement(); }// 通常ありえないコードパス
            
            var root = new VisualElement();
            
            var inspectorContainer = new IMGUIContainer(OnInspectorGUI);
            root.Add(inspectorContainer);

            var headerContainer = new VisualElement();
            headerContainer.AddToClassList("header-container");
            headerContainer.Add(new IMGUIContainer(() =>
            {
                DrawOldSaveDataVersionWarning(ttMonoBase);
                DrawerWarning(ttMonoBase);
            }));
            headerContainer.Add(Separator());
            root.Insert(0, headerContainer);

            if (DrawPreviewButton)
            {
                var previewContainer = new IMGUIContainer(() =>
                {
                    PreviewButtonDrawUtil.Draw(ttMonoBase);
                });
                root.Add(previewContainer);
            }
            
            var footerContainer = new VisualElement();
            footerContainer.AddToClassList("footer-container");
            footerContainer.Add(Separator());
            footerContainer.Add(new IMGUIContainer(DrawLangSelector));
            root.Add(footerContainer);
            
            return root;

            VisualElement Separator()
            {
                return new VisualElement {
                    style = {
                        height = 1,
                        marginTop = 5,
                        marginBottom = 5,
                        backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1)
                    }
                };   
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            OnTexTransComponentInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }
        
        protected virtual bool DrawPreviewButton => true;

        protected virtual void OnTexTransComponentInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private static void DrawLangSelector()
        {
            if (Localize.Languages is null) return;
            
            var globalSettings = TTTGlobalConfig.instance;
            var bIndex = Array.IndexOf(Localize.Languages, globalSettings.Language);
            var aIndex = EditorGUILayout.Popup("Language", bIndex, Localize.Languages);
            if (bIndex != aIndex) { globalSettings.Language = Localize.Languages[aIndex]; }
        }
        
        public static void DrawerWarning(TexTransMonoBase ttMonoBase)
        {
            if (ttMonoBase is ITexTransToolStableComponent) { return; }

            var typeName = ttMonoBase.GetType().Name;
            EditorGUILayout.HelpBox(typeName + " " + "Common:ExperimentalWarning".GetLocalize(), MessageType.Warning);
        }
        public static void DrawOldSaveDataVersionWarning(TexTransMonoBase ttMonoBase)
        {
            if (ttMonoBase is ITexTransToolStableComponent texTransToolStableComponent)
            {
                if ((ttMonoBase as ITexTransToolTag).SaveDataVersion < texTransToolStableComponent.StabilizeSaveDataVersion)
                    DrawMigratorWindowButton();
            }
            else
            {
                if ((ttMonoBase as ITexTransToolTag).SaveDataVersion < TexTransMonoBase.TTTDataVersion)
                    DrawMigratorWindowButton();
            }

            void DrawMigratorWindowButton()
            {
                if (GUILayout.Button("Common:button:ThisComponentSaveDataIsOldOpenMigratorWindow".Glc()))
                    Migration.MigratorWindow.ShowWindow();
            }
        }
    }
}