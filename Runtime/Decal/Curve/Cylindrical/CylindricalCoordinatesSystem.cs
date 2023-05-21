using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rs64.TexTransTool.Decal.Curve.Cylindrical
{
    public class CylindricalCoordinatesSystem : MonoBehaviour
    {
        public float GizmoRadius = 0.25f;
        public float GizmoHeight = 1f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.black;

            var up = Vector3.up * GizmoHeight;

            Gizmos.DrawWireSphere(Vector3.zero, GizmoRadius);
            Gizmos.DrawLine(Vector3.zero, up);
            Gizmos.DrawWireSphere(up, GizmoRadius);
        }

        public Vector3 GetCCSPoint(Vector3 Point)
        {
            var LoaclaPoint = transform.worldToLocalMatrix.MultiplyPoint(Point);
            var Height = LoaclaPoint.y;
            var Angle = Mathf.Atan2(LoaclaPoint.x, LoaclaPoint.z) * Mathf.Rad2Deg;
            var Distance = new Vector2(LoaclaPoint.x, LoaclaPoint.z).magnitude;

            return new Vector3(Height, Angle, Distance);
        }

        public Vector3 GetWorldPoint(Vector3 CCSPoint)
        {
            var Height = CCSPoint.x;
            var Angle = CCSPoint.y * Mathf.Deg2Rad;
            var Distance = CCSPoint.z;

            var x = Mathf.Sin(Angle) * Distance;
            var z = Mathf.Cos(Angle) * Distance;

            return transform.localToWorldMatrix.MultiplyPoint(new Vector3(x, Height, z));
        }

        public static float OffsetAngle(float angle, float offset)
        {
            var a = angle + offset;
            if (a > 180) a -= 360;
            if (a < -180) a += 360;
            return a;
        }

        public List<Vector3> VartexsConvertCCS(List<Vector3> Vartexs, float OffSetAngle = 0)
        {
            return Vartexs.Select(i =>
            {
                var CCSPoint = GetCCSPoint(i);
                return new Vector3(CCSPoint.x, OffsetAngle(CCSPoint.y, OffSetAngle), CCSPoint.z);
            }
            ).ToList();
        }

        public (List<Vector3>, List<Vector3>) VartexsConvertCCS(List<Vector3> Quad, List<Vector3> Varticals, bool IsAngeleOffset = true)
        {
            var AngleOffset = IsAngeleOffset ? GetCCSPoint(Quad[0]).y : 0;
            var LoaclVarticals = VartexsConvertCCS(Varticals, AngleOffset);
            var LoaclQuad = VartexsConvertCCS(Quad, AngleOffset);
            return (LoaclQuad, LoaclVarticals);
        }
    }
}