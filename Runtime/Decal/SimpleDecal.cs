#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rs64.TexTransTool.Decal
{
    public class SimpleDecal : TextureTransformer
    {
        public Renderer TargetRenderer;
        public Texture2D DecalTexture;
        public float Scale;
        public float MaxDistans;

        [SerializeField] string TargetPropatyName = "_MainTex";
        public void ScaleAppry()
        {
            if (DecalTexture != null)
            {
                transform.localScale = new Vector3(Scale, Scale * (DecalTexture.width / DecalTexture.height), MaxDistans);
            }
            else
            {
                transform.localScale = new Vector3(Scale, Scale, MaxDistans);
            }
        }

        [SerializeField] List<Texture2D> CompiledTextures;
        [SerializeField] Material[] BackUpMaterials;
        [SerializeField] Material[] EditMaterialsSave;
        [SerializeField] Texture2D[] PileTexteres;

        public override void Compile()
        {
            var ResultTexutres = DecalUtil.CreatDecalTexture(TargetRenderer, DecalTexture, transform.worldToLocalMatrix);
            AssetSaveHelper.DeletAssets(CompiledTextures);
            CompiledTextures = AssetSaveHelper.SaveAssets(ResultTexutres);
        }
        public override void Appry()
        {
            if (_IsAppry) return;
            _IsAppry = true;

            var Materials = TargetRenderer.sharedMaterials;
            var EditMaterials = new Material[Materials.Length];
            var NewPileTexteres = new Texture2D[Materials.Length];

            foreach (var Index in Enumerable.Range(0, Materials.Length))
            {
                var EditableMaterial = Instantiate(Materials[Index]);
                if (CompiledTextures[Index] != null && EditableMaterial.GetTexture(TargetPropatyName) is Texture2D BaseTex)
                {
                    var AddTex = CompiledTextures[Index];
                    Compiler.NotFIlterAndReadWritTexture2D(ref BaseTex);
                    Compiler.NotFIlterAndReadWritTexture2D(ref AddTex);
                    var PileTextere = TextureLayerUtil.PileTexture(BaseTex, AddTex, TextureLayerUtil.PileType.Normal).Result;
                    var SavedPiletexure = AssetSaveHelper.SaveAsset(PileTextere);
                    EditableMaterial.SetTexture(TargetPropatyName, SavedPiletexure);
                    NewPileTexteres[Index] = SavedPiletexure;
                }
                EditMaterials[Index] = EditableMaterial;
            }
            EditMaterials = AssetSaveHelper.SaveAssets(EditMaterials).ToArray();
            PileTexteres = NewPileTexteres;
            TargetRenderer.sharedMaterials = EditMaterials;
            EditMaterialsSave = EditMaterials;
            BackUpMaterials = Materials;
        }
        public override void Revart()
        {
            if (!_IsAppry) return;
            _IsAppry = false;

            TargetRenderer.sharedMaterials = BackUpMaterials;
            AssetSaveHelper.DeletAssets(EditMaterialsSave);
            AssetSaveHelper.DeletAssets(PileTexteres);
            EditMaterialsSave = null;
            BackUpMaterials = null;
            PileTexteres = null;

        }



        public Material DisplayDecalMat;
        public Color GizmoColoro = new Color(0, 0, 0, 1);
        public Mesh Quad;

        [SerializeField] bool _IsAppry;
        public override bool IsAppry => _IsAppry;

        public override bool IsPossibleAppry => CompiledTextures.Any();

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColoro;
            var Matrix = transform.localToWorldMatrix;

            Gizmos.matrix = Matrix;

            var CenterPos = Vector3.zero;
            Gizmos.DrawWireCube(CenterPos + new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));//基準となる四角形

            if (DecalTexture != null)
            {
                DisplayDecalMat.SetPass(0);
                Graphics.DrawMeshNow(Quad, Matrix);
            }
            Gizmos.DrawLine(CenterPos, CenterPos + new Vector3(0, 0, MaxDistans / 2));//前方向の表示
        }
    }
}



#endif