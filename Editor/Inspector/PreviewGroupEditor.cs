using UnityEditor;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransTool;
using UnityEngine.UIElements;
using net.rs64.TexTransTool.Build;
using System.Collections.Generic;
using net.rs64.TexTransTool.Preview;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(PreviewGroup))]
    internal class PreviewGroupEditor : TexTransGroupEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            LoadStyle();

            var rootVE = new VisualElement();


            CrateGroupElements();
            EditorApplication.hierarchyChanged += CrateGroupElements;
            _disableActions.Add(() => EditorApplication.hierarchyChanged -= CrateGroupElements);

            return rootVE;


            void CrateGroupElements()
            {
                rootVE.hierarchy.Clear();

                var previewButton = new IMGUIContainer(() => { TextureTransformerEditor.DrawerWarning(nameof(PreviewGroup)); PreviewButtonDrawUtil.Draw(target as PreviewGroup); });

                rootVE.hierarchy.Add(previewButton);
                rootVE.styleSheets.Add(s_style);

                var previewGroup = target as PreviewGroup;
                var atPhase = AvatarBuildUtils.FindAtPhase(previewGroup.gameObject);

                var label = new Label(TexTransPhase.MaterialModification.ToString());
                label.style.fontSize = 16f;
                rootVE.hierarchy.Add(label);
                CreateGroupElements(rootVE, atPhase[TexTransPhase.MaterialModification]);

                 label = new Label(TexTransPhase.BeforeUVModification.ToString());
                label.style.fontSize = 16f;
                rootVE.hierarchy.Add(label);
                CreateGroupElements(rootVE, atPhase[TexTransPhase.BeforeUVModification]);

                label = new Label(TexTransPhase.UVModification.ToString());
                label.style.fontSize = 16f;
                rootVE.hierarchy.Add(label);
                CreateGroupElements(rootVE, atPhase[TexTransPhase.UVModification]);


                label = new Label(TexTransPhase.AfterUVModification.ToString());
                label.style.fontSize = 16f;
                rootVE.hierarchy.Add(label);
                CreateGroupElements(rootVE, atPhase[TexTransPhase.AfterUVModification]);

                label = new Label(TexTransPhase.PostProcessing.ToString());
                label.style.fontSize = 16f;
                rootVE.hierarchy.Add(label);
                CreateGroupElements(rootVE, atPhase[TexTransPhase.PostProcessing]);


                label = new Label(TexTransPhase.UnDefined.ToString());
                label.style.fontSize = 16f;
                rootVE.hierarchy.Add(label);
                CreateGroupElements(rootVE, atPhase[TexTransPhase.UnDefined]);

                label = new Label(TexTransPhase.Optimizing.ToString());
                label.style.fontSize = 16f;
                rootVE.hierarchy.Add(label);
                CreateGroupElements(rootVE, atPhase[TexTransPhase.Optimizing]);
            }
        }

    }
}
