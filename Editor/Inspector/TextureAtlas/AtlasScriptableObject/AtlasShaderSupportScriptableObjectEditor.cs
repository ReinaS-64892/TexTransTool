// using UnityEngine;
// using UnityEditor;
// using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;
// using net.rs64.TexTransTool.Editor;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// namespace net.rs64.TexTransTool.TextureAtlas.Editor
// {

//     [CustomEditor(typeof(AtlasShaderSupportScriptableObject))]
//     public class AtlasShaderSupportScriptableObjectEditor : UnityEditor.Editor
//     {
//         HashSet<object> _hash;
//         public override void OnInspectorGUI()
//         {
//             var sTTTSaveDataVersion = serializedObject.FindProperty("TTTSaveDataVersion");
//             if (sTTTSaveDataVersion.intValue != TexTransBehavior.TTTDataVersion)
//             {
// #pragma warning disable CS0612
//                 migration();
// #pragma warning restore CS0612
//             }
//             TextureTransformerEditor.DrawerWarning("AtlasShaderSupportScriptableObject");
//             base.OnInspectorGUI();

//             var atd = serializedObject.FindProperty("AtlasTargetDefines");
//             var arraySize = atd.arraySize;
//             _hash ??= new HashSet<object>(arraySize);
//             for (var i = 0; arraySize > i; i += 1)
//             {
//                 var adc = atd.GetArrayElementAtIndex(i).FindPropertyRelative("AtlasDefineConstraints");
//                 var mrf = adc.managedReferenceValue;
//                 if (_hash.Contains(mrf)) { adc.managedReferenceValue = new FloatPropertyValueGreater(); }
//                 else { _hash.Add(mrf); }
//             }
//             _hash.Clear();

//             serializedObject.ApplyModifiedProperties();
//         }
//         [Obsolete]
//         private void migration()
//         {
//             if (GUILayout.Button("MigrateToV" + TexTransBehavior.TTTDataVersion))
//             {
//                 serializedObject.ApplyModifiedProperties();
//                 var thisObject = target as AtlasShaderSupportScriptableObject;

//                 while (thisObject.TTTSaveDataVersion < TexTransBehavior.TTTDataVersion)
//                 {

//                     switch (thisObject.TTTSaveDataVersion)
//                     {
//                         default: { break; }
//                         case 4:
//                             {
//                                 for (var i = 0; thisObject.AtlasTargetDefines.Count > i; i += 1)
//                                 {
//                                     var define = thisObject.AtlasTargetDefines[i];

//                                     define.BakePropertyDescriptions = define.BakePropertyNames.Select(i => new BakePropertyDescription() { PropertyName = i }).ToList();
//                                     thisObject.AtlasTargetDefines[i] = define;
//                                 }
//                                 break;
//                             }
//                     }

//                     thisObject.TTTSaveDataVersion += 1;
//                 }

//                 EditorUtility.SetDirty(thisObject);
//                 serializedObject.Update();
//             }
//         }
//     }
// }
