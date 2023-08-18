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
        public List<List<PropAndTexture>> AtlasTextures
        {
            get => ConvartSubList(_atlasTextures.Cast<SubList<PropAndTexture>>()); set
            {
                var AtlasTextures = value;
                ClearAtlasTextures();
                if (AtlasTextures == null) return;
                var ConvAtlasTextures = SubListPropAndTexture.ConvartSubList(AtlasTextures);
                foreach (var item in ConvAtlasTextures.SelectMany(I => I.SubListInstans))
                {
                    AssetDatabase.CreateAsset(item.Texture2D, AssetSaveHelper.GenereatAssetPath(item.Texture2D.name, ".asset"));

                }
                _atlasTextures = ConvAtlasTextures;
            }
        }

        [SerializeField] List<MeshAndMatRef> _meshes;
        public List<MeshAndMatRef> GenereatMeshs
        {
            get => _meshes; set
            {
                ClearMeshs();
                if (value == null) return;
                var Meshs = value;
                foreach (var item in Meshs)
                {
                    AssetDatabase.CreateAsset(item.Mesh, AssetSaveHelper.GenereatAssetPath(item.Mesh.name, ".asset"));
                }
                _meshes = Meshs;

            }
        }

        [SerializeField] List<SubListInt> _matRefarens;
        public List<List<int>> ChannnelsMatRef { get => ConvartSubList(_matRefarens.Cast<SubList<int>>()); set => _matRefarens = SubListInt.ConvartSubList(value); }



        [SerializeField] List<SubListMaterial> _genereatMaterials;
        public List<List<Material>> GenereatMaterials
        {
            get => ConvartSubList(_genereatMaterials.Cast<SubList<Material>>());
            set
            {
                ClearGenereatMaterials();
                if (value == null) return;
                _genereatMaterials = SubListMaterial.ConvartSubList(value);
            }
        }
        [SerializeField] bool _IsPossibleApply;
        public bool IsPossibleApply
        {
            get
            {
                var IsPossibleMesh = true;
                foreach (var item in _meshes)
                {
                    if (item.Mesh == null)
                    {
                        IsPossibleMesh = false;
                        break;
                    }
                }
                return _IsPossibleApply && IsPossibleMesh;
            }
            set => _IsPossibleApply = value;
        }

        void ClearAtlasTextures()
        {
            if (_atlasTextures == null) return;
            AssetSaveHelper.DeletAssets(_atlasTextures.SelectMany(I => I.SubListInstans.Select(J => J.Texture2D)));
            _atlasTextures.Clear();
        }
        void ClearMeshs()
        {
            if (_meshes == null) { return; }
            AssetSaveHelper.DeletAssets(_meshes.Select(I => I.Mesh));
            _meshes.Clear();
        }
        void ClearGenereatMaterials()
        {
            if (_genereatMaterials == null) { return; }
            AssetSaveHelper.DeletAssets(_genereatMaterials.SelectMany(I => I.SubListInstans));
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