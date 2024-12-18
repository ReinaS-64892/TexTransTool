using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Preview;
namespace net.rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(NegotiateAAOConfig))]
    internal class NegotiateAAOConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("NegotiateAAOConfig");
            base.OnInspectorGUI();
        }
    }
}
