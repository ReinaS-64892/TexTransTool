#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal;
using Rs64.TexTransTool.Editor;

namespace Rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(SimpleDecal), true)]
    public class SimpleDecalEditor : UnityEditor.Editor
    {
        bool FordiantAdvansd;
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            var ThisObject = target as SimpleDecal;

            var S_Advansd = This_S_Object.FindProperty("AdvansdMode");
            var AdvansdMode = S_Advansd.boolValue;


            EditorGUI.BeginDisabledGroup(ThisObject.IsAppry);


            var S_TargetRenderer = This_S_Object.FindProperty("TargetRenderers");
            if (!AdvansdMode)
            {
                S_TargetRenderer.arraySize = 1;
                var S_TRArryElemt = S_TargetRenderer.GetArrayElementAtIndex(0);
                var TRArryElemetValue = S_TRArryElemt.objectReferenceValue;
                var TRArryElemetEditValue = EditorGUILayout.ObjectField("TargetRenderer", TRArryElemetValue, typeof(Renderer), true) as Renderer;
                if (TRArryElemetValue != TRArryElemetEditValue)
                {
                    Renderer FiltalingdRendarer = RendererFiltaling(TRArryElemetEditValue);
                    S_TRArryElemt.objectReferenceValue = FiltalingdRendarer;
                }
            }
            else
            {
                EditorGUILayout.LabelField("TargetRenderer");
                foreach (var Index in Enumerable.Range(0, S_TargetRenderer.arraySize))
                {
                    var S_TargetRendererValue = S_TargetRenderer.GetArrayElementAtIndex(Index);
                    var TargetRendererValue = S_TargetRendererValue.objectReferenceValue;
                    var TargetRendererEditValue = EditorGUILayout.ObjectField("Target " + (Index + 1), TargetRendererValue, typeof(Renderer), true) as Renderer;
                    if (TargetRendererValue != TargetRendererEditValue)
                    {
                        Renderer FiltalingdRendarer = RendererFiltaling(TargetRendererEditValue);
                        S_TargetRendererValue.objectReferenceValue = FiltalingdRendarer;
                    }
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+")) S_TargetRenderer.arraySize += 1;
                EditorGUI.BeginDisabledGroup(S_TargetRenderer.arraySize <= 1);
                if (GUILayout.Button("-")) S_TargetRenderer.arraySize -= 1;
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }



            var S_DecalTexture = This_S_Object.FindProperty("DecalTexture");
            var DecalTexValue = S_DecalTexture.objectReferenceValue;
            var DecalTexEditValue = EditorGUILayout.ObjectField("DecalTexture", DecalTexValue, typeof(Texture2D), false) as Texture2D;
            if (DecalTexValue != DecalTexEditValue)
            {
                S_DecalTexture.objectReferenceValue = DecalTexEditValue;
                This_S_Object.ApplyModifiedProperties();

                Undo.RecordObject(ThisObject, "AppryScaile - TextureAspect");
                ThisObject.ScaleAppry();
                ThisObject.GizmInstans();
            }



            var S_FixedAspect = This_S_Object.FindProperty("FixedAspect");
            var S_Scale = This_S_Object.FindProperty("Scale");
            if (!AdvansdMode || S_FixedAspect.boolValue)
            {
                var ScaleValue = S_Scale.vector2Value;
                var ScaleEditValue = EditorGUILayout.FloatField("Scale", ScaleValue.x);
                if (ScaleValue.x != ScaleEditValue)
                {
                    ScaleValue.x = ScaleEditValue;
                    S_Scale.vector2Value = ScaleValue;
                    This_S_Object.ApplyModifiedProperties();
                    Undo.RecordObject(ThisObject, "ScaleAppry - ScaleEdit");
                    ThisObject.ScaleAppry();
                }
            }
            else
            {
                var ScaleValue = S_Scale.vector2Value;
                var ScaleEditValue = EditorGUILayout.Vector2Field("Scale", ScaleValue);
                if (ScaleValue != ScaleEditValue)
                {
                    S_Scale.vector2Value = ScaleEditValue;
                    This_S_Object.ApplyModifiedProperties();
                    Undo.RecordObject(ThisObject, "ScaleAppry - ScaleEdit");
                    ThisObject.ScaleAppry();
                }
            }



            var S_MaxDistans = This_S_Object.FindProperty("MaxDistans");
            var MaxDistansValue = S_MaxDistans.floatValue;
            var MaxDistansEditValue = EditorGUILayout.FloatField("MaxDistans", MaxDistansValue);
            if (MaxDistansValue != MaxDistansEditValue)
            {
                Undo.RecordObject(ThisObject, "AppryScaile - MaxDistans");
                ThisObject.MaxDistans = MaxDistansEditValue;
                ThisObject.ScaleAppry();
            }



            if (AdvansdMode)
            {

                EditorGUILayout.PropertyField(S_FixedAspect);

                var S_BlendType = This_S_Object.FindProperty("BlendType");
                EditorGUILayout.PropertyField(S_BlendType);

                var S_TargetPropatyNames = This_S_Object.FindProperty("TargetPropatyName");
                EditorGUILayout.PropertyField(S_TargetPropatyNames);

                var S_PolygonCaling = This_S_Object.FindProperty("PolygonCaling");
                EditorGUILayout.PropertyField(S_PolygonCaling);

                var S_SideChek = This_S_Object.FindProperty("SideChek");
                EditorGUILayout.PropertyField(S_SideChek);

            }
            var EditAdvansdMode = EditorGUILayout.Toggle("AdvansdMode", AdvansdMode);
            if (AdvansdMode != EditAdvansdMode )
            {
                if (!EditAdvansdMode)
                {
                    Undo.RecordObject(ThisObject, "EditAdvansdMode - False");
                    ThisObject.AdvansdModeReset();
                }
                else
                {
                    Undo.RecordObject(ThisObject, "EditAdvansdMode - True");
                    ThisObject.AdvansdMode = EditAdvansdMode;
                }
            }

            EditorGUI.EndDisabledGroup();

            TextureTransformerEditor.TextureTransformerEditorDrow(ThisObject);

            This_S_Object.ApplyModifiedProperties();
        }

        private static Renderer RendererFiltaling(Renderer TargetRendererEditValue)
        {
            Renderer FiltalingdRendarer;
            if (TargetRendererEditValue is SkinnedMeshRenderer || TargetRendererEditValue is MeshRenderer)
            {
                FiltalingdRendarer = TargetRendererEditValue;
            }
            else
            {
                FiltalingdRendarer = null;
            }

            return FiltalingdRendarer;
        }
    }


}
#endif