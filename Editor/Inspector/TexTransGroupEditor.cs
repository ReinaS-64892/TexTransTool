using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using UnityEditor.UIElements;
using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransTool.Build;
using System.Linq;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(TexTransGroup))]
    internal class TexTransGroupEditor : UnityEditor.Editor
    {
        internal static Dictionary<Type, Func<TexTransBehavior, VisualElement>> s_summary = new();
        internal static StyleSheet s_style;

        protected HashSet<Action> _disableActions = new();

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

                var previewButton = new IMGUIContainer(() => OneTimePreviewContext.instance.DrawApplyAndRevert(target as TexTransGroup));

                rootVE.hierarchy.Add(previewButton);
                rootVE.styleSheets.Add(s_style);

                var groupBehaviors = new List<TexTransBehavior>();
                AvatarBuildUtils.GroupedComponentsCorrect(groupBehaviors, (target as TexTransGroup).gameObject, new AvatarBuildUtils.DefaultGameObjectWakingTool());
                CreateGroupElements(rootVE, groupBehaviors);
            }
        }

        internal static void LoadStyle() { s_style ??= AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("9d80dcf21bff21f4cb110fff304f5622")); }

        internal static void CreateGroupElements(VisualElement rootVE, List<TexTransBehavior> group)
        {
            if (group.Any() is false) { return; }

            foreach (var ttb in group)
            {
                var ttbSummaryElement = CreateSummaryBase(ttb);
                CreateSummary(ttbSummaryElement, ttb);
                rootVE.hierarchy.Add(ttbSummaryElement);
            }
        }

        internal static VisualElement CreateSummaryBase(TexTransBehavior ttb)
        {
            var ttbSummaryElement = new VisualElement();
            ttbSummaryElement.AddToClassList("SummaryElementRoot");
            ttbSummaryElement.hierarchy.Add(SummaryBase(ttb, v => ttbSummaryElement.style.opacity = v ? 1 : 0.5f));
            return ttbSummaryElement;
        }

        internal static void CreateSummary(VisualElement ttbSummaryElement, TexTransBehavior ttb)
        {
            if (s_summary.TryGetValue(ttb.GetType(), out var generator))
            {
                var summaryContainer = new VisualElement();
                summaryContainer.AddToClassList("SummaryContainer");
                summaryContainer.hierarchy.Add(generator.Invoke(ttb));
                ttbSummaryElement.hierarchy.Add(summaryContainer);
            }
            else
            {
                var noneLabel = new Label("TexTransGroup:label:SummaryNone".GetLocalize());
                noneLabel.AddToClassList("SummaryContainer");
                ttbSummaryElement.hierarchy.Add(noneLabel);
            }
        }

        internal static VisualElement SummaryBase(TexTransBehavior ttb, Action<bool> isActive = null)
        {
            var sObj = new SerializedObject(ttb.gameObject);
            var sActive = sObj.FindProperty("m_IsActive");

            var ttbSummaryBase = new VisualElement();
            ttbSummaryBase.style.flexDirection = FlexDirection.Row;

            var goButton = new Toggle();
            goButton.RegisterValueChangedCallback(v => isActive?.Invoke(ttb.ThisEnable && v.newValue));
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
            componentField.AddToClassList("TexTransBehaviorRef");
            ttbSummaryBase.hierarchy.Add(componentField);


            return ttbSummaryBase;
        }

        private void OnDisable() { foreach (var i in _disableActions) { i.Invoke(); } _disableActions.Clear(); }

    }
}
