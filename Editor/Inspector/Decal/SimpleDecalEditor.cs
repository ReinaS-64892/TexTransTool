#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransCore.Decal;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SimpleDecal))]
    internal class SimpleDecalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var thisSObject = serializedObject;
            var thisObject = target as SimpleDecal;
            var isMultiEdit = targets.Length != 1;

            if (isMultiEdit && PreviewContext.IsPreviewContains) { EditorGUILayout.LabelField("Multiple edits during preview are not supported.".GetLocalize()); return; }

            EditorGUI.BeginDisabledGroup(PreviewContext.IsPreviewing(thisObject));

            AbstractDecalEditor.DrawerDecalEditor(thisSObject);

            if (!isMultiEdit)
            {
                var tf_sObg = new SerializedObject(thisObject.transform);
                var decalTexture = thisObject.DecalTexture;
                DrawerScale(thisSObject, tf_sObg, decalTexture);
                tf_sObg.ApplyModifiedProperties();
            }

            EditorGUILayout.LabelField("CullingSettings".GetLocalize(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sPolygonCulling = thisSObject.FindProperty("PolygonCulling");
            EditorGUILayout.PropertyField(sPolygonCulling, sPolygonCulling.name.GetLC());

            var sSideCulling = thisSObject.FindProperty("SideCulling");
            EditorGUILayout.PropertyField(sSideCulling, sSideCulling.name.GetLC());


            EditorGUI.indentLevel -= 1;

            AbstractDecalEditor.DrawerAdvancedOption(thisSObject);


            s_ExperimentalFutureOption = EditorGUILayout.Foldout(s_ExperimentalFutureOption, "Experimental Future".GetLocalize());
            if (s_ExperimentalFutureOption)
            {
                var sIslandCulling = thisSObject.FindProperty("IslandCulling");
                EditorGUILayout.PropertyField(sIslandCulling, sIslandCulling.name.GetLC());
                if (sIslandCulling.boolValue)
                {
                    var sIslandSelectorPos = thisSObject.FindProperty("IslandSelectorPos");
                    EditorGUI.indentLevel += 1;
                    EditorGUILayout.LabelField(sIslandSelectorPos.name.GetLocalize());
                    EditorGUI.indentLevel += 1;
                    var sIslandSelectorPosX = sIslandSelectorPos.FindPropertyRelative("x");
                    var sIslandSelectorPosY = sIslandSelectorPos.FindPropertyRelative("y");
                    EditorGUILayout.Slider(sIslandSelectorPosX, 0, 1, new GUIContent("x"));
                    EditorGUILayout.Slider(sIslandSelectorPosY, 0, 1, new GUIContent("y"));
                    EditorGUI.indentLevel -= 1;
                    var sIslandSelectorRange = thisSObject.FindProperty("IslandSelectorRange");
                    EditorGUILayout.Slider(sIslandSelectorRange, 0, 1, sIslandSelectorRange.name.GetLC());
                    EditorGUI.indentLevel -= 1;
                }

                var sUseDepth = thisSObject.FindProperty("UseDepth");
                var sDepthInvert = thisSObject.FindProperty("DepthInvert");
                EditorGUILayout.PropertyField(sUseDepth, sUseDepth.name.GetLC());
                if (sUseDepth.boolValue) { EditorGUILayout.PropertyField(sDepthInvert, sDepthInvert.name.GetLC()); }


                EditorGUI.indentLevel += 1;
                if (!isMultiEdit)
                {
                    AbstractDecalEditor.DrawerDecalGrabEditor(thisObject);
                }
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.LabelField("---");
            }

            EditorGUI.EndDisabledGroup();
            if (!isMultiEdit)
            {
                AbstractDecalEditor.DrawerRealTimePreviewEditor(thisObject);
                EditorGUI.BeginDisabledGroup(RealTimePreviewManager.instance.RealTimePreviews.ContainsKey(thisObject));
                PreviewContext.instance.DrawApplyAndRevert(thisObject);
                EditorGUI.EndDisabledGroup();
            }

            thisSObject.ApplyModifiedProperties();
        }

        static bool s_ExperimentalFutureOption = false;

        public static void DrawerScale(SerializedObject thisSObject, SerializedObject tf_sObg, Texture2D decalTexture)
        {
            EditorGUILayout.LabelField("ScaleSettings".GetLocalize(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sLocalScale = tf_sObg.FindProperty("m_LocalScale");
            var sFixedAspect = thisSObject.FindProperty("FixedAspect");

            TextureTransformerEditor.Filter<float> editCollBack = (value) =>
            {
                var aspectValue = 1f;
                if (decalTexture != null) { aspectValue = ((float)decalTexture.height / (float)decalTexture.width); }
                sLocalScale.FindPropertyRelative("y").floatValue = value * aspectValue;
                return value;
            };

            if (sFixedAspect.boolValue)
            {
                TextureTransformerEditor.DrawerPropertyFloat(
                    sLocalScale.FindPropertyRelative("x"),
                    "Scale".GetLC(),
                    editCollBack
                );
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Scale".GetLocalize(), GUILayout.Width(60));
                EditorGUILayout.LabelField("x", GUILayout.Width(30));
                EditorGUILayout.PropertyField(sLocalScale.FindPropertyRelative("x"), GUIContent.none);
                EditorGUILayout.LabelField("y", GUILayout.Width(30));
                EditorGUILayout.PropertyField(sLocalScale.FindPropertyRelative("y"), GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }

            TextureTransformerEditor.DrawerPropertyBool(sFixedAspect, sFixedAspect.displayName.GetLC(), (Value) => { if (Value) { editCollBack.Invoke(sLocalScale.FindPropertyRelative("x").floatValue); } return Value; });

            EditorGUILayout.PropertyField(sLocalScale.FindPropertyRelative("z"), "MaxDistance".GetLC());

            EditorGUI.indentLevel -= 1;
        }

        public static void DrawerSummary(SimpleDecal target)
        {
            var sObj = new SerializedObject(target);

            var sTargetRenderers = sObj.FindProperty("TargetRenderers");
            TextureTransformerEditor.DrawerTargetRenderersSummary(sTargetRenderers);

            var sDecalTexture = sObj.FindProperty("DecalTexture");
            TextureTransformerEditor.DrawerObjectReference<Texture2D>(sDecalTexture, sDecalTexture.name.GetLC());

            sObj.ApplyModifiedProperties();
        }


    }


}
#endif
