using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;
using net.rs64.TexTransTool.Editor;
using System;
namespace net.rs64.TexTransTool.TextureAtlas.Editor
{

    [CustomPropertyDrawer(typeof(IAtlasMaterialPostProses))]
    public class IAtlasMaterialPostProsesDrawer : PropertyDrawer
    {

        string[] protProses = new[] { "TextureReferenceCopy" };
        Func<IAtlasMaterialPostProses>[] protProsesGet = new Func<IAtlasMaterialPostProses>[] { () => new TextureReferenceCopy() };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sv = EditorGUI.Popup(position,"Select AtlasMaterialPostProses", -1, protProses);
            if (sv != -1) { property.managedReferenceValue = protProsesGet[sv].Invoke(); }
        }
    }
}
