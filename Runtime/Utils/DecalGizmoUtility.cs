using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool.Utils
{
    public static class DecalGizmoUtility
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

        public static void DrawGizmoQuad(Texture2D texture2D, Color mulColor, Matrix4x4 matrix4X4)
        {
            if (Quad == null || DisplayDecalMat == null) { DecalGizmoUtilityInit(); }
            DisplayDecalMat.SetTexture("_MainTex", texture2D);
            DisplayDecalMat.SetColor("_MulColor", mulColor);
            DisplayDecalMat.SetPass(0);
            Graphics.DrawMeshNow(Quad, matrix4X4);
        }
    }
}
