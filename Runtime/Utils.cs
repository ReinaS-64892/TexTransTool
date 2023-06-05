#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rs64.TexTransTool.Decal;
using UnityEngine;
namespace Rs64.TexTransTool
{
    public static class Utils
    {
        public static void ForEach2D(Vector2Int Reange, Action<int, int> action)
        {
            int countx = 0;
            int county = 0;
            while (true)
            {
                if (!(Reange.x > countx))
                {
                    countx = 0;
                    county += 1;
                }
                if (!(Reange.y > county))
                {
                    break;
                }

                action.Invoke(countx, county);

                countx += 1;
            }
        }
        public static List<Vector2Int> Reange2d(Vector2Int Reange)
        {
            var List = new List<Vector2Int>();
            ForEach2D(Reange, (x, y) => List.Add(new Vector2Int(x, y)));
            return List;
        }

        public static int TwoDToOneDIndex(Vector2Int TowDIndex, int Size)
        {
            return (TowDIndex.y * Size) + TowDIndex.x;
        }
        public static Vector2Int OneDToTwoDIndex(int TowDIndex, int withLengs)
        {
            return new Vector2Int(TowDIndex % withLengs, TowDIndex / withLengs);
        }
        public static Texture2D CreateFillTexture(int Size, Color FillColor)
        {
            return CreateFillTexture(new Vector2Int(Size, Size), FillColor);
        }
        public static Texture2D CreateFillTexture(Vector2Int Size, Color FillColor)
        {
            var TestTex = new Texture2D(Size.x, Size.y);
            List<Color> Colors = new List<Color>();
            foreach (var count in Enumerable.Range(0, Size.x * Size.y))
            {
                Colors.Add(FillColor);
            }
            TestTex.SetPixels(Colors.ToArray());
            TestTex.Apply();
            return TestTex;
        }
        public static List<TraiangleIndex> ToList(int[] triangleIndexs)
        {
            var TraianglesList = new List<TraiangleIndex>();
            int count = 0;
            while (triangleIndexs.Length > count)
            {
                TraianglesList.Add(new TraiangleIndex(triangleIndexs[count], triangleIndexs[count += 1], triangleIndexs[count += 1]));
                count += 1;
            }
            return TraianglesList;
        }

        public static T[] TowDtoOneD<T>(T[,] SouseArry, Vector2Int Size)
        {
            T[] OneDArry = new T[Size.x * Size.y];
            foreach (var Index in Utils.Reange2d(Size))
            {
                OneDArry[TwoDToOneDIndex(Index, Size.x)] = SouseArry[Index.x, Index.y];
            }
            return OneDArry;
        }
        public static T[,] OneDToTowD<T>(T[] SouseArry, Vector2Int Size)
        {
            T[,] TowDArry = new T[Size.x, Size.y];
            int count = -1;
            foreach (var value in SouseArry)
            {
                count += 1;
                var Index = OneDToTwoDIndex(count, Size.x);
                TowDArry[Index.x, Index.y] = value;
            }
            return TowDArry;
        }
        public static List<Material> GetMaterials(IEnumerable<Renderer> Rendres)
        {
            List<Material> MatS = new List<Material>();
            foreach (var Rendera in Rendres)
            {
                MatS.AddRange(Rendera.sharedMaterials);
            }
            return MatS;
        }
        public static void SetMaterials(IEnumerable<Renderer> Rendres, List<Material> Mat)
        {
            int StartOffset = 0;
            foreach (var Rendera in Rendres)
            {
                int TakeLengs = Rendera.sharedMaterials.Length;
                Rendera.sharedMaterials = Mat.Skip(StartOffset).Take(TakeLengs).ToArray();
                StartOffset += TakeLengs;
            }
        }
        public static List<Mesh> GetMeshes(List<Renderer> renderers,bool NullInsertion = false)
        {
            List<Mesh> Meshs = new List<Mesh>();
            foreach (var Rendera in renderers)
            {
                Mesh mesh = null;
                switch (Rendera)
                {
                    case SkinnedMeshRenderer SMR:
                        {
                            mesh = SMR.sharedMesh;
                            break;
                        }
                    case MeshRenderer MR:
                        {
                            mesh = MR.GetComponent<MeshFilter>().sharedMesh;
                            break;
                        }
                    default:
                        continue;
                }
                if (mesh != null || NullInsertion) Meshs.Add(mesh);
            }
            return Meshs;
        }

        public static void SetMeshs(List<Renderer> renderers, List<Mesh> DistMesh, List<Mesh> SetMesh)
        {
            foreach (var Rendera in renderers)
            {
                switch (Rendera)
                {
                    case SkinnedMeshRenderer SMR:
                        {
                            if (DistMesh.Contains(SMR.sharedMesh))
                            {
                                SMR.sharedMesh = SetMesh[DistMesh.IndexOf(SMR.sharedMesh)];
                            }
                            break;
                        }
                    case MeshRenderer MR:
                        {
                            var MF = MR.GetComponent<MeshFilter>();
                            if (DistMesh.Contains(MF.sharedMesh))
                            {
                                MF.sharedMesh = SetMesh[DistMesh.IndexOf(MF.sharedMesh)];
                            }
                            break;
                        }
                    default:
                        continue;
                }
            }
        }

        public static List<Vector3> ZipListVector3(List<Vector2> XY, List<float> Z)
        {
            var Count = XY.Count;
            if (Count != Z.Count) { throw new System.ArgumentException("XY.Count != Z.Count"); }

            List<Vector3> Result = new List<Vector3>(Count);

            foreach (var Index in Enumerable.Range(0, Count))
            {
                Result.Add(new Vector3(XY[Index].x, XY[Index].y, Z[Index]));
            }

            return Result;
        }

        public static Dictionary<T, List<T2>> ZipToDictionaryOnList<T, T2>(Dictionary<T, List<T2>> Souse, Dictionary<T, List<T2>> Add)
        {
            var Result = new Dictionary<T, List<T2>>(Souse);
            foreach (var Key in Add.Keys)
            {
                if (Result.ContainsKey(Key))
                {
                    Result[Key].AddRange(Add[Key]);
                }
                else
                {
                    Result.Add(Key, Add[Key]);
                }
            }
            return Result;
        }

        public static Dictionary<T, List<T2>> ZipToDictionaryOnList<T, T2>(List<Dictionary<T, List<T2>>> Target)
        {
            var Result = new Dictionary<T, List<T2>>();
            foreach (var Add in Target)
            {
                foreach (var Key in Add.Keys)
                {
                    if (Result.ContainsKey(Key))
                    {
                        Result[Key].AddRange(Add[Key]);
                    }
                    else
                    {
                        Result.Add(Key, Add[Key]);
                    }
                }
            }
            return Result;
        }

        public static List<Texture2D> GeneratTexturesList(List<Material> SouseMaterials, Dictionary<Material, Texture2D> MatAndTexs)
        {
            List<Texture2D> Result = new List<Texture2D>();
            foreach (var Mat in SouseMaterials)
            {
                if (MatAndTexs.ContainsKey(Mat))
                {
                    Result.Add(MatAndTexs[Mat]);
                }
                else
                {
                    Result.Add(null);
                }
            }
            return Result;
        }

        public static Dictionary<T, T2> GeneretFromKvP<T, T2>(List<KeyValuePair<T, T2>> KvPList)
        {
            Dictionary<T, T2> Result = new Dictionary<T, T2>();
            foreach (var KvP in KvPList)
            {
                Result.Add(KvP.Key, KvP.Value);
            }
            return Result;
        }

    }
    public static class GizmosUtility
    {
        public static void DrowGizmoQuad(List<List<Vector3>> Quads)
        {
            foreach (var Quad in Quads)
            {
                Gizmos.DrawLine(Quad[0], Quad[1]);
                Gizmos.DrawLine(Quad[0], Quad[2]);
                Gizmos.DrawLine(Quad[2], Quad[3]);
                Gizmos.DrawLine(Quad[1], Quad[3]);
            }
        }

        public static void DrowGimzLine(List<Vector3> Line)
        {
            var LineCount = Line.Count;
            if (LineCount < 1) return;
            int Count = 1;
            while (LineCount > Count)
            {

                var FromPos = Line[Count - 1];
                var ToPos = Line[Count];
                Gizmos.DrawLine(FromPos, ToPos);

                Count += 1;

            }
        }
    }
}
#endif