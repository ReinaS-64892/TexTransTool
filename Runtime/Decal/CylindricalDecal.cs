#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using Rs64.TexTransTool.Decal.Cylindrical;

namespace Rs64.TexTransTool.Decal
{
    [AddComponentMenu("TexTransTool/CylindricalDecal")]
    public class CylindricalDecal : AbstractDecal
    {
        public CylindricalCoordinatesSystem cylindricalCoordinatesSystem;
        public bool FixedAspect = true;
        public Vector2 Scale = Vector2.one;
        public override void Compile()
        {
            if (_IsApply) return;
            if (!IsPossibleCompile) return;

            var DictCompiledTextures = new List<Dictionary<Material, List<Texture2D>>>();
            var PPSSpase = new CCSSpace(cylindricalCoordinatesSystem, GetQuad());
            var PPSFilter = new CCSFilter();


            TargetRenderers.ForEach(i => DictCompiledTextures.Add(DecalUtil.CreatDecalTexture(
                                                i,
                                                DecalTexture,
                                                PPSSpase,
                                                PPSFilter,
                                                TargetPropatyName
                                                )
                                        ));

            var MatTexDict = ZipAndBlendTextures(DictCompiledTextures);
            var TextureList = Utils.GeneratTexturesList(Utils.GetMaterials(TargetRenderers), MatTexDict);
            TextureList.ForEach(Tex => { if (Tex != null) Tex.name = "DecalTexture"; });
            SetContainer(TextureList);
        }

        public static readonly Vector3[] LocalQuad = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
        };

        public List<Vector3> GetQuad()
        {
            var Matrix = transform.localToWorldMatrix;
            var WorldSpaseQuad = new List<Vector3>(4);
            foreach (var i in LocalQuad)
            {
                WorldSpaseQuad.Add(Matrix.MultiplyPoint(i));
            }
            return WorldSpaseQuad;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.black;
            var Matrix = transform.localToWorldMatrix;
            Gizmos.matrix = Matrix;

            var CenterPos = Vector3.zero;

            GizmosUtility.DrowQuad(LocalQuad);




        }

        public override void ScaleApply()
        {
            ScaleApply(new Vector3(Scale.x, Scale.y, 1), FixedAspect);
        }
    }
}
#endif
