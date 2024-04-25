using UnityEditor;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransTool;
using UnityEngine.UIElements;
using net.rs64.TexTransTool.Build;
using System.Collections.Generic;
using System;
using UnityEngine;
using net.rs64.TexTransTool.Preview.Custom;
using net.rs64.TexTransTool.Preview;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(PreviewRenderer))]
    internal class PreviewRendererEditor : UnityEditor.Editor
    {
        protected HashSet<Action> _disableActions = new();
        public override VisualElement CreateInspectorGUI()
        {
            TexTransGroupEditor.LoadStyle();

            var rootVE = new VisualElement();


            CrateElements();
            EditorApplication.hierarchyChanged += CrateElements;
            _disableActions.Add(() => EditorApplication.hierarchyChanged -= CrateElements);

            return rootVE;


            void CrateElements()
            {
                rootVE.hierarchy.Clear();

                var previewButton = new IMGUIContainer(() => { TextureTransformerEditor.DrawerWarning(nameof(PreviewRenderer)); OneTimePreviewContext.instance.DrawApplyAndRevert(target as PreviewRenderer); });

                rootVE.hierarchy.Add(previewButton);
                rootVE.styleSheets.Add(TexTransGroupEditor.s_style);

                var previewRenderer = (PreviewRenderer)target;
                var domainRoot = DomainMarkerFinder.FindMarker(previewRenderer.gameObject);

                TexTransGroupEditor.CreateGroupElements(rootVE, PreviewRendererPreview.FindAtTargetRenderer(previewRenderer.GetComponent<Renderer>(), domainRoot));
            }
        }

        private void OnDisable() { foreach (var i in _disableActions) { i.Invoke(); } _disableActions.Clear(); }
    }
}
