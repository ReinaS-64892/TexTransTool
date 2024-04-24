using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using net.rs64.TexTransTool.IslandSelector;
using net.rs64.TexTransTool.Preview.RealTime;

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
                var sIslandSelector = thisSObject.FindProperty("IslandSelector");
                EditorGUILayout.PropertyField(sIslandSelector, "SimpleDecal:prop:ExperimentalFuture:IslandSelector".Glc());

                if (sIslandSelector.objectReferenceValue == null || sIslandSelector.objectReferenceValue is RayCastIslandSelector)
                {
                    var sIslandCulling = thisSObject.FindProperty("IslandCulling");
                    if (sIslandCulling.boolValue && GUILayout.Button("Migrate IslandCulling to  IslandSelector"))
                    {
                        MigrateIslandCullingToIslandSelector(thisObject);
                    }
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
        // private void OnEnable()
        // {
        //     foreach (var decal in targets)
        //     {
        //         RealTimePreviewManager.instance.ForcesDecal.Add(decal as AbstractDecal);
        //     }

        // }

        // private void OnDisable()
        // {
        //     foreach (var decal in targets)
        //     {
        //         RealTimePreviewManager.instance.ForcesDecal.Remove(decal as AbstractDecal);
        //     }
        // }









        public void MigrateIslandCullingToIslandSelector(SimpleDecal simpleDecal)
        {
            if (simpleDecal.IslandSelector != null)
            {
                if (simpleDecal.IslandSelector is not RayCastIslandSelector) { Debug.LogError("IslandSelector にすでに何かが割り当てられているため、マイグレーションを実行できません。"); return; }
                else { if (!EditorUtility.DisplayDialog("Migrate IslandCulling To IslandSelector", "IslandSelector に RayCastIslandSelector が既に割り当てられています。 \n 割り当てられている RayCastIslandSelector を編集する形でマイグレーションしますか？", "実行")) { return; } }
            }
            Undo.RecordObject(simpleDecal, "MigrateIslandCullingToIslandSelector");

            simpleDecal.IslandCulling = false;
            var islandSelector = simpleDecal.IslandSelector as RayCastIslandSelector;

            if (islandSelector == null)
            {
                var go = new GameObject("RayCastIslandSelector");
                go.transform.SetParent(simpleDecal.transform, false);
                simpleDecal.IslandSelector = islandSelector = go.AddComponent<RayCastIslandSelector>();
            }
            Undo.RecordObject(islandSelector, "MigrateIslandCullingToIslandSelector - islandSelectorEdit");


            Vector3 selectorOrigin = new Vector2(simpleDecal.IslandSelectorPos.x - 0.5f, simpleDecal.IslandSelectorPos.y - 0.5f);


            var ltwMatrix = simpleDecal.transform.localToWorldMatrix;
            islandSelector.transform.position = ltwMatrix.MultiplyPoint3x4(selectorOrigin);
            islandSelector.IslandSelectorRange = simpleDecal.IslandSelectorRange;

        }


    }


}
