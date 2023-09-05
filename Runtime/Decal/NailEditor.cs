#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using net.rs64.TexTransTool.Island;

namespace net.rs64.TexTransTool.Decal
{

    [AddComponentMenu("TexTransTool/NailEditor")]
    public class NailEditor : AbstractDecal
    {
        public Animator TargetAvatar;

        public NailSet LeftHand;
        public NailSet RightHand;

        public bool UseTextureAspect = false;



        public override bool IsPossibleApply => TargetAvatar != null && TargetRenderers.Any(i => i != null);

        public override Dictionary<Texture2D, Texture> CompileDecal()
        {
            if (FastMode)
            {
                var decalCompiledTextures = new Dictionary<Texture2D, RenderTexture>();

                foreach (var nailTexSpaceFilter in GetNailTexSpaceFilters())
                {
                    foreach (var renderer in TargetRenderers)
                    {
                        DecalUtil.CreateDecalTexture(
                            renderer,
                            decalCompiledTextures,
                            nailTexSpaceFilter.Item1,
                            nailTexSpaceFilter.Item2,
                            nailTexSpaceFilter.Item3,
                            TargetPropertyName,
                            GetOutRangeTexture,
                            Padding
                        );
                    }
                }

                var decalCompiledRenderTextures = new Dictionary<Texture2D, Texture>();
                foreach (var texture in decalCompiledTextures)
                {
                    decalCompiledRenderTextures.Add(texture.Key, texture.Value);
                }
                return decalCompiledRenderTextures;
            }
            else
            {
                var decalsCompileTexListDict = new List<Dictionary<Texture2D, List<Texture2D>>>();


                foreach (var nailTexSpaceFilter in GetNailTexSpaceFilters())
                {
                    foreach (var renderer in TargetRenderers)
                    {
                        decalsCompileTexListDict.Add(
                                DecalUtil.CreateDecalTextureCS(
                                    renderer,
                                    nailTexSpaceFilter.Item1,
                                    nailTexSpaceFilter.Item2,
                                    nailTexSpaceFilter.Item3,
                                    TargetPropertyName,
                                    GetOutRangeTexture,
                                    Padding
                                ));
                    }
                }

                var decalCompiledRenderTextures = new Dictionary<Texture2D, Texture>();

                var zipDict = Utils.ZipToDictionaryOnList(decalsCompileTexListDict);

                foreach (var texture in zipDict)
                {
                    var blendTexture = TextureLayerUtil.BlendTextureUseComputeShader(null, texture.Value, BlendType.AlphaLerp);
                    blendTexture.Apply();
                    decalCompiledRenderTextures.Add(texture.Key, blendTexture);
                }

                return decalCompiledRenderTextures;
            }
        }

        List<(Texture2D, ParallelProjectionSpace, ParallelProjectionFilter)> GetNailTexSpaceFilters()
        {
            var spaceList = new List<(Texture2D, ParallelProjectionSpace, ParallelProjectionFilter)>();


            CompileNail(LeftHand, false);
            CompileNail(RightHand, true);


            void CompileNail(NailSet nailSet, bool IsRight)
            {
                foreach (var NailDD in nailSet)
                {
                    var finger = NailDD.Item1;
                    var nailDecalDescription = NailDD.Item2;
                    if (nailDecalDescription.DecalTexture == null) continue;
                    var souseFingerTF = GetFinger(finger, IsRight);
                    var matrix = GetNailMatrix(souseFingerTF, nailDecalDescription, nailSet.FingerUpVector, IsRight);

                    var islandSelector = new IslandSelector(new Ray(matrix.MultiplyPoint(Vector3.zero), matrix.MultiplyVector(Vector3.forward)), matrix.lossyScale.z * 1);

                    var SpaceConverter = new ParallelProjectionSpace(matrix.inverse);
                    var Filter = new IslandCullingPPFilter(GetFilter(), new List<IslandSelector>(1) { islandSelector });

                    spaceList.Add((nailDecalDescription.DecalTexture, SpaceConverter, Filter));
                }
            }

            return spaceList;
        }

        public List<TriangleFilterUtils.ITriangleFiltering<List<Vector3>>> GetFilter()
        {
            return new List<TriangleFilterUtils.ITriangleFiltering<List<Vector3>>>
            {
                new TriangleFilterUtils.FarStruct(1, false),
                new TriangleFilterUtils.NearStruct(0, true),
                new TriangleFilterUtils.SideStruct(),
                new TriangleFilterUtils.OutOfPolygonStruct(PolygonCulling.Edge, 0, 1, true)
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (TargetAvatar == null) return;

            DrawNailGizmo(LeftHand, false);
            DrawNailGizmo(RightHand, true);



            void DrawNailGizmo(NailSet nailSet, bool IsRight)
            {
                foreach (var NailDD in nailSet)
                {
                    var Finger = NailDD.Item1;
                    var nailDecalDescription = NailDD.Item2;
                    var souseFingerTF = GetFinger(Finger, IsRight);
                    var matrix = GetNailMatrix(souseFingerTF, nailDecalDescription, nailSet.FingerUpVector, IsRight);

                    Gizmos.matrix = matrix;
                    Gizmos.DrawWireCube(new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));
                    Gizmos.DrawLine(Vector3.zero, Vector3.forward);
                }
            }
        }

        private Matrix4x4 GetNailMatrix(Transform souseFingerTF, NailDecalDescription nailDecalDescription, UpVector FingerUpVector, bool InvarsRight)
        {
            var fingerSize = souseFingerTF.localPosition.magnitude;
            var sRot = souseFingerTF.rotation;

            switch (FingerUpVector)
            {
                default:
                case UpVector.ZMinus:
                    break;
                case UpVector.ZPlus:
                    sRot *= Quaternion.Euler(0, 180, 0);
                    break;
                case UpVector.YMinus:
                    sRot *= Quaternion.Euler(90, 0, 0);
                    break;
                case UpVector.YPlus:
                    sRot *= Quaternion.Euler(-90, 0, 0);
                    break;
                case UpVector.XMinus:
                    sRot *= Quaternion.Euler(0, 90, 0);
                    break;
                case UpVector.XPlus:
                    sRot *= Quaternion.Euler(0, -90, 0);
                    break;

            }

            var nailPos = souseFingerTF.position;
            nailPos += sRot * (souseFingerTF.localPosition * 0.9f);
            nailPos += sRot * new Vector3(0, 0, fingerSize * -0.25f);
            nailPos += sRot * (!InvarsRight ? nailDecalDescription.PositionOffset : PosOffsetInverseRight(nailDecalDescription.PositionOffset));
            var nailRot = sRot * Quaternion.Euler(!InvarsRight ? nailDecalDescription.RotationOffset : RotOffsetInverseRight(nailDecalDescription.RotationOffset));
            var nailSize = nailDecalDescription.ScaleOffset * fingerSize * 0.75f;

            if (UseTextureAspect && nailDecalDescription.DecalTexture != null) { nailSize.y *= (float)nailDecalDescription.DecalTexture.height / (float)nailDecalDescription.DecalTexture.width; }

            return Matrix4x4.TRS(nailPos, nailRot, nailSize);
        }

        public Vector3 PosOffsetInverseRight(Vector3 positionOffset)
        {
            return new Vector3(positionOffset.x * -1, positionOffset.y, positionOffset.z);
        }
        public Vector3 RotOffsetInverseRight(Vector3 rotationOffset)
        {
            return new Vector3(rotationOffset.x, rotationOffset.y * -1, rotationOffset.z * -1);
        }
        public Transform GetFinger(Finger finger, bool IsRight)
        {
            return TargetAvatar.GetBoneTransform(ConvertHumanBodyBones(finger, IsRight));
        }

        public HumanBodyBones ConvertHumanBodyBones(Finger finger, bool IsRight)
        {
            switch (finger)
            {
                default:
                case Finger.Thumb:
                    return !IsRight ? HumanBodyBones.LeftThumbDistal : HumanBodyBones.RightThumbDistal;
                case Finger.Index:
                    return !IsRight ? HumanBodyBones.LeftIndexDistal : HumanBodyBones.RightIndexDistal;
                case Finger.Middle:
                    return !IsRight ? HumanBodyBones.LeftMiddleDistal : HumanBodyBones.RightMiddleDistal;
                case Finger.Ring:
                    return !IsRight ? HumanBodyBones.LeftRingDistal : HumanBodyBones.RightRingDistal;
                case Finger.Little:
                    return !IsRight ? HumanBodyBones.LeftLittleDistal : HumanBodyBones.RightLittleDistal;
            }
        }
    }

    [Serializable]
    public class NailSet : IEnumerable<(Finger, NailDecalDescription)>
    {
        public UpVector FingerUpVector;

        public NailDecalDescription Thumb;
        public NailDecalDescription Index;
        public NailDecalDescription Middle;
        public NailDecalDescription Ring;
        public NailDecalDescription Little;

        public NailSet()
        {
            Thumb = new NailDecalDescription();
            Index = new NailDecalDescription();
            Middle = new NailDecalDescription();
            Ring = new NailDecalDescription();
            Little = new NailDecalDescription();
        }

        IEnumerator<(Finger, NailDecalDescription)> IEnumerable<(Finger, NailDecalDescription)>.GetEnumerator()
        {
            yield return (Finger.Thumb, Thumb);
            yield return (Finger.Index, Index);
            yield return (Finger.Middle, Middle);
            yield return (Finger.Ring, Ring);
            yield return (Finger.Little, Little);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return (Finger.Thumb, Thumb);
            yield return (Finger.Index, Index);
            yield return (Finger.Middle, Middle);
            yield return (Finger.Ring, Ring);
            yield return (Finger.Little, Little);
        }

        public void Copy(NailSet Souse)
        {
            FingerUpVector = Souse.FingerUpVector;
            Thumb.Copy(Souse.Thumb);
            Index.Copy(Souse.Index);
            Middle.Copy(Souse.Middle);
            Ring.Copy(Souse.Ring);
            Little.Copy(Souse.Little);
        }
        public void Copy(NailOffSets Souse)
        {
            FingerUpVector = Souse.UpVector;
            Thumb.Copy(Souse.Thumb);
            Index.Copy(Souse.Index);
            Middle.Copy(Souse.Middle);
            Ring.Copy(Souse.Ring);
            Little.Copy(Souse.Little);

        }
        public NailSet Clone()
        {
            var newI = new NailSet();
            newI.Copy(this);
            return newI;
        }

    }

    [Serializable]
    public class NailDecalDescription
    {
        public Texture2D DecalTexture;

        public Vector3 PositionOffset = Vector3.zero;
        public Vector3 ScaleOffset = Vector3.one;
        public Vector3 RotationOffset = Vector3.zero;

        public void Copy(NailDecalDescription Souse)
        {
            DecalTexture = Souse.DecalTexture;
            PositionOffset = Souse.PositionOffset;
            ScaleOffset = Souse.ScaleOffset;
            RotationOffset = Souse.RotationOffset;
        }
        public NailDecalDescription Clone()
        {
            var newI = new NailDecalDescription();
            newI.Copy(this);
            return newI;
        }

        public void Copy(NailOffset Souse)
        {
            PositionOffset = Souse.PositionOffset;
            ScaleOffset = Souse.ScaleOffset;
            RotationOffset = Souse.RotationOffset;
        }
    }

    public enum Finger
    {
        Thumb,
        Index,
        Middle,
        Ring,
        Little,
    }

    public enum UpVector
    {
        ZMinus,
        ZPlus,
        YMinus,
        YPlus,
        XMinus,
        XPlus,
    }

}


#endif
