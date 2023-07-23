#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Rs64.TexTransTool.ShaderSupport;

namespace Rs64.TexTransTool.TexturAtlas
{
    [Serializable]
    public class AtlasTextureDataContainer
    {
        [SerializeField] List<SubListPropAndTexture> _atlasTextures;
        public List<List<PropAndTexture>> AtlasTextures { get => ConvartSubList(_atlasTextures.Cast<SubList<PropAndTexture>>()); set => SetAtlasTextures(value); }

        [SerializeField] List<MeshAndMatRef> _meshes;
        public List<MeshAndMatRef> GenereatMeshs { get => _meshes; set => SetMeshs(value); }

        [SerializeField] List<SubListInt> _matRefarens;
        public List<List<int>> ChannnelsMatRef { get => ConvartSubList(_matRefarens.Cast<SubList<int>>()); set => _matRefarens = SubListInt.ConvartSubList(value); }



        [SerializeField] List<SubListMaterial> _genereatMaterials;
        public List<List<Material>> GenereatMaterials { get => ConvartSubList(_genereatMaterials.Cast<SubList<Material>>()); set => ConvartSubList(value); }



        [SerializeField] bool _IsPossibleApply;
        public bool IsPossibleApply { get => _IsPossibleApply; set => _IsPossibleApply = value; }

        public void SetAtlasTextures(List<List<PropAndTexture>> AtlasTextures)
        {
            ClearAtlasTextures();
            if (AtlasTextures == null) return;
            var ConvAtlasTextures = SubListPropAndTexture.ConvartSubList(AtlasTextures);
            var count = ConvAtlasTextures.Count;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < ConvAtlasTextures[i].Count; j++)
                {
                    var porp2Tex = ConvAtlasTextures[i][j];
                    porp2Tex.Texture2D = AssetSaveHelper.SaveAsset(porp2Tex.Texture2D);
                    ConvAtlasTextures[i][j] = porp2Tex;
                }
            }
            _atlasTextures = ConvAtlasTextures;
        }
        void ClearAtlasTextures()
        {
            if (_atlasTextures == null) return;
            foreach (var item in _atlasTextures.SelectMany(I => I.SubListInstans))
            {
                AssetSaveHelper.DeletAsset(item.Texture2D);
            }
            _atlasTextures.Clear();
        }
        public void SetMeshs(List<MeshAndMatRef> Meshs)
        {
            ClearMeshs();
            if (Meshs == null) return;
            var count = Meshs.Count;
            for (int i = 0; i < count; i++)
            {
                var mesh2matref = Meshs[i];
                mesh2matref.Mesh = AssetSaveHelper.SaveAsset(mesh2matref.Mesh);
                Meshs[i] = mesh2matref;
            }
            _meshes = Meshs;
        }
        void ClearMeshs()
        {
            if (_meshes == null) return;
            foreach (var item in _meshes)
            {
                AssetSaveHelper.DeletAsset(item.Mesh);
            }
            _meshes.Clear();
        }

        public void SetGenereatMaterials(List<List<Material>> GenereatMaterials)
        {
            ClearGenereatMaterials();
            if (GenereatMaterials == null) return;
            var ConvartSubMeshs = SubListMaterial.ConvartSubList(GenereatMaterials);
            var count = ConvartSubMeshs.Count;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < ConvartSubMeshs[i].Count; j++)
                {
                    var mat = ConvartSubMeshs[i][j];
                    mat = AssetSaveHelper.SaveAsset(mat);
                    ConvartSubMeshs[i][j] = mat;
                }
            }
            _genereatMaterials = ConvartSubMeshs;
        }
        void ClearGenereatMaterials()
        {
            if (_genereatMaterials == null) return;
            foreach (var item in _genereatMaterials.SelectMany(I => I.SubListInstans))
            {
                AssetSaveHelper.DeletAsset(item);
            }
            _genereatMaterials.Clear();
        }

        public static List<List<T2>> ConvartSubList<T2>(List<SubList<T2>> Souse)
        {
            if (Souse == null) return null;
            var result = new List<List<T2>>();
            foreach (var item in Souse)
            {
                result.Add(item.SubListInstans);
            }
            return result;
        }
        public static List<List<T2>> ConvartSubList<T2>(IEnumerable<SubList<T2>> Souse)
        {
            if (Souse == null) return null;
            var result = new List<List<T2>>();
            foreach (var item in Souse)
            {
                result.Add(item.SubListInstans);
            }
            return result;
        }
        public static List<SubList<T2>> ConvartSubList<T2>(List<List<T2>> Souse)
        {
            if (Souse == null) return null;
            var result = new List<SubList<T2>>();
            foreach (var item in Souse)
            {
                result.Add(new SubList<T2>(item));
            }
            return result;
        }
        public static List<SubList<T2>> ConvartSubList<T2>(IEnumerable<List<T2>> Souse)
        {
            if (Souse == null) return null;
            var result = new List<SubList<T2>>();
            foreach (var item in Souse)
            {
                result.Add(new SubList<T2>(item));
            }
            return result;
        }
        public AtlasTextureDataContainer()
        {

        }
        [Serializable]
        public class MeshAndMatRef
        {
            public int RefMesh;
            public Mesh Mesh;
            public int[] MatRefs;

            public MeshAndMatRef(int refMesh, Mesh mesh, int[] matrefs)
            {
                RefMesh = refMesh;
                Mesh = mesh;
                MatRefs = matrefs;
            }
            public MeshAndMatRef()
            {

            }
        }

        [Serializable]
        public class SubList<T>
        {
            public List<T> SubListInstans;

            public T this[int index]
            {
                get => SubListInstans[index];
                set => SubListInstans[index] = value;
            }

            public int Count => SubListInstans.Count;



            public SubList(List<T> SubListInstans)
            {
                this.SubListInstans = SubListInstans;
            }

            public SubList()
            {
                SubListInstans = new List<T>();
            }

        }
        [Serializable]
        public class SubListPropAndTexture : SubList<PropAndTexture>
        {
            public SubListPropAndTexture(List<PropAndTexture> SubListInstans) : base(SubListInstans)
            {
            }

            public static List<SubListPropAndTexture> ConvartSubList(List<List<PropAndTexture>> atlasTextures)
            {
                if (atlasTextures == null) return null;
                var result = new List<SubListPropAndTexture>();
                foreach (var item in atlasTextures)
                {
                    result.Add(new SubListPropAndTexture(item));
                }
                return result;
            }

        }
        [Serializable]
        public class SubListMeshAndMatRef : SubList<MeshAndMatRef>
        {
            public SubListMeshAndMatRef(List<MeshAndMatRef> SubListInstans) : base(SubListInstans)
            {
            }

            public static List<SubListMeshAndMatRef> ConvartSubList(List<List<MeshAndMatRef>> meshs)
            {
                if (meshs == null) return null;
                var result = new List<SubListMeshAndMatRef>();
                foreach (var item in meshs)
                {
                    result.Add(new SubListMeshAndMatRef(item));
                }
                return result;
            }
        }
        [Serializable]
        public class SubListMaterial : SubList<Material>
        {
            public SubListMaterial(List<Material> SubListInstans) : base(SubListInstans)
            {
            }


            public static List<SubListMaterial> ConvartSubList(List<List<Material>> genereatMaterials)
            {
                if (genereatMaterials == null) return null;
                var result = new List<SubListMaterial>();
                foreach (var item in genereatMaterials)
                {
                    result.Add(new SubListMaterial(item));
                }
                return result;
            }
        }
        [Serializable]
        public class SubListInt : SubList<int>
        {
            public SubListInt(List<int> SubListInstans) : base(SubListInstans)
            {
            }

            public static List<SubListInt> ConvartSubList(List<List<int>> matRefarens)
            {
                if (matRefarens == null) return null;
                var result = new List<SubListInt>();
                foreach (var item in matRefarens)
                {
                    result.Add(new SubListInt(item));
                }
                return result;
            }
        }




    }
}
#endif