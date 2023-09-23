#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool.Utils
{
    public static class MaterialUtility
    {
        public static void SetTextures(this Material TargetMat, List<PropAndTexture2D> PropAndTextures, bool FocusSetTexture = false)
        {
            foreach (var propAndTexture in PropAndTextures)
            {
                if (!TargetMat.HasProperty(propAndTexture.PropertyName)) { continue; }
                if (FocusSetTexture || TargetMat.GetTexture(propAndTexture.PropertyName) is Texture2D)
                {
                    TargetMat.SetTexture(propAndTexture.PropertyName, propAndTexture.Texture2D);
                }
            }
        }

        //MIT License
        //Copyright (c) 2020-2021 lilxyzw
        //https://github.com/lilxyzw/lilToon/blob/master/Assets/lilToon/Editor/lilMaterialUtils.cs
        //
        //https://light11.hatenadiary.com/entry/2018/12/04/224253
        public static void RemoveUnusedProperties(this Material material)
        {
            var so = new SerializedObject(material);
            so.Update();
            var savedProps = so.FindProperty("m_SavedProperties");

            var texs = savedProps.FindPropertyRelative("m_TexEnvs");
            DeleteUnused(ref texs, material);

            var floats = savedProps.FindPropertyRelative("m_Floats");
            DeleteUnused(ref floats, material);

            var colors = savedProps.FindPropertyRelative("m_Colors");
            DeleteUnused(ref colors, material);

            so.ApplyModifiedProperties();
        }

        public static void DeleteUnused(ref SerializedProperty props, Material material)
        {
            for (int i = props.arraySize - 1; i >= 0; i--)
            {
                if (!material.HasProperty(props.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue))
                {
                    props.DeleteArrayElementAtIndex(i);
                }
            }
        }

        public static Dictionary<string, Texture2D> GetPropAndTextures(Material material)
        {
            var so = new SerializedObject(material);
            so.Update();
            var savedProps = so.FindProperty("m_SavedProperties");

            var texs = savedProps.FindPropertyRelative("m_TexEnvs");

            Dictionary<string, Texture2D> PropAndTextures = new Dictionary<string, Texture2D>();

            for (int i = 0; i < texs.arraySize; i++)
            {
                var prop = texs.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
                var tex = texs.GetArrayElementAtIndex(i).FindPropertyRelative("second.m_Texture").objectReferenceValue as Texture2D;
                PropAndTextures.Add(prop, tex);
            }

            return PropAndTextures;
        }

        public static Dictionary<string, Texture2D> FiltalingUnused(Dictionary<string, Texture2D> PropAndTextures, Material material)
        {
            Dictionary<string, Texture2D> FiltalingPropAndTextures = new Dictionary<string, Texture2D>();
            foreach (var kvp in PropAndTextures)
            {
                if (material.HasProperty(kvp.Key))
                {
                    FiltalingPropAndTextures.Add(kvp.Key, kvp.Value);
                }
            }
            return FiltalingPropAndTextures;
        }

    }
}
#endif
