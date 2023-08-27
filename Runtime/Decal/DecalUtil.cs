#if UNITY_EDITOR
using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool.Decal
{
    public static class DecalUtil
    {
        public interface IConvertSpace
        {
            void Input(MeshDatas meshDatas);
            List<Vector2> OutPutUV();
        }
        public interface ITrianglesFilter<SpaseConverter>
        {
            List<TriangleIndex> Filtering(SpaseConverter Spase, List<TriangleIndex> Traiangeles);
        }
        public class MeshDatas
        {
            public IReadOnlyList<Vector3> Varticals;
            public IReadOnlyList<Vector2> UV;
            public IReadOnlyList<IReadOnlyList<TriangleIndex>> TraiangelsSubMesh;

            public MeshDatas(List<Vector3> varticals, List<Vector2> uV, List<List<TriangleIndex>> traiangelsSubMesh)
            {
                Varticals = varticals;
                UV = uV;
                TraiangelsSubMesh = traiangelsSubMesh.Cast<IReadOnlyList<TriangleIndex>>().ToList();
            }
        }
        public static Dictionary<KeyTexture, RenderTexture> CreatDecalTexture<KeyTexture, SpaseConverter>(
            Renderer TargetRenderer,
            Dictionary<KeyTexture, RenderTexture> RenderTextures,
            Texture SousTextures,
            SpaseConverter ConvertSpase,
            ITrianglesFilter<SpaseConverter> Filter,
            string TargetProptyeName = "_MainTex",
            Vector2? TextureOutRange = null,
            //TexWrapMode TexWrapMode = TexWrapMode.NotWrap,
            float DefoaltPading = 0.5f
        )
        where KeyTexture : Texture
        where SpaseConverter : IConvertSpace
        {
            if (RenderTextures == null) RenderTextures = new Dictionary<KeyTexture, RenderTexture>();

            var Vraticals = GetWorldSpeasVertices(TargetRenderer);
            (var tUV, var TraiangelsSubMesh) = RendererMeshToGetUVAndTariangel(TargetRenderer);

            ConvertSpase.Input(new MeshDatas(Vraticals, tUV, TraiangelsSubMesh));
            var sUV = ConvertSpase.OutPutUV();

            var Materials = TargetRenderer.sharedMaterials;

            for (int i = 0; i < TraiangelsSubMesh.Count; i++)
            {
                var Traiangel = TraiangelsSubMesh[i];
                var TargetMat = Materials[i];

                if (!TargetMat.HasProperty(TargetProptyeName)) { continue; };
                var TargetTexture = TargetMat.GetTexture(TargetProptyeName) as KeyTexture;
                if (TargetTexture == null) { continue; }
                var TargetTexSize = TargetTexture is Texture2D tex2d ? tex2d.NativeSize() : new Vector2Int(TargetTexture.width, TargetTexture.height);

                var FiltaringdTrainagle = Filter != null ? Filter.Filtering(ConvertSpase, Traiangel) : Traiangel;
                if (FiltaringdTrainagle.Any() == false) { continue; }




                if (!RenderTextures.ContainsKey(TargetTexture))
                {
                    var RendererTexture = new RenderTexture(TargetTexSize.x, TargetTexSize.y, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
                    RenderTextures.Add(TargetTexture, RendererTexture);
                }

                TransTexture.TransTextureToRenderTexture(
                    RenderTextures[TargetTexture],
                    SousTextures,
                    new TransTexture.TransUVData(FiltaringdTrainagle, tUV, sUV),
                    DefoaltPading,
                    TextureOutRange
                );


            }

            return RenderTextures;
        }
        public static Dictionary<Texture2D, List<Texture2D>> CreatDecalTextureCS<SpaseConverter>(
            Renderer TargetRenderer,
            Texture2D SousTextures,
            SpaseConverter ConvertSpase,
            ITrianglesFilter<SpaseConverter> Filter,
            string TargetProptyeName = "_MainTex",
            Vector2? TextureOutRange = null,
            float DefoaltPading = 1f
        )
        where SpaseConverter : IConvertSpace
        {
            var ResultTextures = new Dictionary<Texture2D, List<Texture2D>>();

            var Vraticals = GetWorldSpeasVertices(TargetRenderer);
            (var tUV, var TraiangelsSubMesh) = RendererMeshToGetUVAndTariangel(TargetRenderer);

            ConvertSpase.Input(new MeshDatas(Vraticals, tUV, TraiangelsSubMesh));
            var sUV = ConvertSpase.OutPutUV();

            var Materials = TargetRenderer.sharedMaterials;

            for (int i = 0; i < TraiangelsSubMesh.Count; i++)
            {
                var Traiangel = TraiangelsSubMesh[i];
                var TargetMat = Materials[i];

                var TargetTexture = TargetMat.GetTexture(TargetProptyeName) as Texture2D;
                if (TargetTexture == null) { continue; }
                var TargetTexSize = TargetTexture.NativeSize();

                var FiltaringdTrainagle = Filter != null ? Filter.Filtering(ConvertSpase, Traiangel) : Traiangel;
                if (FiltaringdTrainagle.Any() == false) { continue; }


                var AtlasTex = new TransTargetTexture(Utils.CreateFillTexture(TargetTexSize, new Color(0, 0, 0, 0)), new TowDMap<float>(TransTexture.CSPading(DefoaltPading), TargetTexSize));
                TransTexture.TransTextureUseCS(AtlasTex, SousTextures, new TransTexture.TransUVData(FiltaringdTrainagle, tUV, sUV), DefoaltPading, TextureOutRange);


                if (ResultTextures.ContainsKey(TargetTexture) == false) { ResultTextures.Add(TargetTexture, new List<Texture2D>() { AtlasTex.Texture2D }); }
                else { ResultTextures[TargetTexture].Add(AtlasTex.Texture2D); }
            }

            return ResultTextures;
        }
        public static List<Vector3> GetWorldSpeasVertices(Renderer Target)
        {
            List<Vector3> Vertices = new List<Vector3>();
            switch (Target)
            {
                case SkinnedMeshRenderer SMR:
                    {
                        Mesh Mesh = new Mesh();
                        SMR.BakeMesh(Mesh);
                        Mesh.GetVertices(Vertices);
                        Matrix4x4 matlix;
                        if (SMR.bones.Any())
                        {
                            matlix = Matrix4x4.TRS(SMR.transform.position, SMR.transform.rotation, Vector3.one);
                        }
                        else if (SMR.rootBone == null)
                        {
                            matlix = SMR.localToWorldMatrix;
                        }
                        else
                        {
                            matlix = SMR.rootBone.localToWorldMatrix;
                        }
                        ConvartVerticesInMatlix(matlix, Vertices, Vector3.zero);
                        break;
                    }
                case MeshRenderer MR:
                    {
                        MR.GetComponent<MeshFilter>().sharedMesh.GetVertices(Vertices);
                        ConvartVerticesInMatlix(MR.localToWorldMatrix, Vertices, Vector3.zero);
                        break;
                    }
                default:
                    {
                        throw new System.ArgumentException("Rendererが対応したタイプではないか、TargetRendererが存在しません。");
                    }
            }
            return Vertices;
        }
        public static (List<Vector2>, List<List<TriangleIndex>>) RendererMeshToGetUVAndTariangel(Renderer Target)
        {
            Mesh Mesh;
            switch (Target)
            {
                case SkinnedMeshRenderer SMR:
                    {
                        Mesh = SMR.sharedMesh;
                        break;
                    }
                case MeshRenderer MR:
                    {
                        Mesh = MR.GetComponent<MeshFilter>().sharedMesh;
                        break;
                    }
                default:
                    {
                        throw new System.ArgumentException("Rendererが対応したタイプではありません。");
                    }
            }
            List<Vector2> UV = new List<Vector2>();
            Mesh.GetUVs(0, UV);
            List<List<TriangleIndex>> TraingleIndexs = new List<List<TriangleIndex>>();
            foreach (var Index in Enumerable.Range(0, Mesh.subMeshCount))
            {
                List<TriangleIndex> TraingleIndex = Utils.ToList(Mesh.GetTriangles(Index));
                TraingleIndexs.Add(TraingleIndex);
            }
            return (UV, TraingleIndexs);
        }
        public static List<Vector3> ConvartVerticesInMatlix(Matrix4x4 matrix, IEnumerable<Vector3> Vertices, Vector3 Offset)
        {
            var ConvertVertices = new List<Vector3>();
            foreach (var Vertice in Vertices)
            {
                var Pos = matrix.MultiplyPoint3x4(Vertice) + Offset;
                ConvertVertices.Add(Pos);
            }
            return ConvertVertices;
        }
        public static void ConvartVerticesInMatlix(Matrix4x4 matrix, List<Vector3> Vertices, Vector3 Offset)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = matrix.MultiplyPoint3x4(Vertices[i]) + Offset;
            }
        }
        public static Vector2 QuadNormaliz(IReadOnlyList<Vector2> Quad, Vector2 TargetPos)
        {
            var OneNeaPoint = TransMapper.NeaPoint(Quad[0], Quad[2], TargetPos);
            var OneCross = Vector3.Cross(Quad[2] - Quad[0], TargetPos - Quad[0]).z > 0 ? -1 : 1;

            var TwoNeaPoint = TransMapper.NeaPoint(Quad[0], Quad[1], TargetPos);
            var TwoCross = Vector3.Cross(Quad[1] - Quad[0], TargetPos - Quad[0]).z > 0 ? 1 : -1;

            var ThreeNeaPoint = TransMapper.NeaPoint(Quad[1], Quad[3], TargetPos);
            var ThreeCross = Vector3.Cross(Quad[3] - Quad[1], TargetPos - Quad[1]).z > 0 ? 1 : -1;

            var ForNeaPoint = TransMapper.NeaPoint(Quad[2], Quad[3], TargetPos);
            var ForCross = Vector3.Cross(Quad[3] - Quad[2], TargetPos - Quad[2]).z > 0 ? -1 : 1;

            var OneDistans = Vector2.Distance(OneNeaPoint, TargetPos) * OneCross;
            var TowDistans = Vector2.Distance(TwoNeaPoint, TargetPos) * TwoCross;
            var ThreeDistnas = Vector2.Distance(ThreeNeaPoint, TargetPos) * ThreeCross;
            var ForDistans = Vector2.Distance(ForNeaPoint, TargetPos) * ForCross;

            var x = OneDistans / (OneDistans + ThreeDistnas);
            var y = TowDistans / (TowDistans + ForDistans);

            return new Vector2(x, y);
        }
        public static List<Vector2> QuadNormaliz(IReadOnlyList<Vector2> Quad, List<Vector2> TargetPoss)
        {
            List<Vector2> NormalizedPos = new List<Vector2>(TargetPoss.Count);
            foreach (var TargetPos in TargetPoss)
            {
                NormalizedPos.Add(QuadNormaliz(Quad, TargetPos));
            }
            return NormalizedPos;
        }
    }

    public enum PolygonCulling
    {
        Vartex,
        Edge,
        EdgeAndCenterRay,
    }
}
#endif
