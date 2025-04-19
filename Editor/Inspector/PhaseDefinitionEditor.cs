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
    [CustomEditor(typeof(PhaseDefinition))]
    internal class PhaseDefinitionEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI() { return CrateGroupElements(target as PhaseDefinition, serializedObject); }
        internal static VisualElement CrateGroupElements(PhaseDefinition pd, SerializedObject serializedObject)
        {
            var rootVE = new VisualElement();
            rootVE.hierarchy.Clear();

            var previewButton = new IMGUIContainer(() =>
            {
                PreviewButtonDrawUtil.Draw(pd);

                serializedObject.Update();
                var sTexTransPhase = serializedObject.FindProperty(nameof(PhaseDefinition.TexTransPhase));
                EditorGUILayout.PropertyField(sTexTransPhase, sTexTransPhase.name.Glc());
                serializedObject.ApplyModifiedProperties();
            });
            rootVE.hierarchy.Add(previewButton);

            var groupBehaviors = new List<TexTransBehavior>();
            TexTransBehaviorSearch.GroupedComponentsCorrect(groupBehaviors, pd.gameObject, new TexTransBehaviorSearch.DefaultGameObjectWakingTool());
            foreach (var ttb in groupBehaviors) rootVE.hierarchy.Add(PreviewGroupEditor.Summary(ttb));

            return rootVE;
        }

    }
}
