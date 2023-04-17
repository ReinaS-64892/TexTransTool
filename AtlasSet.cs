#if UNITY_EDITOR
using System.Security;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Rs.TexturAtlasCompiler;


namespace Rs.TexturAtlasCompiler
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

        [SerializeField]bool _IsAppry;

        public bool IsAppry => _IsAppry;
        [SerializeField]List<Mesh> BackUpMeshs = new List<Mesh>();
        [SerializeField]List<Mesh> BackUpStaticMeshs = new List<Mesh>();
        [SerializeField]List<Material> BackUpMaterial = new List<Material>();
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
            int Count = -1;
            List<Renderer> renderers = new List<Renderer>(AtlasTargetMeshs);
            renderers.AddRange(AtlasTargetStaticMeshs);
            foreach (var render in renderers)
            {
                var mats = render.sharedMaterials;
                for (int i = 0; mats.Length > i; i += 1)
                {
                    if (mats[i].mainTexture != null)
                    {
                        Count += 1;
                        BackUpMaterial.Add(mats[i]);
                        mats[i] = Contenar.Mat[Count];
                    }
                }
                render.sharedMaterials = mats;
            }


        }
        public void MaterialRevart()
        {
            if (_IsAppry == false) return;

            int Count = -1;
            List<Renderer> renderers = new List<Renderer>(AtlasTargetMeshs);
            renderers.AddRange(AtlasTargetStaticMeshs);
            foreach (var render in renderers)
            {
                var mats = render.sharedMaterials;
                for (int i = 0; mats.Length > i; i += 1)
                {
                    if (mats[i].mainTexture != null)
                    {
                        Count += 1;
                        mats[i] = BackUpMaterial[Count];
                    }
                }
                render.sharedMaterials = mats;
            }
        }


    }
}
#endif