using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    public static class GizmosUtility
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
}