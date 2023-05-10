#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public static List<Mesh> GetMeshes(List<Renderer> renderers)
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
                        throw new InvalidCastException();
                }
                Meshs.Add(mesh);
            }
            return Meshs;
        }
        public static List<Vector2> GetSubMeshUV(Mesh mesh, int UVchanel, int SubMeshIndex)
        {
            var UV = new List<Vector2>();
            var SubMeshDescliptor = mesh.GetSubMesh(SubMeshIndex);
            mesh.GetUVs(UVchanel, UV);
            return UV.GetRange(SubMeshDescliptor.indexStart, SubMeshDescliptor.indexCount);
        }

    }
}
#endif