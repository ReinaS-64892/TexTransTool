using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

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

            AbstractDecalEditor.DrawerDecalEditor(thisSObject);

            if (!isMultiEdit)
            {
                var tf_sObg = new SerializedObject(thisObject.transform);
                var decalTexture = thisObject.DecalTexture;
                DrawerScale(thisSObject, tf_sObg, decalTexture);
                tf_sObg.ApplyModifiedProperties();
            }

            EditorGUILayout.LabelField("SimpleDecal:label:CullingSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sPolygonCulling = thisSObject.FindProperty("PolygonCulling");
            EditorGUILayout.PropertyField(sPolygonCulling, "SimpleDecal:prop:PolygonCulling".Glc());

            var sSideCulling = thisSObject.FindProperty("SideCulling");
            EditorGUILayout.PropertyField(sSideCulling, "SimpleDecal:prop:SideCulling".Glc());


            EditorGUI.indentLevel -= 1;

            AbstractDecalEditor.DrawerAdvancedOption(thisSObject);


            s_ExperimentalFutureOption = EditorGUILayout.Foldout(s_ExperimentalFutureOption, "Common:ExperimentalFuture".Glc());
            if (s_ExperimentalFutureOption)
            {
                var sIslandCulling = thisSObject.FindProperty("IslandCulling");
                EditorGUILayout.PropertyField(sIslandCulling, "SimpleDecal:prop:ExperimentalFuture:IslandCulling".Glc());
                if (sIslandCulling.boolValue)
                {
                    var sIslandSelectorPos = thisSObject.FindProperty("IslandSelectorPos");
                    EditorGUI.indentLevel += 1;
                    EditorGUILayout.LabelField("SimpleDecal:prop:ExperimentalFuture:IslandSelectorPos".Glc());
                    EditorGUI.indentLevel += 1;
                    var sIslandSelectorPosX = sIslandSelectorPos.FindPropertyRelative("x");
                    var sIslandSelectorPosY = sIslandSelectorPos.FindPropertyRelative("y");
                    EditorGUILayout.Slider(sIslandSelectorPosX, 0, 1, new GUIContent("x"));
                    EditorGUILayout.Slider(sIslandSelectorPosY, 0, 1, new GUIContent("y"));
                    EditorGUI.indentLevel -= 1;
                    var sIslandSelectorRange = thisSObject.FindProperty("IslandSelectorRange");
                    EditorGUILayout.Slider(sIslandSelectorRange, 0, 1, "SimpleDecal:prop:ExperimentalFuture:IslandSelectorRange".Glc());
                    EditorGUI.indentLevel -= 1;
                }

                var sUseDepth = thisSObject.FindProperty("UseDepth");
                var sDepthInvert = thisSObject.FindProperty("DepthInvert");
                EditorGUILayout.PropertyField(sUseDepth, "SimpleDecal:prop:ExperimentalFuture:UseDepth".Glc());
                if (sUseDepth.boolValue) { EditorGUILayout.PropertyField(sDepthInvert, "SimpleDecal:prop:ExperimentalFuture:DepthInvert".Glc()); }

            }

            AbstractDecalEditor.DrawerRealTimePreviewEditor(targets);

            // if (!isMultiEdit)
            // {
            //     EditorGUI.BeginDisabledGroup(RealTimePreviewManager.Contains(thisObject));
            //     PreviewContext.instance.DrawApplyAndRevert(thisObject);
            //     EditorGUI.EndDisabledGroup();
            // }

            thisSObject.ApplyModifiedProperties();
        }

        static bool s_ExperimentalFutureOption = false;

        public static void DrawerScale(SerializedObject thisSObject, SerializedObject tf_sObg, Texture2D decalTexture)
        {
            EditorGUILayout.LabelField("SimpleDecal:label:ScaleSettings".Glc(), EditorStyles.boldLabel);
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
                    "SimpleDecal:prop:Scale".Glc(),
                    editCollBack
                );
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SimpleDecal:prop:Scale".Glc(), GUILayout.Width(60));
                EditorGUILayout.LabelField("x", GUILayout.Width(30));
                EditorGUILayout.PropertyField(sLocalScale.FindPropertyRelative("x"), GUIContent.none);
                EditorGUILayout.LabelField("y", GUILayout.Width(30));
                EditorGUILayout.PropertyField(sLocalScale.FindPropertyRelative("y"), GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }

            TextureTransformerEditor.DrawerPropertyBool(sFixedAspect, "SimpleDecal:prop:FixedAspect".Glc(), (Value) => { if (Value) { editCollBack.Invoke(sLocalScale.FindPropertyRelative("x").floatValue); } return Value; });

            EditorGUILayout.PropertyField(sLocalScale.FindPropertyRelative("z"), "SimpleDecal:prop:MaxDistance".Glc());

            EditorGUI.indentLevel -= 1;
        }


        [InitializeOnLoadMethod]
        internal static void RegisterSummary()
        {
            TexTransGroupEditor.s_summary[typeof(SimpleDecal)] = at =>
            {
                var ve = new VisualElement();
                var serializedObject = new SerializedObject(at);
                var sTargetRenderers = serializedObject.FindProperty("TargetRenderers");
                var sAtlasTextureSize = serializedObject.FindProperty("DecalTexture");

                var targetRoot = new PropertyField();
                targetRoot.label = "CommonDecal:prop:TargetRenderer".GetLocalize();
                targetRoot.BindProperty(sTargetRenderers.GetArrayElementAtIndex(0));
                ve.hierarchy.Add(targetRoot);

                var atlasTextureSize = new ObjectField();
                atlasTextureSize.label = "CommonDecal:prop:DecalTexture".GetLocalize();
                atlasTextureSize.BindProperty(sAtlasTextureSize);
                ve.hierarchy.Add(atlasTextureSize);

                return ve;
            };
        }
        private void OnEnable()
        {
            foreach (var decal in targets)
            {
                RealTimePreviewManager.instance.ForcesDecal.Add(decal as AbstractDecal);
            }

        }

        private void OnDisable()
        {
            foreach (var decal in targets)
            {
                RealTimePreviewManager.instance.ForcesDecal.Remove(decal as AbstractDecal);
            }
        }


    }


}
