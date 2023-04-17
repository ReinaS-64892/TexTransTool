#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Rs.TexturAtlasCompiler
{
    [CreateAssetMenu(fileName = "CompileDataContenar", menuName = "Rs/CompileDataContenar")]
    public class CompileDataContenar : ScriptableObject
    {
        public string Hash;
        public List<Mesh> Meshs;
        public List<Material> Mat;
        public List<PropatyAndTextureAndDistans> TextureAndDistansMap;
        //public string TexturePath = null;

        private string ThisPath => AssetDatabase.GetAssetPath(this);
        public void SetMesh(CompileData Souse)
        {
            ClearAssets<Mesh>();
            foreach (var mesh in Souse.meshes)
            {
                AssetDatabase.AddObjectToAsset(mesh, this);
            }
            AssetDatabase.ImportAsset(ThisPath);
            Meshs = Souse.meshes;

        }
        public void SetMaterial(List<Material> mats)
        {
            ClearAssets<Material>();
            foreach (var mat in mats)
            {
                //mat.mainTexture = TextureAndDistansMap.texture2D;
                AssetDatabase.AddObjectToAsset(mat, this);
                //Debug.Log(mat.shader.name);
            }
            AssetDatabase.ImportAsset(ThisPath);
            Mat = mats;
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
            foreach (var TexAndDist in TextureAndDistansMap)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(TexAndDist.TextureAndDistansMap.Texture2D));
            }
            TextureAndDistansMap.Clear();
            /*
            if (!string.IsNullOrEmpty(TexturePath))
            {
                AssetDatabase.DeleteAsset(TexturePath);
            }*/
        }

        public void SetTexture(PropatyAndTextureAndDistans Souse)
        {
            TextureAndDistansMap.Add(Souse);
            var FilePath = ThisPath.Replace(Path.GetExtension(ThisPath), "");
            FilePath += Souse.PropertyName + "_GenereatAtlasTex" + ".png";
            //TexturePath = FilePath;
            File.WriteAllBytes(FilePath, Souse.TextureAndDistansMap.Texture2D.EncodeToPNG());
            AssetDatabase.ImportAsset(FilePath);
            TextureAndDistansMap[TextureAndDistansMap.IndexOf(Souse)].TextureAndDistansMap.Texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(FilePath);
        }
        public static CompileDataContenar CreateCompileDataContenar(string path)
        {
            var newI = CreateInstance<CompileDataContenar>();
            AssetDatabase.CreateAsset(newI, path);
            return newI;
        }


        public CompileDataContenar()
        {

        }


    }
}
#endif