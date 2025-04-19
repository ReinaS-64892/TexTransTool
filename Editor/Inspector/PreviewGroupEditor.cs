using UnityEditor;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransTool;
using UnityEngine.UIElements;
using net.rs64.TexTransTool.Build;
using System.Collections.Generic;
using net.rs64.TexTransTool.Preview;
using System;
using UnityEditor.UIElements;
using System.ComponentModel;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(PreviewGroup))]
    internal class PreviewGroupEditor : PhaseDefinitionEditor
    {
        public override VisualElement CreateInspectorGUI() { return CrateGroupElements(target as PreviewGroup); }

        internal static VisualElement CrateGroupElements(PreviewGroup previewGroup)
        {
            var rootVE = new VisualElement();
            rootVE.hierarchy.Clear();

            var previewButton = new IMGUIContainer(() =>
            {
                TextureTransformerEditor.DrawerWarning(nameof(PreviewGroup));
                PreviewButtonDrawUtil.Draw(previewGroup);
            });

            rootVE.hierarchy.Add(previewButton);

            var atPhase = TexTransBehaviorSearch.FindAtPhase(previewGroup.gameObject);

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

            return rootVE;
        }

        internal static void CreateGroupElements(VisualElement rootVE, List<TexTransBehavior> texTransBehaviors)
        {
            foreach (var ttb in texTransBehaviors)
            {
                rootVE.hierarchy.Add(Summary(ttb));
            }
        }
        internal static VisualElement Summary(TexTransBehavior ttb)
        {
            var sObj = new SerializedObject(ttb.gameObject);
            var sActive = sObj.FindProperty("m_IsActive");

            var ttbSummaryBase = new VisualElement();
            ttbSummaryBase.style.flexDirection = FlexDirection.Row;

            var goButton = new Toggle();
            goButton.RegisterValueChangedCallback(v => updateActive(ttbSummaryBase, ttb.ThisEnable && v.newValue));
            goButton.BindProperty(sActive);
            goButton.AddToClassList("ActiveToggle");
            goButton.label = "";
            goButton.text = "TexTransGroup:prop:GOEnable".GetLocalize();
            ttbSummaryBase.hierarchy.Add(goButton);

            var componentField = new ObjectField();
            componentField.value = ttb;
            componentField.label = "";
            componentField.SetEnabled(false);
            componentField.style.opacity = 0.9f;
            componentField.style.flexGrow = 1.0f;
            ttbSummaryBase.hierarchy.Add(componentField);


            return ttbSummaryBase;
            static void updateActive(VisualElement ttbSummaryBase, bool v) { ttbSummaryBase.style.opacity = v ? 1 : 0.5f; }
        }

    }
}
