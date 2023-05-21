#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal;
using Rs64.TexTransTool.Editor;


namespace Rs64.TexTransTool.Editor.Decal
{
    public static class DecalEditorUtili
    {
        public static void DorwRendarar(SerializedProperty RendererListSP, bool MultiRendararMode)
        {

            if (!MultiRendararMode)
            {
                RendererListSP.arraySize = 1;
                var S_TRArryElemt = RendererListSP.GetArrayElementAtIndex(0);
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
                foreach (var Index in Enumerable.Range(0, RendererListSP.arraySize))
                {
                    var S_TargetRendererValue = RendererListSP.GetArrayElementAtIndex(Index);
                    var TargetRendererValue = S_TargetRendererValue.objectReferenceValue;
                    var TargetRendererEditValue = EditorGUILayout.ObjectField("Target " + (Index + 1), TargetRendererValue, typeof(Renderer), true) as Renderer;
                    if (TargetRendererValue != TargetRendererEditValue)
                    {
                        Renderer FiltalingdRendarer = RendererFiltaling(TargetRendererEditValue);
                        S_TargetRendererValue.objectReferenceValue = FiltalingdRendarer;
                    }
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+")) RendererListSP.arraySize += 1;
                EditorGUI.BeginDisabledGroup(RendererListSP.arraySize <= 1);
                if (GUILayout.Button("-")) RendererListSP.arraySize -= 1;
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
        }
        public static void DrowTextureFiled(SerializedProperty SerializedTexture, Action<Texture2D> CallBack)
        {
            var DecalTexValue = SerializedTexture.objectReferenceValue;
            var DecalTexEditValue = EditorGUILayout.ObjectField("DecalTexture", DecalTexValue, typeof(Texture2D), false) as Texture2D;
            if (DecalTexValue != DecalTexEditValue)
            {
                SerializedTexture.objectReferenceValue = DecalTexEditValue;

                if (CallBack != null) CallBack.Invoke(DecalTexEditValue);
            }


        }

        public static Renderer RendererFiltaling(Renderer TargetRendererEditValue)
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

        public static void DrowArryResizeButton(SerializedProperty ArrayPorpatye)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+")) ArrayPorpatye.arraySize += 1;
            EditorGUI.BeginDisabledGroup(ArrayPorpatye.arraySize <= 1);
            if (GUILayout.Button("-")) ArrayPorpatye.arraySize -= 1;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
    }


}
#endif