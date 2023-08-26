using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.Decal.Curve
{
    public class BezierCurve : ICurve
    {
        public List<CurevSegment> Segments = new List<CurevSegment>();
        public RoolMode RoolMode;

        float _DefaultWightStep = 0.1f;
        public float DefaultWightStep { get => _DefaultWightStep; set => _DefaultWightStep = value > 0 ? value : 0.1f; }
        public BezierCurve(List<CurevSegment> segments, RoolMode roolMode = RoolMode.WorldUp, float defaultWightStep = 0.1f)
        {
            Segments = segments;
            DefaultWightStep = defaultWightStep;
            RoolMode = roolMode;
        }
        public Vector3 GetPoint(float wight)
        {
            var SegmentIndexMax = Segments.Count - 1;
            var MidollIndex = Mathf.RoundToInt(wight);
            if (MidollIndex < 1)
            {
                return Vector3.LerpUnclamped(Segments[0].position, Segments[1].position, wight);
            }
            else if (MidollIndex >= SegmentIndexMax)
            {
                return Vector3.LerpUnclamped(Segments[SegmentIndexMax - 1].position, Segments[SegmentIndexMax].position, wight - (SegmentIndexMax - 1));
            }

            var FromIndex = MidollIndex - 1;
            var ToIndex = MidollIndex + 1;

            var PointPoint = Segments[MidollIndex].position;
            var FromPoint = Vector3.Lerp(Segments[FromIndex].position, PointPoint, 0.5f);
            var ToPoint = Vector3.Lerp(PointPoint, Segments[ToIndex].position, 0.5f);

            var FromWightRenge = FromIndex + 0.5f;
            var ToWightRenge = ToIndex - 0.5f;

            var RelativeWight = (wight - FromWightRenge) / (ToWightRenge - FromWightRenge);
            var GetPoint = CalculatBezier(FromPoint, PointPoint, ToPoint, RelativeWight);

            return GetPoint;

        }
        public (Vector3, float) GetOfLeng(float FormWight, float Lengs)
        {
            var FromPoint = GetPoint(FormWight);

            var Point2 = new (Vector3, float, float)?[2] { null, (FromPoint, 0f, FormWight) };
            var NawWight = FormWight;
            while (true)
            {
                NawWight += DefaultWightStep;
                var NawPoint = GetPoint(NawWight);
                var NawLeng = Vector3.Distance(FromPoint, NawPoint);

                Point2[0] = Point2[1];
                Point2[1] = (NawPoint, NawLeng, NawWight);

                if (NawLeng > Lengs)
                {
                    break;
                }
            }

            var Minv = Point2[0].Value;
            var Maxv = Point2[1].Value;
            var Minleng = Minv.Item2;
            var Maxleng = Maxv.Item2;
            var Minwight = Minv.Item3;
            var Maxwight = Maxv.Item3;

            var wight = (Lengs - Minleng) / (Maxleng - Minleng);
            var reswight = Mathf.LerpUnclamped(Minwight, Maxwight, wight);

            return (GetPoint(reswight), reswight);
        }

        public float GetRool(float wight)
        {
            var SegmentIndexMax = Segments.Count - 1;
            var FloorIndex = Mathf.FloorToInt(wight);
            var CeilIndex = Mathf.CeilToInt(wight);
            if (FloorIndex < 1)
            {
                return Mathf.LerpUnclamped(Segments[0].Rool, Segments[1].Rool, wight);
            }
            else if (CeilIndex > SegmentIndexMax)
            {
                return Mathf.LerpUnclamped(Segments[SegmentIndexMax - 1].Rool, Segments[SegmentIndexMax].Rool, wight - (SegmentIndexMax - 1));
            }
            return Mathf.Lerp(Segments[FloorIndex].Rool, Segments[CeilIndex].Rool, wight - FloorIndex);
        }

        public static Vector3 CalculatBezier(Vector3 From, Vector3 Point, Vector3 To, float wight)
        {
            var fp = Vector3.Lerp(From, Point, wight);
            var pt = Vector3.Lerp(Point, To, wight);

            return Vector3.Lerp(fp, pt, wight);
        }
        public List<List<Vector3>> GetQuad(uint Quad, float Size, float StartWight = 0f)
        {
            if (!Segments.Any()) throw new System.Exception("Segments is null");

            var Quads = new List<List<Vector3>>();
            var FromWight = StartWight;
            var FromPoint = GetPoint(FromWight);
            var FromEdge = GetEdge(FromWight, Size);


            foreach (var Index in Enumerable.Range(0, (int)Quad))
            {
                Vector3 ToPoint; float ToWight; (ToPoint, ToWight) = GetOfLeng(FromWight, Size);

                var ToEdge = GetEdge(ToWight, Size);

                Quads.Add(new List<Vector3>(4) { FromEdge.Item1, FromEdge.Item2, ToEdge.Item1, ToEdge.Item2 });

                FromWight = ToWight;
                FromPoint = ToPoint;
                FromEdge = ToEdge;
            }
            return Quads;
        }

        public (Vector3, Vector3) GetEdge(float wight, float Size)
        {
            var Point = GetPoint(wight);
            var Rool = GetRool(wight);

            switch (RoolMode)
            {
                default:
                case RoolMode.WorldUp:
                    {
                        var foward = GetPoint(wight + DefaultWightStep);
                        var ToLook = Quaternion.FromToRotation(foward, Point);
                        ToLook *= Quaternion.AngleAxis(Rool, Point - foward);
                        var ToEdge = (
                                Point + ToLook * Vector3.left * (Size * 0.5f),
                                Point + ToLook * Vector3.right * (Size * 0.5f)
                            );
                        return ToEdge;
                    }
                case RoolMode.Cross:
                    {
                        var Back = GetPoint(wight - DefaultWightStep);
                        var foward = GetPoint(wight + DefaultWightStep);

                        var RoolAsix = foward - Back;

                        var Clossvec = Vector3.Cross(Back - Point, foward - Point);
                        Clossvec *= Vector3.left.magnitude / Clossvec.magnitude;

                        Clossvec = Quaternion.AngleAxis(Rool, RoolAsix) * Clossvec;

                        Clossvec *= (Size * 0.5f);
                        var Invers = Clossvec * -1;

                        var Left = Point + Clossvec;
                        var Right = Point + Invers;

                        return (Left, Right);
                    }
            }

        }

        public List<Vector3> GetLine(float StartWight, float Endwight)
        {
            var NawWight = StartWight;
            var Line = new List<Vector3>();

            while (NawWight < Endwight)
            {
                Line.Add(GetPoint(NawWight));
                NawWight += DefaultWightStep;
            }

            return Line;
        }

        public List<Vector3> GetLine()
        {
            return GetLine(0f, Segments.Count - 1f);
        }
    }

    public interface ICurve
    {
        Vector3 GetPoint(float wight);
        float GetRool(float wight);
        (Vector3, float) GetOfLeng(float FormWight, float Lengs);
    }

    public enum RoolMode
    {
        WorldUp,
        Cross,

    }
}