using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.ReferenceResolver;

namespace net.rs64.TexTransTool.Editor.ReferenceResolver
{
    [CustomEditor(typeof(AbstractResolver), true)]
    internal class AbstractResolverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("ReferenceResolver");

            base.OnInspectorGUI();

        }
    }
}