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
        public TextureAndDistansMap TextureAndDistansMap;
        //public string TexturePath = null;

        private string ThisPath => AssetDatabase.GetAssetPath(this);
        public void SetMeshEditableClone(CompileData Souse)
        {
            ClearAssets<Mesh>();
            List<Mesh> ClonedMesh = new List<Mesh>();
            foreach (var mesh in Souse.meshes)
            {
                var clonemesh = Instantiate<Mesh>(mesh);
                AssetDatabase.AddObjectToAsset(clonemesh, this);
                ClonedMesh.Add(clonemesh);
            }
            AssetDatabase.ImportAsset(ThisPath);
            Souse.meshes = ClonedMesh;
            Meshs = ClonedMesh;

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
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(TextureAndDistansMap.texture2D));
            TextureAndDistansMap = null;
            /*
            if (!string.IsNullOrEmpty(TexturePath))
            {
                AssetDatabase.DeleteAsset(TexturePath);
            }*/
        }

        public void SetTexture(TextureAndDistansMap Souse)
        {
            TextureAndDistansMap = Souse;
            var FilePath = ThisPath.Replace(Path.GetExtension(ThisPath), "");
            FilePath += "_GenereatAtlasTex" + ".png";
            //TexturePath = FilePath;
            File.WriteAllBytes(FilePath, Souse.texture2D.EncodeToPNG());
            AssetDatabase.ImportAsset(FilePath);
            TextureAndDistansMap.texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(FilePath);
        }
        public static CompileDataContenar CreateCompileDataContenar(string path)
        {
            var newI = CreateInstance<CompileDataContenar>();
            AssetDatabase.CreateAsset(newI, path);
            return newI;
        }

        public void SetMaterial(List<Material> mats)
        {
            ClearAssets<Material>();
            var Clonemats = new List<Material>();
            foreach (var mat in mats)
            {
                var Clonemat = Instantiate<Material>(mat);
                Clonemat.mainTexture = TextureAndDistansMap.texture2D;
                AssetDatabase.AddObjectToAsset(Clonemat, this);
                Clonemats.Add(Clonemat);
            }
            AssetDatabase.ImportAsset(ThisPath);
            Mat = Clonemats;
        }

        public CompileDataContenar()
        {

        }


    }
}
#endif