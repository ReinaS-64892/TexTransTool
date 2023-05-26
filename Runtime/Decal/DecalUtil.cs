#if UNITY_EDITOR
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
        public delegate List<Vector3> ConvertSpase(List<Vector3> Varticals);
        public static Dictionary<Material, List<Texture2D>> CreatDecalTexture(
            Renderer TargetRenderer,
            Texture2D SousTextures,
            ConvertSpase ConvertSpase,
            string TargetProptyeName = "_MainTex",
            string TransMapperPath = null,
            List<Filtaring> TrainagleFilters = null,
            Vector2? TextureOutRenge = null,
            float DefoaltPading = -1f
        )
        {
            var ResultTexutres = new Dictionary<Material, List<Texture2D>>();

            var Vraticals = GetWorldSpeasVertices(TargetRenderer);
            List<Vector2> tUV; List<List<TraiangleIndex>> TraiangelsSubMesh; (tUV, TraiangelsSubMesh) = RendererMeshToGetUVAndTariangel(TargetRenderer);

            var LoaclVarticals = ConvertSpase.Invoke(Vraticals);
            var sUV = LoaclVarticals.ConvertAll<Vector2>(i => i);

            var CS = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(TransMapperPath);
            var Materials = TargetRenderer.sharedMaterials;

            int SubMeshCount = -1;
            foreach (var Traiangel in TraiangelsSubMesh)
            {
                SubMeshCount += 1;
                var TargetMat = Materials[SubMeshCount];
                var TargetTexture = TargetMat.GetTexture(TargetProptyeName) as Texture2D;
                if (TargetTexture == null) { break; }
                var FiltaringdTrainagle = TrainagleFilters != null ? FiltaringTraiangle(Traiangel, LoaclVarticals, TrainagleFilters) : Traiangel;
                if (FiltaringdTrainagle.Any() == false) { break; }


                var TargetTexSize = new Vector2Int(TargetTexture.width, TargetTexture.height);
                var Map = new TransMapData(DefoaltPading, TargetTexSize);
                var TargetScaileTargetUV = TransMapper.UVtoTexScale(tUV, TargetTexSize);
                Map = TransMapper.TransMapGeneratUseComputeSheder(null, Map, FiltaringdTrainagle, TargetScaileTargetUV, sUV);
                var AtlasTex = new TransTargetTexture(Utils.CreateFillTexture(new Vector2Int(TargetTexture.width, TargetTexture.height), new Color(0, 0, 0, 0)), DefoaltPading);
                AtlasTex = Compiler.TransCompileUseGetPixsel(SousTextures, Map, AtlasTex, TexWrapMode.Stretch, TextureOutRenge);
                AtlasTex.Texture2D.Apply();
                if (ResultTexutres.ContainsKey(TargetMat) == false) { ResultTexutres.Add(TargetMat, new List<Texture2D>() { AtlasTex.Texture2D }); }
                else { ResultTexutres[TargetMat].Add(AtlasTex.Texture2D); }
            }
            return ResultTexutres;
        }

        public static List<Texture2D> CreatDecalTexture(Renderer TargetRenderer, Texture2D SousTextures, Matrix4x4 SouseMatrix, string TargetProptyeName = "_MainTex", string TransMapperPath = null, List<Filtaring> TrainagleFilters = null)
        {
            List<Texture2D> ResultTexutres = new List<Texture2D>();

            var Vraticals = GetWorldSpeasVertices(TargetRenderer);
            List<Vector2> tUV; List<List<TraiangleIndex>> TraiangelsSubMesh; (tUV, TraiangelsSubMesh) = RendererMeshToGetUVAndTariangel(TargetRenderer);

            var LoaclVarticals = ConvartVerticesInMatlix(SouseMatrix, Vraticals, new Vector3(0.5f, 0.5f, 0));
            var sUV = LoaclVarticals.ConvertAll<Vector2>(i => i);

            var CS = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(TransMapperPath);
            var Materials = TargetRenderer.sharedMaterials;

            int SubMeshCount = -1;
            foreach (var Traiangel in TraiangelsSubMesh)
            {
                SubMeshCount += 1;
                var TargetTexture = Materials[SubMeshCount].GetTexture(TargetProptyeName) as Texture2D;
                if (TargetTexture == null)
                {
                    ResultTexutres.Add(null);
                    break;
                }
                var FiltaringdTrainagle = TrainagleFilters != null ? FiltaringTraiangle(Traiangel, LoaclVarticals, TrainagleFilters) : Traiangel;

                if (FiltaringdTrainagle.Any() == false)
                {
                    ResultTexutres.Add(null);
                    break;
                }

                var TargetTexSize = new Vector2Int(TargetTexture.width, TargetTexture.height);
                var Map = new TransMapData(-1, TargetTexSize);
                var TargetScaileTargetUV = TransMapper.UVtoTexScale(tUV, TargetTexSize);
                Map = TransMapper.TransMapGeneratUseComputeSheder(null, Map, FiltaringdTrainagle, TargetScaileTargetUV, sUV);
                var AtlasTex = new TransTargetTexture(Utils.CreateFillTexture(new Vector2Int(TargetTexture.width, TargetTexture.height), new Color(0, 0, 0, 0)), -1);
                AtlasTex = Compiler.TransCompileUseGetPixsel(SousTextures, Map, AtlasTex, TexWrapMode.NotWrap);
                AtlasTex.Texture2D.Apply();
                ResultTexutres.Add(AtlasTex.Texture2D);
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
                        Vertices = ConvartVerticesInMatlix(SMR.localToWorldMatrix, Vertices, Vector3.zero);
                        break;
                    }
                case MeshRenderer MR:
                    {
                        MR.GetComponent<MeshFilter>().sharedMesh.GetVertices(Vertices);
                        Vertices = ConvartVerticesInMatlix(MR.localToWorldMatrix, Vertices, Vector3.zero);
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
        public static List<Vector3> ConvartVerticesInMatlix(Matrix4x4 matrix, List<Vector3> Vertices, Vector3 Offset)
        {
            var ConvertVertices = new List<Vector3>();
            foreach (var Vertice in Vertices)
            {
                var Pos = matrix.MultiplyPoint3x4(Vertice) + Offset;
                ConvertVertices.Add(Pos);
            }
            return ConvertVertices;
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

        public delegate bool Filtaring(TraiangleIndex TargetTri, List<Vector3> Vartex);//対象の三角形を通せない場合True
        public static List<TraiangleIndex> FiltaringTraiangle(List<TraiangleIndex> Target, List<Vector3> Vartex, List<Filtaring> Filtars)
        {
            var FiltalingTraingles = new List<TraiangleIndex>(Target.Count);
            foreach (var Traiangle in Target)
            {
                bool Isfiltered = false;
                foreach (var filtar in Filtars)
                {
                    if (filtar.Invoke(Traiangle, Vartex))
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

        public static Vector2 QuadNormaliz(List<Vector2> Quad, Vector2 TargetPos)
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
        public static List<Vector2> QuadNormaliz(List<Vector2> Quad, List<Vector2> TargetPoss)
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