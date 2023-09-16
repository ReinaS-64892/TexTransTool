#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V0
{
    [Obsolete]
    internal static class AbstractDecalV0
    {
        public static void MigrationAbstractDecalV0ToV1(AbstractDecal abstractDecal)
        {
            if (abstractDecal == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (abstractDecal.SaveDataVersion != 0) { Debug.Log(abstractDecal.name + " AbstractDecal : マイグレーションのバージョンが違います。"); return; }

            var GameObject = abstractDecal.gameObject;


            if (abstractDecal.MigrationV0DataAbstractDecal != null)
            {
                if (abstractDecal.IsSeparateMatAndTexture)
                {
                    if (abstractDecal.MigrationV0DataMatAndTexSeparator == null)
                    {
                        var NewSeparator = abstractDecal.MigrationV0DataMatAndTexSeparatorGameObject.AddComponent<MatAndTexUtils.MatAndTexRelativeSeparator>();
                        SetUpSeparator(NewSeparator, abstractDecal);
                        abstractDecal.MigrationV0DataMatAndTexSeparator = NewSeparator;
                    }
                    else
                    {
                        //すでにあるなら何もする必要はない。上は消されてるから再生成。
                    }
                }
                else
                {
                    if (abstractDecal.MigrationV0DataMatAndTexSeparator != null) { UnityEngine.Object.DestroyImmediate(abstractDecal.MigrationV0DataMatAndTexSeparator); }
                }
                abstractDecal.MigrationV0DataAbstractDecal.CopyFromDecal(abstractDecal);
                abstractDecal.MigrationV0DataAbstractDecal.MigrationV0ClearTarget = false;
                abstractDecal.MigrationV0DataAbstractDecal.IsSeparateMatAndTexture = false;
            }
            else
            {
                if (abstractDecal.IsSeparateMatAndTexture)
                {
                    var texTransParentGroup = GameObject.AddComponent<TexTransParentGroup>();

                    var newGameObjectSeparator = new GameObject("Separator");
                    newGameObjectSeparator.transform.parent = GameObject.transform;
                    var NewSeparator = newGameObjectSeparator.AddComponent<MatAndTexUtils.MatAndTexRelativeSeparator>();
                    SetUpSeparator(NewSeparator, abstractDecal);

                    var newGameObjectDecal = new GameObject("Decal");
                    newGameObjectDecal.transform.parent = GameObject.transform;
                    var NewDecal = newGameObjectDecal.AddComponent(abstractDecal.GetType()) as AbstractDecal;
                    NewDecal.CopyFromDecal(abstractDecal);
                    NewDecal.IsSeparateMatAndTexture = false;
                    NewDecal.MigrationV0ClearTarget = false;
                    MigrationUtility.SetSaveDataVersion(NewDecal, 1);

                    abstractDecal.MigrationV0DataAbstractDecal = NewDecal;
                    abstractDecal.MigrationV0DataMatAndTexSeparator = NewSeparator;
                    abstractDecal.MigrationV0DataMatAndTexSeparatorGameObject = NewSeparator.gameObject;
                    abstractDecal.MigrationV0ClearTarget = true;

                }
                else
                {
                    //何もしなくてよい
                }
            }
        }
        public static void FinalizeMigrationAbstractDecalV0ToV1(AbstractDecal abstractDecal)
        {
            if (abstractDecal.MigrationV0ClearTarget)
            {
                UnityEngine.Object.DestroyImmediate(abstractDecal);
            }
            else
            {
                MigrationUtility.SetSaveDataVersion(abstractDecal, 1);
            }
        }

        static void SetUpSeparator(MatAndTexUtils.MatAndTexRelativeSeparator matAndTexSeparator, AbstractDecal abstractDecal)
        {
            if (abstractDecal.TargetRenderers != null)
            {
                matAndTexSeparator.TargetRenderers = abstractDecal.TargetRenderers;
                var separateTarget = new List<List<bool>>();
                for (int rd = 0; abstractDecal.TargetRenderers.Count > rd; rd += 1)
                {
                    var boolList = new List<bool>();
                    var renderer = abstractDecal.TargetRenderers[rd];
                    if (renderer == null) { continue; }
                    var materials = renderer.sharedMaterials;
                    for (int ms = 0; materials.Length > ms; ms += 1)
                    {
                        boolList.Add(true);
                    }
                    separateTarget.Add(boolList);
                }
                matAndTexSeparator.SeparateTarget = separateTarget;
            }
            matAndTexSeparator.IsTextureSeparate = true;
            matAndTexSeparator.PropertyName = abstractDecal.TargetPropertyName;
        }

        static void CopyFromDecal(this AbstractDecal target, AbstractDecal copySouse)
        {
            if (target.GetType() != copySouse.GetType()) { return; };
            var fieldInfos = target.GetType().GetFields();

            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.IsStatic) { continue; }
                fieldInfo.SetValue(target, fieldInfo.GetValue(copySouse));
            }

        }
    }
}
#endif