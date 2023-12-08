#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    internal static class GizmosUtility
    {
        public static void DrawGizmoQuad(IEnumerable<List<Vector3>> Quads)
        {
            foreach (var Quad in Quads)
            {
                DrawQuad(Quad);
            }
        }

        public static void DrawQuad(IReadOnlyList<Vector3> Quad)
        {
            Gizmos.DrawLine(Quad[0], Quad[1]);
            Gizmos.DrawLine(Quad[0], Quad[2]);
            Gizmos.DrawLine(Quad[2], Quad[3]);
            Gizmos.DrawLine(Quad[1], Quad[3]);
        }

        public static void DrawGizmoLine(List<Vector3> Line)
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
        public static List<Transform> GetChildren(this Transform Parent)
        {
            var list = new List<Transform>();
            foreach (Transform child in Parent)
            {
                list.Add(child);
            }
            return list;
        }



    }
    public class DecalGizmoUtility
    {
        public static Mesh Quad;
        public static Material DisplayDecalMat;
        public static void DecalGizmoUtilityInit()
        {
            Quad = new Mesh
            {
                vertices = new Vector3[]{
                new Vector3(-0.5f,-0.5f),
                new Vector3(-0.5f,0.5f),
                new Vector3(0.5f,-0.5f),
                new Vector3(0.5f,0.5f),
            },
                triangles = new int[]{
                0,1,2,
                2,1,3
            },
                uv = new Vector2[]{
                new Vector3(0,0),
                new Vector3(0,1),
                new Vector3(1,0),
                new Vector3(1,1),
                }
            };
            DisplayDecalMat = new Material(Shader.Find("Hidden/DisplayDecalTexture"));
        }

        public static void DrawGizmoQuad(Texture2D texture2D, Color MulColor, Matrix4x4 matrix4X4)
        {
            if (Quad == null || DisplayDecalMat == null) { DecalGizmoUtilityInit(); }
            DisplayDecalMat.SetTexture("_MainTex", texture2D);
            DisplayDecalMat.SetColor("_MulColor", MulColor);
            DisplayDecalMat.SetPass(0);
            Graphics.DrawMeshNow(Quad, matrix4X4);
        }
    }
}
#endif
