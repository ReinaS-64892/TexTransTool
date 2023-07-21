#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Rs64.TexTransTool.ShaderSupport;

namespace Rs64.TexTransTool.TexturAtlas
{
    [System.Serializable]
    public class AtlasTextureDataContainer
    {
        [SerializeField] List<List<PropAndTexture>> _atlasTextures;
        public List<List<PropAndTexture>> AtlasTextures { get => _atlasTextures; set => SetAtlasTextures(value); }

        [SerializeField] List<List<MeshAndMatRef>> _meshes;
        public List<List<MeshAndMatRef>> Meshes { get => _meshes; set => _meshes = value; }

        [SerializeField]List<List<Material>> _genereatMaterials;
        public List<List<Material>> GenereatMaterials { get => _genereatMaterials; set => _genereatMaterials = value; }



        [SerializeField] bool _IsPossibleApply;
        public bool IsPossibleApply { get => _IsPossibleApply; set => _IsPossibleApply = value; }

        public void SetAtlasTextures(List<List<PropAndTexture>> AtlasTextures)
        {
            ClearAtlasTextures();
            var count = AtlasTextures.Count;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < AtlasTextures[i].Count; j++)
                {
                    var porp2Tex = AtlasTextures[i][j];
                    porp2Tex.Texture2D = AssetSaveHelper.SaveAsset(porp2Tex.Texture2D);
                    AtlasTextures[i][j] = porp2Tex;
                }
            }
            _atlasTextures = AtlasTextures;
        }
        void ClearAtlasTextures()
        {
            if (_atlasTextures == null) return;
            foreach (var item in _atlasTextures.SelectMany(I => I))
            {
                AssetSaveHelper.DeletAsset(item.Texture2D);
            }
            _atlasTextures.Clear();
        }
        public void SetMeshs(List<List<MeshAndMatRef>> Meshs)
        {
            ClearMeshs();
            var count = Meshs.Count;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < Meshs[i].Count; j++)
                {
                    var mesh2matref = Meshs[i][j];
                    mesh2matref.Mesh = AssetSaveHelper.SaveAsset(mesh2matref.Mesh);
                    Meshs[i][j] = mesh2matref;
                }
            }
            _meshes = Meshs;
        }
        void ClearMeshs()
        {
            if (_meshes == null) return;
            foreach (var item in _meshes.SelectMany(I => I))
            {
                AssetSaveHelper.DeletAsset(item.Mesh);
            }
            _meshes.Clear();
        }

        public void SetGenereatMaterials(List<List<Material>> GenereatMaterials)
        {
            ClearGenereatMaterials();
            var count = GenereatMaterials.Count;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < GenereatMaterials[i].Count; j++)
                {
                    var mat = GenereatMaterials[i][j];
                    mat = AssetSaveHelper.SaveAsset(mat);
                    GenereatMaterials[i][j] = mat;
                }
            }
            this._genereatMaterials = GenereatMaterials;
        }
        void ClearGenereatMaterials()
        {
            if (_genereatMaterials == null) return;
            foreach (var item in _genereatMaterials.SelectMany(I => I))
            {
                AssetSaveHelper.DeletAsset(item);
            }
            _genereatMaterials.Clear();
        }



        public AtlasTextureDataContainer()
        {

        }

        public class MeshAndMatRef
        {
            public int RefMesh;
            public Mesh Mesh;
            public int[] MatRefs;

            public MeshAndMatRef(int refMesh,Mesh mesh, int[] matrefs)
            {
                RefMesh = refMesh;
                Mesh = mesh;
                MatRefs = matrefs;
            }
            public MeshAndMatRef()
            {

            }
        }

    }
}
#endif