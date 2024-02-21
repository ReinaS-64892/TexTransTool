using UnityEditor;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransTool;
using UnityEngine.UIElements;

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
            PreviewContext.instance.DrawApplyAndRevert(target as TexTransGroup);
            var sTexTransPhase = serializedObject.FindProperty("TexTransPhase");
            EditorGUILayout.PropertyField(sTexTransPhase, sTexTransPhase.name.Glc());
            serializedObject.ApplyModifiedProperties();
        });

        rootVE.hierarchy.Add(previewButton);
        rootVE.styleSheets.Add(s_style);

        CreateGroupElements(rootVE, target as PhaseDefinition, true);

        return rootVE;
    }

}
