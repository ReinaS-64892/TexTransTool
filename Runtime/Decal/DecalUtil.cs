#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Rs64.TexTransTool.Decal
{
    public static class DecalUtil
    {
        public static List<Texture2D> CreatDecalTexture(Renderer TargetRenderer, Texture2D SousTextures, Matrix4x4 SouseMatrix, string TargetProptyeName = "_MainTex", string AtlasMapperPath = null)
        {
            if (AtlasMapperPath == null) AtlasMapperPath = TransMapper.AtlasMapperPath;
            List<Texture2D> ResultTexutres = new List<Texture2D>();

            var Vraticals = GetWorldSpeasVertices(TargetRenderer);
            List<Vector2> tUV; List<List<TraiangleIndex>> TraiangelsSubMesh; (tUV, TraiangelsSubMesh) = RendererMeshToGetUVAndTariangel(TargetRenderer);

            var LoaclVarticals = ConvartVerticesInMatlix(SouseMatrix, Vraticals, new Vector3(0.5f, 0.5f, 0));
            var sUV = LoaclVarticals.ConvertAll<Vector2>(i => i);

            var CS = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(AtlasMapperPath);
            var Materials = TargetRenderer.sharedMaterials;

            int SubMeshCount = -1;
            foreach (var Traiangel in TraiangelsSubMesh)
            {
                SubMeshCount += 1;
                var TargetTexture = Materials[SubMeshCount].GetTexture(TargetProptyeName) as Texture2D;
                var FiltaringdTrainagle = FiltaringTraiangle(Traiangel, LoaclVarticals);
                if (TargetTexture == null || FiltaringdTrainagle.Any() == false)
                {
                    ResultTexutres.Add(null);
                    break;
                }

                var TargetTexSize = new Vector2Int(TargetTexture.width, TargetTexture.height);
                var Map = new TransMapData(-1, TargetTexSize);
                var TargetScaileTargetUV = TransMapper.UVtoTexScale(tUV, TargetTexSize);
                Map = TransMapper.UVMappingTableGeneratorComputeShederUsed(CS, Map, FiltaringdTrainagle, TargetScaileTargetUV, sUV);
                var AtlasTex = new TransTargetTexture(Utils.CreateFillTexture(new Vector2Int(TargetTexture.width, TargetTexture.height), new Color(0, 0, 0, 0)), -1);
                AtlasTex = Compiler.TransCompileUseGetPixsel(SousTextures, Map, AtlasTex);
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
                        break;
                    }
                case MeshRenderer MR:
                    {
                        MR.GetComponent<MeshFilter>().mesh.GetVertices(Vertices);
                        Vertices = ConvartVerticesInMatlix(MR.localToWorldMatrix, Vertices, Vector3.zero);
                        break;
                    }
                default:
                    {
                        throw new System.ArgumentException("Rendererが対応したタイプではありません。");
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
                    //Debug.Log(Tvartex);
                    if (Tvartex.x < MaxRange && Tvartex.x > MinRange && Tvartex.y < MaxRange && Tvartex.y > MinRange) OutOfPrygon = true;
                }
                if (!OutOfPrygon) continue;


                FiltalingTraingles.Add(Traiangle);
            }
            return FiltalingTraingles;
        }
    }
}
#endif