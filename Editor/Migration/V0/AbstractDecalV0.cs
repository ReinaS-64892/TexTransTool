using System;
using System.Collections.Generic;
using net.rs64.TexTransTool.Decal;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V0
{
    [Obsolete]
    internal static class AbstractDecalV0
    {
        public static void MigrationAbstractDecalV0ToV1(SimpleDecal abstractDecal)
        {
            if (abstractDecal == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            if (abstractDecal is ITexTransToolTag TTTag && TTTag.SaveDataVersion > 1) { Debug.Log(abstractDecal.name + " AbstractDecal : マイグレーション不可能なバージョンです。"); return; }

            var GameObject = abstractDecal.gameObject;


            if (abstractDecal.MigrationV0DataAbstractDecal != null)
            {
                abstractDecal.MigrationV0DataAbstractDecal.CopyFromDecal(abstractDecal);
                abstractDecal.MigrationV0DataAbstractDecal.MigrationV0ClearTarget = false;
                abstractDecal.MigrationV0DataAbstractDecal.IsSeparateMatAndTexture = false;
                EditorUtility.SetDirty(abstractDecal.MigrationV0DataAbstractDecal);
            }
            else
            {
                if (abstractDecal.IsSeparateMatAndTexture)
                {

                    var newGameObjectSeparator = new GameObject("Separator");
                    newGameObjectSeparator.transform.parent = GameObject.transform;


                    var newGameObjectDecal = new GameObject("Decal");
                    newGameObjectDecal.transform.parent = GameObject.transform;
                    var NewDecal = newGameObjectDecal.AddComponent(abstractDecal.GetType()) as SimpleDecal;
                    NewDecal.CopyFromDecal(abstractDecal);
                    abstractDecal.HighQualityPadding = abstractDecal.FastMode;
                    NewDecal.IsSeparateMatAndTexture = false;
                    NewDecal.MigrationV0ClearTarget = false;
                    NewDecal.MigrationV0DataAbstractDecal = null;
                    NewDecal.MigrationV0DataMatAndTexSeparatorGameObject = null;
                    EditorUtility.SetDirty(NewDecal);

                    abstractDecal.MigrationV0DataAbstractDecal = NewDecal;
                    abstractDecal.MigrationV0ClearTarget = true;

                }
                else
                {
                    abstractDecal.HighQualityPadding = abstractDecal.FastMode;
                }
            }
        }
        public static void FinalizeMigrationAbstractDecalV0ToV1(SimpleDecal abstractDecal)
        {
            if (abstractDecal.MigrationV0ClearTarget)
            {
                var go = abstractDecal.gameObject;
                UnityEngine.Object.DestroyImmediate(abstractDecal);
                go.AddComponent<TexTransGroup>();
            }
            else
            {
                MigrationUtility.SetSaveDataVersion(abstractDecal, 1);
            }
        }


        static void CopyFromDecal(this SimpleDecal target, SimpleDecal copySource)
        {
            if (target.GetType() != copySource.GetType()) { return; };
            var fieldInfos = target.GetType().GetFields();

            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.IsStatic) { continue; }
                fieldInfo.SetValue(target, fieldInfo.GetValue(copySource));
            }

        }
    }
}
