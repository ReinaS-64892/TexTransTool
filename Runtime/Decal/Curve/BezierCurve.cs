using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.Decal.Curve
{
    internal class BezierCurve : ICurve
    {
        public List<CurveSegment> Segments = new List<CurveSegment>();
        public RollMode RollMode;

        float _DefaultWightStep = 0.1f;
        public float DefaultWightStep { get => _DefaultWightStep; set => _DefaultWightStep = value > 0 ? value : 0.1f; }
        public BezierCurve(List<CurveSegment> segments, RollMode rollMode = RollMode.WorldUp, float defaultWightStep = 0.1f)
        {
            Segments = segments;
            DefaultWightStep = defaultWightStep;
            RollMode = rollMode;
        }
        public Vector3 GetPoint(float wight)
        {
            var segmentIndexMax = Segments.Count - 1;
            var middleIndex = Mathf.RoundToInt(wight);
            if (middleIndex < 1)
            {
                return Vector3.LerpUnclamped(Segments[0].position, Segments[1].position, wight);
            }
            else if (middleIndex >= segmentIndexMax)
            {
                return Vector3.LerpUnclamped(Segments[segmentIndexMax - 1].position, Segments[segmentIndexMax].position, wight - (segmentIndexMax - 1));
            }

            var fromIndex = middleIndex - 1;
            var toIndex = middleIndex + 1;

            var pointPoint = Segments[middleIndex].position;
            var fromPoint = Vector3.Lerp(Segments[fromIndex].position, pointPoint, 0.5f);
            var toPoint = Vector3.Lerp(pointPoint, Segments[toIndex].position, 0.5f);

            var fromWightRange = fromIndex + 0.5f;
            var toWightRange = toIndex - 0.5f;

            var relativeWight = (wight - fromWightRange) / (toWightRange - fromWightRange);
            var getPoint = CalculateBezier(fromPoint, pointPoint, toPoint, relativeWight);

            return getPoint;

        }
        public (Vector3 point, float weight) GetOfLength(float formWight, float length)
        {
            var fromPoint = GetPoint(formWight);

            Span<(Vector3, float, float)?> point2 = stackalloc (Vector3, float, float)?[2] { null, (fromPoint, 0f, formWight) };
            var nawWight = formWight;

            var safetyCount = 0;
            while (safetyCount < 512)
            {
                nawWight += DefaultWightStep;
                var nawPoint = GetPoint(nawWight);
                var nawLength = Vector3.Distance(fromPoint, nawPoint);

                point2[0] = point2[1];
                point2[1] = (nawPoint, nawLength, nawWight);

                if (nawLength > length)
                {
                    break;
                }

                safetyCount += 1;
            }

            var minV = point2[0].Value;
            var maxV = point2[1].Value;
            var minLength = minV.Item2;
            var maxLength = maxV.Item2;
            var minWight = minV.Item3;
            var maxWight = maxV.Item3;

            var wight = (length - minLength) / (maxLength - minLength);
            var resWight = Mathf.LerpUnclamped(minWight, maxWight, wight);

            return (GetPoint(resWight), resWight);
        }

        public float GetRoll(float wight)
        {
            var segmentIndexMax = Segments.Count - 1;
            var floorIndex = Mathf.FloorToInt(wight);
            var ceilIndex = Mathf.CeilToInt(wight);
            if (floorIndex < 1)
            {
                return Mathf.LerpUnclamped(Segments[0].Roll, Segments[1].Roll, wight);
            }
            else if (ceilIndex > segmentIndexMax)
            {
                return Mathf.LerpUnclamped(Segments[segmentIndexMax - 1].Roll, Segments[segmentIndexMax].Roll, wight - (segmentIndexMax - 1));
            }
            return Mathf.Lerp(Segments[floorIndex].Roll, Segments[ceilIndex].Roll, wight - floorIndex);
        }

        public static Vector3 CalculateBezier(Vector3 From, Vector3 Point, Vector3 To, float wight)
        {
            var fp = Vector3.Lerp(From, Point, wight);
            var pt = Vector3.Lerp(Point, To, wight);

            return Vector3.Lerp(fp, pt, wight);
        }
        public List<List<Vector3>> GetQuad(uint Quad, float Size, float StartWight = 0f)
        {
            if (!Segments.Any()) throw new System.Exception("Segments is null");

            var quads = new List<List<Vector3>>();
            var fromWight = StartWight;
            // var fromPoint = GetPoint(fromWight);
            var fromEdge = GetEdge(fromWight, Size);


            foreach (var index in Enumerable.Range(0, (int)Quad))
            {
                Vector3 toPoint; float toWight; (toPoint, toWight) = GetOfLength(fromWight, Size);

                var toEdge = GetEdge(toWight, Size);

                quads.Add(new List<Vector3>(4) { fromEdge.Item1, fromEdge.Item2, toEdge.Item1, toEdge.Item2 });

                fromWight = toWight;
                // fromPoint = ToPoint;
                fromEdge = toEdge;
            }
            return quads;
        }

        public (Vector3, Vector3) GetEdge(float wight, float Size)
        {
            var point = GetPoint(wight);
            var roll = GetRoll(wight);

            switch (RollMode)
            {
                default:
                case RollMode.WorldUp:
                    {
                        var forward = GetPoint(wight + DefaultWightStep);
                        var toLook = Quaternion.FromToRotation(forward, point);
                        toLook *= Quaternion.AngleAxis(roll, point - forward);
                        var toEdge = (
                                point + toLook * Vector3.left * (Size * 0.5f),
                                point + toLook * Vector3.right * (Size * 0.5f)
                            );
                        return toEdge;
                    }
                case RollMode.Cross:
                    {
                        var back = GetPoint(wight - DefaultWightStep);
                        var forward = GetPoint(wight + DefaultWightStep);

                        var RollAxis = forward - back;

                        var crossVec = Vector3.Cross(back - point, forward - point);
                        crossVec *= Vector3.left.magnitude / crossVec.magnitude;

                        crossVec = Quaternion.AngleAxis(roll, RollAxis) * crossVec;

                        crossVec *= (Size * 0.5f);
                        var Inverse = crossVec * -1;

                        var Left = point + crossVec;
                        var right = point + Inverse;

                        return (Left, right);
                    }
            }

        }

        public List<Vector3> GetLine(float StartWight, float EndWight)
        {
            var nawWight = StartWight;
            var line = new List<Vector3>();

            while (nawWight < EndWight)
            {
                line.Add(GetPoint(nawWight));
                nawWight += DefaultWightStep;
            }

            return line;
        }

        public List<Vector3> GetLine()
        {
            return GetLine(0f, Segments.Count - 1f);
        }
    }

    internal interface ICurve
    {
        Vector3 GetPoint(float wight);
        float GetRoll(float wight);
        (Vector3 point, float weight) GetOfLength(float formWight, float length);
    }

    internal enum RollMode
    {
        WorldUp,
        Cross,

    }
}