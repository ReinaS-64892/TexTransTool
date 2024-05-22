using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;
using net.rs64.TexTransTool.Editor;
using System;
using System.Collections.Generic;
namespace net.rs64.TexTransTool.TextureAtlas.Editor
{

    [CustomEditor(typeof(AtlasShaderSupportScriptableObject))]
    public class AtlasShaderSupportScriptableObjectEditor : UnityEditor.Editor
    {
        HashSet<object> _hash;
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("AtlasShaderSupportScriptableObject");
            base.OnInspectorGUI();

            var atd = serializedObject.FindProperty("AtlasTargetDefines");
            var arraySize = atd.arraySize;
            _hash ??= new HashSet<object>(arraySize);
            for (var i = 0; arraySize > i; i += 1)
            {
                var adc = atd.GetArrayElementAtIndex(i).FindPropertyRelative("AtlasDefineConstraints");
                var mrf = adc.managedReferenceValue;
                if (_hash.Contains(mrf)) { adc.managedReferenceValue = new FloatPropertyValueGreater(); }
                else { _hash.Add(mrf); }
            }
            _hash.Clear();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
