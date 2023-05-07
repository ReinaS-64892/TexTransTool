#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Rs64.TexTransTool.Decal
{
    //[AddComponentMenu("TexTransTool/SimpleDecal")]
    public class SimpleDecal : TextureTransformer
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public Texture2D DecalTexture;
        public Vector2 Scale = Vector2.one;
        public float MaxDistans = 1;

        public bool AdvansdMode;
        public BlendType BlendType = BlendType.Normal;
        public bool FixedAspect = true;

        public string TargetPropatyName = "_MainTex";
        public virtual void ScaleAppry()
        {
            if (DecalTexture != null && FixedAspect)
            {
                transform.localScale = new Vector3(Scale.x, Scale.x * ((float)DecalTexture.height / (float)DecalTexture.width), MaxDistans);
            }
            else
            {
                transform.localScale = new Vector3(Scale.x, FixedAspect ? Scale.x : Scale.y, MaxDistans);
            }
        }

        [SerializeField] List<Texture2D> DecalCompiledTextures;
        [SerializeField] List<Material> DecaleBlendMaterialsSave;
        [SerializeField] List<Texture2D> DecaleBlendTexteres;
        [SerializeField] List<Material> BackUpMaterials;
        public void CompileDataClear()
        {
            AssetSaveHelper.DeletAssets(DecalCompiledTextures);
            DecalCompiledTextures = new List<Texture2D> { null };
        }
        public override void Compile()
        {
            if (_IsAppry) return;
            var ResultTexutres = new List<Texture2D>();
            foreach (var TargetRenderer in TargetRenderers)
            {
                ResultTexutres.AddRange(DecalUtil.CreatDecalTexture(TargetRenderer, DecalTexture, transform.worldToLocalMatrix, TargetPropatyName));
            }
            AssetSaveHelper.DeletAssets(DecalCompiledTextures);
            DecalCompiledTextures = AssetSaveHelper.SaveAssets(ResultTexutres);
        }
        public override void Appry()
        {
            if (_IsAppry) return;
            _IsAppry = true;
            var AllMaterials = new List<Material>();
            var AllEditMaterials = new List<Material>();
            var AllNewblendTexteres = new List<Texture2D>();
            int MaterialIndexOffset = 0;
            foreach (var TargetRenderer in TargetRenderers)
            {
                var Materials = TargetRenderer.sharedMaterials;
                var EditMaterials = new Material[Materials.Length];
                var NewblendTexteres = new Texture2D[Materials.Length];

                foreach (var Index in Enumerable.Range(0, Materials.Length))
                {
                    var EditableMaterial = Instantiate(Materials[Index]);
                    if (DecalCompiledTextures[MaterialIndexOffset + Index] != null && EditableMaterial.GetTexture(TargetPropatyName) is Texture2D BaseTex)
                    {
                        var AddTex = DecalCompiledTextures[MaterialIndexOffset + Index];
                        Compiler.NotFIlterAndReadWritTexture2D(ref BaseTex);
                        Compiler.NotFIlterAndReadWritTexture2D(ref AddTex);
                        Texture2D BlendTextere = TextureLayerUtil.BlendTextureUseComputeSheder(null, BaseTex, AddTex, BlendType);
                        var SavedPiletexure = AssetSaveHelper.SaveAsset(BlendTextere);
                        EditableMaterial.SetTexture(TargetPropatyName, SavedPiletexure);
                        NewblendTexteres[Index] = SavedPiletexure;
                    }
                    EditMaterials[Index] = EditableMaterial;
                }
                MaterialIndexOffset += Materials.Length;
                AllMaterials.AddRange(Materials);
                AllEditMaterials.AddRange(EditMaterials);
                AllNewblendTexteres.AddRange(AllNewblendTexteres);
            }
            AllEditMaterials = AssetSaveHelper.SaveAssets(AllEditMaterials);
            DecaleBlendTexteres = AllNewblendTexteres;
            DecaleBlendMaterialsSave = AllEditMaterials;
            BackUpMaterials = AllMaterials;

            int IndexCount = 0;
            foreach (var TargetRenderer in TargetRenderers)
            {
                TargetRenderer.sharedMaterials = AllEditMaterials.Skip(IndexCount).Take(TargetRenderer.sharedMaterials.Length).ToArray();
                IndexCount += TargetRenderer.sharedMaterials.Length;
            }
        }



        public override void Revart()
        {
            if (!_IsAppry) return;
            _IsAppry = false;

            int IndexCount = 0;
            foreach (var TargetRenderer in TargetRenderers)
            {
                TargetRenderer.sharedMaterials = BackUpMaterials.Skip(IndexCount).Take(TargetRenderer.sharedMaterials.Length).ToArray();
                IndexCount += TargetRenderer.sharedMaterials.Length;
            }
            AssetSaveHelper.DeletAssets(DecaleBlendMaterialsSave);
            AssetSaveHelper.DeletAssets(DecaleBlendTexteres);
            DecaleBlendMaterialsSave = null;
            BackUpMaterials = null;
            DecaleBlendTexteres = null;

        }



        [NonSerialized] public Material DisplayDecalMat;
        public Color GizmoColoro = new Color(0, 0, 0, 1);
        [NonSerialized] public Mesh Quad;

        [SerializeField] bool _IsAppry;
        public override bool IsAppry => _IsAppry;

        public override bool IsPossibleAppry => DecalCompiledTextures.Any();

        public override bool IsPossibleCompile => TargetRenderers[0] != null && DecalTexture != null;

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColoro;
            var Matrix = transform.localToWorldMatrix;

            Gizmos.matrix = Matrix;

            var CenterPos = Vector3.zero;
            Gizmos.DrawWireCube(CenterPos + new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));//基準となる四角形

            if (DecalTexture != null)
            {
                if (DisplayDecalMat == null || Quad == null) GizmInstans();
                DisplayDecalMat.SetPass(0);
                Graphics.DrawMeshNow(Quad, Matrix);
            }
            Gizmos.DrawLine(CenterPos, CenterPos + new Vector3(0, 0, MaxDistans / 2));//前方向の表示
        }

        public void GizmInstans()
        {
            DisplayDecalMat = new Material(Shader.Find("Hidden/DisplayDecalTexture"));
            DisplayDecalMat.mainTexture = DecalTexture;
            Quad = AssetDatabase.LoadAllAssetsAtPath("Library/unity default resources").ToList().Find(i => i.name == "Quad") as Mesh;

        }
    }
}



#endif