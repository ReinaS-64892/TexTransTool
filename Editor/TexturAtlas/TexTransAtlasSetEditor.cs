#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Rs64.TexTransTool.Editor;
using System.Collections.Generic;

namespace Rs64.TexTransTool.TexturAtlas.Editor
{
    [CustomEditor(typeof(AtlasSet), true)]
    public class TexTransAtlasSetEditor : UnityEditor.Editor
    {
        bool PostPrcessFoldout = true;
        public override void OnInspectorGUI()
        {
            var TargetRoot = serializedObject.FindProperty("TargetRoot");
            var SelectMats = serializedObject.FindProperty("SelectMats");
            var TargetRenderer = serializedObject.FindProperty("TargetRenderer");
            var TargetMaterial = serializedObject.FindProperty("TargetMaterial");
            var ForsedMaterialMarge = serializedObject.FindProperty("ForsedMaterialMarge");
            var UseRefarensMaterial = serializedObject.FindProperty("UseRefarensMaterial");
            var ForseSetTexture = serializedObject.FindProperty("ForseSetTexture");
            var RefarensMaterial = serializedObject.FindProperty("RefarensMaterial");
            var TextureSize = serializedObject.FindProperty("AtlasTextureSize");
            var Pading = serializedObject.FindProperty("Pading");
            var PadingType = serializedObject.FindProperty("PadingType");
            var SortingType = serializedObject.FindProperty("SortingType");
            var Contenar = serializedObject.FindProperty("Contenar");

            var ThisTarget = target as AtlasSet;
            var IsApply = ThisTarget.IsApply;

            EditorGUI.BeginDisabledGroup(IsApply);
            TextureTransformerEditor.objectReferencePorpty<GameObject>(TargetRoot, i => SetTargetRoot(i, TargetRenderer, TargetMaterial));
            if (TargetRoot.objectReferenceValue != null)
            {
                if (GUILayout.Button("ResearchRenderas"))
                {
                    SetTargetRoot(TargetRoot.objectReferenceValue as GameObject, TargetRenderer, TargetMaterial);
                }
            }
            MaterialSelectEditor(TargetMaterial);

            EditorGUILayout.PropertyField(ForsedMaterialMarge);
            if (ForsedMaterialMarge.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(UseRefarensMaterial);
                if (UseRefarensMaterial.boolValue)
                {
                    RefarensMaterial.objectReferenceValue = EditorGUI.ObjectField(EditorGUILayout.GetControlRect(), RefarensMaterial.objectReferenceValue, typeof(Material), true);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(ForseSetTexture);
            }

            EditorGUILayout.PropertyField(TextureSize);
            EditorGUILayout.PropertyField(Pading);
            EditorGUILayout.PropertyField(PadingType);
            EditorGUILayout.PropertyField(SortingType);
            EditorGUILayout.PropertyField(Contenar);

            PostPrcessFoldout = EditorGUILayout.Foldout(PostPrcessFoldout, "PostProcess");
            if (PostPrcessFoldout)
            {
                var PostPorsesars = serializedObject.FindProperty("PostProcess");

                EditorGUI.indentLevel += 1;
                foreach (var Index in Enumerable.Range(0, PostPorsesars.arraySize))
                {
                    EditorGUILayout.LabelField("processor " + (Index + 1));

                    EditorGUI.indentLevel += 1;

                    var PostPorses = PostPorsesars.GetArrayElementAtIndex(Index);
                    var RelaPropetyProces = PostPorses.FindPropertyRelative("Process");
                    var RelaPropetySelect = PostPorses.FindPropertyRelative("Select");
                    var RelaPropetyTargetPropatyNames = PostPorses.FindPropertyRelative("TargetPropatyNames");
                    var RelaPropetyProsesValue = PostPorses.FindPropertyRelative("ProsesValue");
                    EditorGUILayout.PropertyField(RelaPropetyProces, new GUIContent("ProceserType"));
                    switch (RelaPropetyProces.enumValueIndex)
                    {
                        case 0:
                            {
                                EditorGUILayout.PropertyField(RelaPropetySelect);
                                var intFildLabel = "MaxSize";
                                if (int.TryParse(RelaPropetyProsesValue.stringValue, out var Intval))
                                {
                                    RelaPropetyProsesValue.stringValue = EditorGUILayout.IntField(intFildLabel, Intval).ToString();
                                }
                                else
                                {
                                    RelaPropetyProsesValue.stringValue = EditorGUILayout.IntField(intFildLabel, 512).ToString();
                                }
                                if (RelaPropetyTargetPropatyNames.arraySize < 1) { RelaPropetyTargetPropatyNames.arraySize = 1; }
                                foreach (var TPNIndex in Enumerable.Range(0, RelaPropetyTargetPropatyNames.arraySize))
                                {
                                    EditorGUILayout.PropertyField(RelaPropetyTargetPropatyNames.GetArrayElementAtIndex(TPNIndex), new GUIContent("TargetPropatyName " + (TPNIndex + 1)));
                                }
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.Space();
                                EditorGUILayout.Space();
                                if (GUILayout.Button("+")) { RelaPropetyTargetPropatyNames.arraySize += 1; }
                                if (GUILayout.Button("-")) { RelaPropetyTargetPropatyNames.arraySize -= 1; }
                                EditorGUILayout.EndHorizontal();
                                break;
                            }
                        case 1:
                            {
                                RelaPropetyTargetPropatyNames.arraySize = 1;
                                EditorGUILayout.PropertyField(RelaPropetyTargetPropatyNames.GetArrayElementAtIndex(0), new GUIContent("TargetPropatyName"));
                                break;
                            }
                    }

                    EditorGUI.indentLevel -= 1;

                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button("+")) { PostPorsesars.arraySize += 1; }
                if (GUILayout.Button("-")) { PostPorsesars.arraySize -= 1; }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel -= 1;
            }

            EditorGUI.EndDisabledGroup();

            TextureTransformerEditor.TextureTransformerEditorDrow(ThisTarget);




            serializedObject.ApplyModifiedProperties();

        }

        public static void SetTargetRoot(GameObject TargetRoot, SerializedProperty TargetRenderer, SerializedProperty TargetMats)
        {
            if (TargetRoot == null)
            {
                TargetRenderer.arraySize = 0;
                return;
            }
            var renderers = TargetRoot.GetComponentsInChildren<Renderer>();
            var FilterRendres = TextureTransformerEditor.RendererFiltaling(renderers).ToArray();
            int count = -1;
            TargetRenderer.arraySize = FilterRendres.Length;
            foreach (var Rendera in FilterRendres)
            {
                count += 1;
                TargetRenderer.GetArrayElementAtIndex(count).objectReferenceValue = Rendera;
            }
            var mats = Utils.GetMaterials(FilterRendres).Distinct().Where(i => i != null).ToArray();
            count = -1;
            TargetMats.arraySize = mats.Length;
            foreach (var mat in mats)
            {
                count += 1;
                TargetMats.GetArrayElementAtIndex(count).FindPropertyRelative("Mat").objectReferenceValue = mat;
            }

        }

        public static void MaterialSelectEditor(SerializedProperty TargetMaterial)
        {
            EditorGUI.indentLevel += 1;
            GUILayout.Label("IsTarget  (Offset)  Material");
            foreach (var Index in Enumerable.Range(0, TargetMaterial.arraySize))
            {
                var MatSelect = TargetMaterial.GetArrayElementAtIndex(Index);
                var SMat = MatSelect.FindPropertyRelative("Mat");
                var SISelect = MatSelect.FindPropertyRelative("IsSelect");
                var SOffset = MatSelect.FindPropertyRelative("Offset");
                EditorGUILayout.BeginHorizontal();
                SISelect.boolValue = EditorGUILayout.Toggle(SISelect.boolValue,GUILayout.MaxWidth(100) );
                if (SISelect.boolValue)
                {
                    SOffset.floatValue = EditorGUILayout.FloatField(SOffset.floatValue, new GUILayoutOption[] { GUILayout.MaxWidth(100) });
                }
                EditorGUI.ObjectField(EditorGUILayout.GetControlRect(GUILayout.MaxWidth(1000)), SMat.objectReferenceValue, typeof(Material), false);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel -= 1;

        }

    }
}
#endif