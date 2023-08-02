#if UNITY_EDITOR
using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Rs64.TexTransTool.Decal
{
    public static class DecalUtil
    {
        public interface IConvertSpace
        {
            void Input(MeshDatas meshDatas);
            List<Vector2> OutPutUV();
        }
        public interface ITraiangleFilter<SpaseConverter>
        {
            List<TraiangleIndex> Filtering(SpaseConverter Spase, List<TraiangleIndex> Traiangeles);
        }
        public class MeshDatas
        {
            public IReadOnlyList<Vector3> Varticals;
            public IReadOnlyList<Vector2> UV;
            public IReadOnlyList<IReadOnlyList<TraiangleIndex>> TraiangelsSubMesh;

            public MeshDatas(List<Vector3> varticals, List<Vector2> uV, List<List<TraiangleIndex>> traiangelsSubMesh)
            {
                Varticals = varticals;
                UV = uV;
                TraiangelsSubMesh = traiangelsSubMesh.Cast<IReadOnlyList<TraiangleIndex>>().ToList();
            }
        }
        public static Dictionary<KeyTexture, RenderTexture> CreatDecalTexture<KeyTexture,SpaseConverter>(
            Renderer TargetRenderer,
            Dictionary<KeyTexture, RenderTexture> RenderTextures,
            Texture2D SousTextures,
            SpaseConverter ConvertSpase,
            ITraiangleFilter<SpaseConverter> Filter,
            string TargetProptyeName = "_MainTex",
            Vector2? TextureOutRenge = null,
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
                    TextureOutRenge
                );


            }

            return RenderTextures;
        }
        public static Dictionary<Texture2D, List<Texture2D>> CreatDecalTextureCS<SpaseConverter>(
            Renderer TargetRenderer,
            Texture2D SousTextures,
            SpaseConverter ConvertSpase,
            ITraiangleFilter<SpaseConverter> Filter,
            string TargetProptyeName = "_MainTex",
            Vector2? TextureOutRenge = null,
            float DefoaltPading = 1f
        )
        where SpaseConverter : IConvertSpace
        {
            var ResultTexutres = new Dictionary<Texture2D, List<Texture2D>>();

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
                TransTexture.TransTextureUseCS(AtlasTex, SousTextures, new TransTexture.TransUVData(FiltaringdTrainagle, tUV, sUV), DefoaltPading, TextureOutRenge);


                if (ResultTexutres.ContainsKey(TargetTexture) == false) { ResultTexutres.Add(TargetTexture, new List<Texture2D>() { AtlasTex.Texture2D }); }
                else { ResultTexutres[TargetTexture].Add(AtlasTex.Texture2D); }
            }

            return ResultTexutres;
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
                        ConvartVerticesInMatlix(SMR.localToWorldMatrix, Vertices, Vector3.zero);
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
        public static (List<Vector2>, List<List<TraiangleIndex>>) RendererMeshToGetUVAndTariangel(Renderer Target)
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
            List<List<TraiangleIndex>> TraingleIndexs = new List<List<TraiangleIndex>>();
            foreach (var Index in Enumerable.Range(0, Mesh.subMeshCount))
            {
                List<TraiangleIndex> TraingleIndex = Utils.ToList(Mesh.GetTriangles(Index));
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
        [Obsolete]
        public static List<TraiangleIndex> FiltaringTraiangle(
            List<TraiangleIndex> Target, List<Vector3> Vartex,
            float StartDistans = 0, float MaxDistans = 1, float MinRange = 0, float MaxRange = 1, bool SideChek = true
        )
        {
            var FiltalingTraingles = new List<TraiangleIndex>();
            foreach (var Traiangle in Target)
            {
                if (Vartex[Traiangle[0]].z < StartDistans || Vartex[Traiangle[1]].z < StartDistans || Vartex[Traiangle[2]].z < StartDistans)
                {
                    continue;
                }
                if (Vartex[Traiangle[0]].z > MaxDistans && Vartex[Traiangle[1]].z > MaxDistans && Vartex[Traiangle[2]].z > MaxDistans)
                {
                    continue;
                }

                if (SideChek)
                {
                    var ba = Vartex[Traiangle[1]] - Vartex[Traiangle[0]];
                    var ac = Vartex[Traiangle[0]] - Vartex[Traiangle[2]];
                    var TraiangleSide = Vector3.Cross(ba, ac).z;
                    if (TraiangleSide < 0)
                    {
                        continue;
                    }
                }


                bool OutOfPrygon = false;
                foreach (var VIndex in Traiangle)
                {
                    var Tvartex = Vartex[VIndex];
                    if (Tvartex.x < MaxRange && Tvartex.x > MinRange && Tvartex.y < MaxRange && Tvartex.y > MinRange) OutOfPrygon = true;
                }
                if (!OutOfPrygon) continue;


                FiltalingTraingles.Add(Traiangle);
            }
            return FiltalingTraingles;
        }

        public delegate bool Filtaring<InterObject>(TraiangleIndex TargetTri, InterObject Vartex);//対象の三角形を通せない場合True
        public static List<TraiangleIndex> FiltaringTraiangle<InterSpace>(List<TraiangleIndex> Target, InterSpace InterObjects, IReadOnlyList<Filtaring<InterSpace>> Filtars)
        {
            var FiltalingTraingles = new List<TraiangleIndex>(Target.Count);
            foreach (var Traiangle in Target)
            {
                bool Isfiltered = false;
                foreach (var filtar in Filtars)
                {
                    if (filtar.Invoke(Traiangle, InterObjects))
                    {
                        Isfiltered = true;
                        break;
                    }
                }
                if (!Isfiltered)
                {
                    FiltalingTraingles.Add(Traiangle);
                }
            }
            return FiltalingTraingles;
        }

        public static bool SideChek(TraiangleIndex TargetTri, List<Vector3> Vartex)
        { return SideChek(TargetTri, Vartex, false); }
        public static bool SideChek(TraiangleIndex TargetTri, List<Vector3> Vartex, bool IsReverse)
        {
            var ba = Vartex[TargetTri[1]] - Vartex[TargetTri[0]];
            var ac = Vartex[TargetTri[0]] - Vartex[TargetTri[2]];
            var TraiangleSide = Vector3.Cross(ba, ac).z;
            if (!IsReverse) return TraiangleSide < 0;
            else return TraiangleSide > 0;
        }
        public static bool FarClip(TraiangleIndex TargetTri, List<Vector3> Vartex, float Far, bool IsAllVartex)//IsAllVartexは排除されるのにすべてが条件に外れてる場合と一つでも条件に外れてる場合の選択
        {
            if (IsAllVartex)
            {
                return Vartex[TargetTri[0]].z > Far && Vartex[TargetTri[1]].z > Far && Vartex[TargetTri[2]].z > Far;
            }
            else
            {
                return Vartex[TargetTri[0]].z > Far || Vartex[TargetTri[1]].z > Far || Vartex[TargetTri[2]].z > Far;
            }
        }
        public static bool NerClip(TraiangleIndex TargetTri, List<Vector3> Vartex, float Nre, bool IsAllVartex)
        {
            if (IsAllVartex)
            {
                return Vartex[TargetTri[0]].z < Nre && Vartex[TargetTri[1]].z < Nre && Vartex[TargetTri[2]].z < Nre;
            }
            else
            {
                return Vartex[TargetTri[0]].z < Nre || Vartex[TargetTri[1]].z < Nre || Vartex[TargetTri[2]].z < Nre;
            }
        }
        public static bool OutOfPorigonVartexBase(TraiangleIndex TargetTri, List<Vector3> Vartex, float MaxRange, float MinRange, bool IsAllVartex)
        {
            bool[] OutOfPrygon = new bool[3] { false, false, false };
            foreach (var Index in Enumerable.Range(0, 3))
            {

                var Tvartex = Vartex[TargetTri[Index]];
                OutOfPrygon[Index] = !(Tvartex.x < MaxRange && Tvartex.x > MinRange && Tvartex.y < MaxRange && Tvartex.y > MinRange);
            }
            if (IsAllVartex) return OutOfPrygon[0] && OutOfPrygon[1] && OutOfPrygon[2];
            else return OutOfPrygon[0] || OutOfPrygon[1] || OutOfPrygon[2];
        }
        public static bool OutOfPorigonEdgeBase(TraiangleIndex TargetTri, List<Vector3> Vartex, float MaxRange, float MinRange, bool IsAllVartex)
        {
            float CenterPos = (MaxRange + MinRange) / 2;
            Vector2 ConterPos2 = new Vector2(CenterPos, CenterPos);
            bool[] OutOfPrygon = new bool[3] { false, false, false };
            foreach (var Index in new Vector2Int[3] { new Vector2Int(0, 1), new Vector2Int(1, 2), new Vector2Int(2, 1) })
            {

                var a = Vartex[TargetTri[Index.x]];
                var b = Vartex[TargetTri[Index.y]];
                var NerPoint = TransMapper.NeaPointOnLine(a, b, ConterPos2);
                OutOfPrygon[Index.x] = !(NerPoint.x < MaxRange && NerPoint.x > MinRange && NerPoint.y < MaxRange && NerPoint.y > MinRange);
            }
            if (IsAllVartex) return OutOfPrygon[0] && OutOfPrygon[1] && OutOfPrygon[2];
            else return OutOfPrygon[0] || OutOfPrygon[1] || OutOfPrygon[2];
        }
        public static bool OutOfPorigonEdgeEdgeAndCenterRayCast(TraiangleIndex TargetTri, List<Vector3> Vartex, float MaxRange, float MinRange, bool IsAllVartex)
        {
            float CenterPos = (MaxRange + MinRange) / 2;
            Vector2 ConterPos2 = new Vector2(CenterPos, CenterPos);
            if (!OutOfPorigonEdgeBase(TargetTri, Vartex, MaxRange, MinRange, IsAllVartex))
            {
                return false;
            }
            else
            {
                var ClossT = TransMapper.ClossTraiangle(new List<Vector2>(3) { Vartex[TargetTri[0]], Vartex[TargetTri[1]], Vartex[TargetTri[2]] }, ConterPos2);
                return TransMapper.IsInCal(ClossT.x, ClossT.y, ClossT.z);
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
    public enum PolygonCaling
    {
        Vartex,
        Edge,
        EdgeAndCenterRay,
    }
}
#endif