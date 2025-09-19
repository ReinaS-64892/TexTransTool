#nullable enable

using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(LayerFolder), true)]
    [CanEditMultipleObjects]
    internal class LayerFolderEditor : AbstractLayerEditor
    {
        protected override void DrawInnerProperties()
        {
            var passThrough = serializedObject.FindProperty(nameof(LayerFolder.PassThrough));
            EditorGUILayout.PropertyField(passThrough, "LayerFolder:prop:PassThrough".Glc());
        }
    }
}