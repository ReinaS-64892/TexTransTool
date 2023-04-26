#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Rs.TexturAtlasCompiler.ShaderSupport;

namespace Rs.TexturAtlasCompiler
{
    [CreateAssetMenu(fileName = "CompileDataContenar", menuName = "Rs/CompileDataContenar")]
    public class CompileDataContenar : ScriptableObject
    {
        public string Hash;//hashから自動でコンパイルするかどうか見るのではなく全部マニュアルでやってしまったほうが単純なのではないだろうか...
        public List<Mesh> Meshs = new List<Mesh>();
        public List<PropAndTexture> PropAndTextures = new List<PropAndTexture>();
        public List<Material> GenereatMaterial = new List<Material>();
        //public string TexturePath = null;

        private string ThisPath => AssetDatabase.GetAssetPath(this);

        public void SetSubAsset<T>(List<T> Assets) where T : UnityEngine.Object
        {
            ClearAssets<T>();
            foreach (var Asset in Assets)
            {
                //mat.mainTexture = TextureAndDistansMap.texture2D;
                AssetDatabase.AddObjectToAsset(Asset, this);
                //Debug.Log(mat.shader.name);
            }
            AssetDatabase.ImportAsset(ThisPath);
        }

        public void ClearAssets<T>() where T : UnityEngine.Object
        {
            foreach (var asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(ThisPath))
            {
                if (asset is T assett && AssetDatabase.IsSubAsset(asset))
                {
                    DestroyImmediate(assett, true);
                }
            }
        }
        public void DeletTexture()
        {
            foreach (var TexAndDist in PropAndTextures)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(TexAndDist.Texture2D));
            }
            PropAndTextures.Clear();
            /*
            if (!string.IsNullOrEmpty(TexturePath))
            {
                AssetDatabase.DeleteAsset(TexturePath);
            }*/
        }

        public void SetTexture(PropAndTexture Souse)
        {
            PropAndTextures.Add(Souse);
            var FilePath = ThisPath.Replace(Path.GetExtension(ThisPath), "");
            FilePath += Souse.PropertyName + "_GenereatAtlasTex" + ".png";
            //TexturePath = FilePath;
            File.WriteAllBytes(FilePath, Souse.Texture2D.EncodeToPNG());
            AssetDatabase.ImportAsset(FilePath);
            PropAndTextures[PropAndTextures.IndexOf(Souse)].Texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(FilePath);
        }
        public static CompileDataContenar CreateCompileDataContenar(string path)
        {
            var newI = CreateInstance<CompileDataContenar>();
            AssetDatabase.CreateAsset(newI, path);
            return newI;
        }

        public List<Material> GeneratCompileTexturedMaterial(List<Material> SouseMatrial, bool IsClearUnusedProperties)
        {
            List<Material> NoDuplicationSousMatrial = new List<Material>();
            List<Material> GeneratMats = new List<Material>();

            List<Material> ResGenereatMats = new List<Material>();

            foreach (var SMat in SouseMatrial)
            {
                if (!NoDuplicationSousMatrial.Contains(SMat))
                {
                    NoDuplicationSousMatrial.Add(SMat);

                    var Gmat = UnityEngine.Object.Instantiate<Material>(SMat);

                    PropToMaterialTexAppry(PropAndTextures, Gmat);

                    if (IsClearUnusedProperties) RemoveUnusedProperties(Gmat);
                    MaterialCustomSetting(Gmat);

                    GeneratMats.Add(Gmat);
                    ResGenereatMats.Add(Gmat);
                }
                else
                {
                    var GmatIndex = NoDuplicationSousMatrial.IndexOf(SMat);
                    var Gmat = GeneratMats[GmatIndex];

                    ResGenereatMats.Add(Gmat);
                }

            }

            SetSubAsset(GeneratMats);
            GenereatMaterial = GeneratMats;
            return ResGenereatMats;
        }

        public static void PropToMaterialTexAppry(List<PropAndTexture> PropAndTextures, Material TargetMat)
        {
            foreach (var propAndTexture in PropAndTextures)
            {
                if (TargetMat.GetTexture(propAndTexture.PropertyName) is Texture2D)
                {
                    TargetMat.SetTexture(propAndTexture.PropertyName, propAndTexture.Texture2D);
                }
            }
        }

        public static void MaterialCustomSetting(Material material)
        {
            var SuppotShder = ShaderSupportUtil.GetSupprotInstans().Find(i => material.shader.name.Contains(i.SupprotShaderName));
            if (SuppotShder == null) return;
            SuppotShder.GenereatMaterialCustomSetting(material);
        }


        //MIT License
        //Copyright (c) 2020-2021 lilxyzw
        //https://github.com/lilxyzw/lilToon/blob/master/Assets/lilToon/Editor/lilMaterialUtils.cs
        //https://light11.hatenadiary.com/entry/2018/12/04/224253
        private static void RemoveUnusedProperties(Material material)
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

        private static void DeleteUnused(ref SerializedProperty props, Material material)
        {
            for (int i = props.arraySize - 1; i >= 0; i--)
            {
                if (!material.HasProperty(props.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue))
                {
                    props.DeleteArrayElementAtIndex(i);
                }
            }
        }




        public CompileDataContenar()
        {

        }


    }
}
#endif