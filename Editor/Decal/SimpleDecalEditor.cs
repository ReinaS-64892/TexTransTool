#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal;
using Rs64.TexTransTool.Editor;

namespace Rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(SimpleDecal))]
    public class SimpleDecalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            var ThisObject = target as SimpleDecal;

            {
                var S_TargetRenderer = This_S_Object.FindProperty("TargetRenderer");
                var TargetRendererValue = S_TargetRenderer.objectReferenceValue;
                var TargetRendererEditValue = EditorGUILayout.ObjectField("TargetRenderer", TargetRendererValue, typeof(Renderer), true) as Renderer;
                if (TargetRendererValue != TargetRendererEditValue)
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
                    Undo.RecordObject(ThisObject, "Edit - TargetRenderer");
                    ThisObject.TargetRenderer = FiltalingdRendarer;
                }
            }
            {
                var S_DecalTexture = This_S_Object.FindProperty("DecalTexture");
                var DecalTexValue = S_DecalTexture.objectReferenceValue;
                var DecalTexEditValue = EditorGUILayout.ObjectField("DecalTexture", DecalTexValue, typeof(Texture2D), false) as Texture2D;
                if (DecalTexValue != DecalTexEditValue)
                {
                    Undo.RecordObject(ThisObject, "AppryScaile - TextureAspect");
                    ThisObject.DecalTexture = DecalTexEditValue;
                    ThisObject.ScaleAppry();

                    if (ThisObject.Quad == null) ThisObject.DisplayDecalMat = new Material(Shader.Find("Hidden/DisplayDecalTexture"));
                    ThisObject.DisplayDecalMat.mainTexture = DecalTexEditValue;
                    if (ThisObject.Quad == null) ThisObject.Quad = AssetDatabase.LoadAllAssetsAtPath("Library/unity default resources").ToList().Find(i => i.name == "Quad") as Mesh;

                }
            }
            {
                var S_Scale = This_S_Object.FindProperty("Scale");
                var ScaleValue = S_Scale.floatValue;
                var ScaleEditValue = EditorGUILayout.FloatField("Scale", ScaleValue);
                if (ScaleValue != ScaleEditValue)
                {
                    Undo.RecordObject(ThisObject, "AppryScaile - Scale");
                    ThisObject.Scale = ScaleEditValue;
                    ThisObject.ScaleAppry();
                }
            }
            {
                var S_MaxDistans = This_S_Object.FindProperty("MaxDistans");
                var MaxDistansValue = S_MaxDistans.floatValue;
                var MaxDistansEditValue = EditorGUILayout.FloatField("MaxDistans", MaxDistansValue);
                if (MaxDistansValue != MaxDistansEditValue)
                {
                    Undo.RecordObject(ThisObject, "AppryScaile - MaxDistans");
                    ThisObject.MaxDistans = MaxDistansEditValue;
                    ThisObject.ScaleAppry();
                }
            }

            TextureTransformerEditor.TextureTransformerEditorDrow(ThisObject);

            This_S_Object.ApplyModifiedProperties();
        }
    }


}
#endif