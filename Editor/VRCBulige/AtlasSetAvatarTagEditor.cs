#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using Rs.TexturAtlasCompiler.VRCBulige;
namespace Rs.TexturAtlasCompiler.VRCBulige.Editor
{
    [CustomEditor(typeof(AtlasSetAvatarTag))]
    public class AtlasSetAvatarTagEditor : UnityEditor.Editor
    {
        bool PostPrcessFoldout = true;
        public override void OnInspectorGUI()
        {
            var serialaizeatlaset = serializedObject.FindProperty("AtlasSet");
            var ClientSelect = serializedObject.FindProperty("ClientSelect");
            var SkindMesh = serialaizeatlaset.FindPropertyRelative("AtlasTargetMeshs");
            var Staticmesh = serialaizeatlaset.FindPropertyRelative("AtlasTargetStaticMeshs");
            var TextureSize = serialaizeatlaset.FindPropertyRelative("AtlasTextureSize");
            var Pading = serialaizeatlaset.FindPropertyRelative("Pading");
            var PadingType = serialaizeatlaset.FindPropertyRelative("PadingType");
            var SortingType = serialaizeatlaset.FindPropertyRelative("SortingType");
            var Contenar = serialaizeatlaset.FindPropertyRelative("Contenar");

            var AtlasSetAvatarTag = target as AtlasSetAvatarTag;
            var IsAppry = AtlasSetAvatarTag.AtlasSet.IsAppry;

            EditorGUI.BeginDisabledGroup(IsAppry);
            EditorGUILayout.PropertyField(SkindMesh);
            EditorGUILayout.PropertyField(Staticmesh);
            EditorGUILayout.PropertyField(TextureSize);
            EditorGUILayout.PropertyField(Pading);
            EditorGUILayout.PropertyField(PadingType);
            EditorGUILayout.PropertyField(SortingType);
            EditorGUILayout.PropertyField(ClientSelect);
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


            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(AtlasSetAvatarTag.AtlasSet.Contenar == null || IsAppry);
            if (GUILayout.Button("Appry"))
            {
                Undo.RecordObject(AtlasSetAvatarTag, "AtlasAppry");
                AtlasSetAvatarTag.AtlasSet.Appry();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!IsAppry);
            if (GUILayout.Button("Revart"))
            {
                Undo.RecordObject(AtlasSetAvatarTag, "AtlasRevart");
                AtlasSetAvatarTag.AtlasSet.Revart();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(IsAppry);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("TexturAtlasCompile!"))
            {
                if (!AtlasSetAvatarTag.AtlasSet.IsAppry)
                {
                    Undo.RecordObject(AtlasSetAvatarTag, "AtlasCompile");
                    if (AtlasSetAvatarTag.PostProcess.Any())
                    {
                        foreach (var PostPrces in AtlasSetAvatarTag.PostProcess)
                        {
                            AtlasSetAvatarTag.AtlasSet.AtlasCompilePostCallBack += (i) => PostPrces.Processing(i);
                        }
                    }
                    else
                    {
                        AtlasSetAvatarTag.AtlasSet.AtlasCompilePostCallBack = (i) => { };
                    }
                    Compiler.AtlasSetCompile(AtlasSetAvatarTag.AtlasSet, AtlasSetAvatarTag.ClientSelect, true);
                }
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif