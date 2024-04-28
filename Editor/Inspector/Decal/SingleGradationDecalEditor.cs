using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using net.rs64.TexTransTool.IslandSelector;
using System;
using net.rs64.TexTransTool.Preview.RealTime;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SingleGradationDecal))]
    internal class SingleGradationDecalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning(target.GetType().Name);
            base.OnInspectorGUI();
            TextureTransformerEditor.DrawerRealTimePreviewEditorButton(target as TexTransRuntimeBehavior);
        }
    }
}
