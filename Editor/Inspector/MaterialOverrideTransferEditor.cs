using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(MaterialOverrideTransfer))]
    public class MaterialOverrideTransferEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning(nameof(MaterialOverrideTransfer));
            serializedObject.Update();
            var sTargetMaterial = serializedObject.FindProperty(nameof(MaterialOverrideTransfer.TargetMaterial));
            var sMaterialsVariantSource = serializedObject.FindProperty(nameof(MaterialOverrideTransfer.MaterialVariantSource));

            EditorGUILayout.PropertyField(sTargetMaterial);
            EditorGUILayout.PropertyField(sMaterialsVariantSource, nameof(MaterialOverrideTransfer.MaterialVariantSource).GlcV());

            serializedObject.ApplyModifiedProperties();
            PreviewButtonDrawUtil.Draw(target as TexTransBehavior);
        }

    }
}
