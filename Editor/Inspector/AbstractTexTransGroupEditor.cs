#if UNITY_EDITOR
using System.Net.Mime;
using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(AbstractTexTransGroup), true)]
    public class AbstractTexTransGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var thisTarget = target as AbstractTexTransGroup;
            PreviewContext.instance.DrawApplyAndRevert(thisTarget);
        }
    }
}
#endif
