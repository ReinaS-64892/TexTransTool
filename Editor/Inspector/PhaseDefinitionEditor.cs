using UnityEditor;
using UnityEngine.UIElements;
using net.rs64.TexTransTool.Preview;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(PhaseDefinition))]
    internal class PhaseDefinitionEditor : TexTransGroupEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            LoadStyle();

            var rootVE = new VisualElement();
            var previewButton = new IMGUIContainer(() =>
            {
                serializedObject.Update();
                PreviewButtonDrawUtil.Draw(target as PreviewGroup);
                var sTexTransPhase = serializedObject.FindProperty("TexTransPhase");
                EditorGUILayout.PropertyField(sTexTransPhase, sTexTransPhase.name.Glc());
                serializedObject.ApplyModifiedProperties();
            });

            rootVE.hierarchy.Add(previewButton);
            rootVE.styleSheets.Add(s_style);

            CreateGroupElements(rootVE, (target as PhaseDefinition).Targets, true);

            return rootVE;
        }

    }
}
