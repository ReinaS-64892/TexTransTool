using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if VRC_BASE
using VRC.SDKBase;
#endif
namespace Rs64.TexTransTool.Decal.Curve.Cylindrical
{
    [AddComponentMenu("TexTransTool/Experimental/CylindricalCoordinatesSystem")]
    public class CylindricalCoordinatesSystem : MonoBehaviour
#if VRC_BASE
    , IEditorOnly
#endif
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
            float AngleOffset;
            if (IsAngeleOffset)
            {
                var NotOffsetQuad = VartexsConvertCCS(Quad, 0);
                var MinValue = NotOffsetQuad[0].y;
                NotOffsetQuad.ForEach(i => MinValue = Mathf.Max(MinValue, i.y));
                AngleOffset = MinValue * -1;
            }
            else
            {
                AngleOffset = 0;
            }

            var LoaclVarticals = VartexsConvertCCS(Varticals, AngleOffset);
            var LoaclQuad = VartexsConvertCCS(Quad, AngleOffset);
            //LoaclQuad.ForEach(i => Debug.Log( i.x + " " + i.y + " " + i.z) );
            return (LoaclQuad, LoaclVarticals);
        }
    }
}