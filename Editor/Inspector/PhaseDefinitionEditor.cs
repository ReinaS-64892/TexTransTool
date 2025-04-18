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
        public override VisualElement CreateInspectorGUI() { return CrateGroupElements(target as PhaseDefinition); }
        internal static VisualElement CrateGroupElements(PhaseDefinition pd)
        {
            var rootVE = new VisualElement();
            rootVE.hierarchy.Clear();

            var previewButton = new IMGUIContainer(() => { PreviewButtonDrawUtil.Draw(pd); });
            rootVE.hierarchy.Add(previewButton);

            var groupBehaviors = new List<TexTransBehavior>();
            AvatarBuildUtils.GroupedComponentsCorrect(groupBehaviors, pd.gameObject, new AvatarBuildUtils.DefaultGameObjectWakingTool());
            foreach (var ttb in groupBehaviors) rootVE.hierarchy.Add(PreviewGroupEditor.Summary(ttb));

            return rootVE;
        }

    }
}
