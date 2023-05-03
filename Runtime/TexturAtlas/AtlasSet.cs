#if UNITY_EDITOR
using System;
using System.Security;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool;


namespace Rs64.TexTransTool
{
    [System.Serializable]
    public class AtlasSet
    {
        public List<SkinnedMeshRenderer> AtlasTargetMeshs;
        public List<MeshRenderer> AtlasTargetStaticMeshs;
        public Vector2Int AtlasTextureSize = new Vector2Int(2048, 2048);
        public float Pading = -10;
        public PadingType PadingType;
        public IslandSortingType SortingType;
        public CompileDataContenar Contenar;

        public bool GeneratMatClearUnusedProperties = true;

        [SerializeField] bool _IsAppry;

        public bool IsAppry => _IsAppry;
        public Action<CompileDataContenar> AtlasCompilePostCallBack = (i) => { };
        [SerializeField] List<Mesh> BackUpMeshs = new List<Mesh>();
        [SerializeField] List<Mesh> BackUpStaticMeshs = new List<Mesh>();
        [SerializeField] List<Material> BackUpMaterial = new List<Material>();
        public void Appry()
        {
            if (Contenar == null) return;
            if (_IsAppry == true) return;
            MeshAppry();
            MaterialAppry();

            _IsAppry = true;
        }
        public void Revart()
        {
            if (Contenar == null) return;
            if (_IsAppry == false) return;
            MeshRevart();
            MaterialRevart();
            _IsAppry = false;
        }
        public void MeshAppry()
        {
            if (Contenar == null) return;
            if (_IsAppry == true) return;
            BackUpMeshs.Clear();
            BackUpStaticMeshs.Clear();

            int count = -1;
            foreach (var mesh in Contenar.Meshs)
            {
                count += 1;
                //Debug.Log(AtlasTargetMeshs.Count);
                if (count < AtlasTargetMeshs.Count)
                {
                    BackUpMeshs.Add(AtlasTargetMeshs[count].sharedMesh);
                    AtlasTargetMeshs[count].sharedMesh = mesh;
                }
                else
                {
                    var target = AtlasTargetStaticMeshs[count - AtlasTargetMeshs.Count].GetComponent<MeshFilter>();
                    BackUpStaticMeshs.Add(target.sharedMesh);
                    target.sharedMesh = mesh;
                }

            }
        }
        public void MeshRevart()
        {
            if (_IsAppry == false) return;
            int Count = -1;
            foreach (var mesh in BackUpMeshs)
            {
                Count += 1;
                AtlasTargetMeshs[Count].sharedMesh = mesh;
            }
            Count = -1;
            foreach (var mesh in BackUpStaticMeshs)
            {
                Count += 1;
                AtlasTargetStaticMeshs[Count].GetComponent<MeshFilter>().sharedMesh = mesh;
            }
        }
        public void MaterialAppry()
        {
            if (Contenar == null) return;
            if (_IsAppry == true) return;

            BackUpMaterial.Clear();
            BackUpMaterial = GetMaterials();

            var GeneratMats = Contenar.GeneratCompileTexturedMaterial(GetMaterials(), GeneratMatClearUnusedProperties);
            SetMaterial(GetRenderers(), GeneratMats);

        }
        public void MaterialRevart()
        {
            if (_IsAppry == false) return;

            Contenar.GenereatMaterial.Clear();
            Contenar.ClearAssets<Material>();

            SetMaterial(GetRenderers(), BackUpMaterial);
        }

        static void SetMaterial(List<Renderer> renderers, List<Material> SouseMats)
        {
            int Count = -1;
            foreach (var render in renderers)
            {
                var Mats = render.sharedMaterials;
                for (int i = 0; Mats.Length > i; i += 1)
                {
                    Count += 1;
                    Mats[i] = SouseMats[Count];
                }
                render.sharedMaterials = Mats;
            }
        }

        List<Material> GetMaterials()
        {
            var Renderers = GetRenderers();
            List<Material> Mats = new List<Material>();

            foreach (var Renderer in Renderers)
            {
                foreach (var Mat in Renderer.sharedMaterials)
                {
                    Mats.Add(Mat);
                }
            }

            return Mats;

        }
        List<Renderer> GetRenderers()
        {
            List<Renderer> Renderers = new List<Renderer>(AtlasTargetMeshs);
            Renderers.AddRange(AtlasTargetStaticMeshs);
            return Renderers;
        }


    }
}
#endif