#if UNITY_EDITOR
using System.Net.Mime;
using UnityEngine;
using UnityEditor;

namespace Rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(AbstractTexTransGroup), true)]
    public class AbstractTexTransGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var thsitarget = target as AbstractTexTransGroup;
            TextureTransformerEditor.DrowApplyAndRevart(thsitarget);
            // TextureTransformerEditor.DrowCompile(thsitarget);
        }
    }
}
#endif