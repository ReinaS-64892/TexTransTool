#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using net.rs64.TexTransTool.Island;

namespace net.rs64.TexTransTool.Decal
{
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
                var DecalCompiledTextures = new Dictionary<Texture2D, RenderTexture>();

                foreach (var NailTexSpaseFilter in GetNailTexSpaseFilters())
                {
                    foreach (var Rendarer in TargetRenderers)
                    {
                        DecalUtil.CreatDecalTexture(
                            Rendarer,
                            DecalCompiledTextures,
                            NailTexSpaseFilter.Item1,
                            NailTexSpaseFilter.Item2,
                            NailTexSpaseFilter.Item3,
                            TargetPropertyName,
                            GetOutRangeTexture,
                            Padding
                        );
                    }
                }

                var DecalCompiledRenderTextures = new Dictionary<Texture2D, Texture>();
                foreach (var Texture in DecalCompiledTextures)
                {
                    DecalCompiledRenderTextures.Add(Texture.Key, Texture.Value);
                }
                return DecalCompiledRenderTextures;
            }
            else
            {
                var DecalsCompoleTexs = new List<Dictionary<Texture2D, List<Texture2D>>>();


                foreach (var NailTexSpaseFilter in GetNailTexSpaseFilters())
                {
                    foreach (var Rendarer in TargetRenderers)
                    {
                        DecalsCompoleTexs.Add(
                                DecalUtil.CreatDecalTextureCS(
                                    Rendarer,
                                    NailTexSpaseFilter.Item1,
                                    NailTexSpaseFilter.Item2,
                                    NailTexSpaseFilter.Item3,
                                    TargetPropertyName,
                                    GetOutRangeTexture,
                                    Padding
                                ));
                    }
                }

                var DecalCompiledRenderTextures = new Dictionary<Texture2D, Texture>();

                var ZipDecit = Utils.ZipToDictionaryOnList(DecalsCompoleTexs);

                foreach (var Texture in ZipDecit)
                {
                    var BlendTexture = TextureLayerUtil.BlendTextureUseComputeSheder(null, Texture.Value, BlendType.AlphaLerp);
                    BlendTexture.Apply();
                    DecalCompiledRenderTextures.Add(Texture.Key, BlendTexture);
                }

                return DecalCompiledRenderTextures;
            }
        }

        List<(Texture2D, ParallelProjectionSpase, ParallelProjectionFilter)> GetNailTexSpaseFilters()
        {
            var Spases = new List<(Texture2D, ParallelProjectionSpase, ParallelProjectionFilter)>();


            CompileNail(LeftHand, false);
            CompileNail(RightHand, true);


            void CompileNail(NailSet nailSet, bool IsRight)
            {
                foreach (var NailDD in nailSet)
                {
                    var Finger = NailDD.Item1;
                    var naileDecalDescription = NailDD.Item2;
                    if (naileDecalDescription.DecalTexture == null) continue;
                    var SorsFingetTF = GetFinger(Finger, IsRight);
                    Matrix4x4 Matlix = GetNailMatrix(SorsFingetTF, naileDecalDescription, nailSet.FingerUpvector, IsRight);

                    var islandSelecotr = new IslandSelector(new Ray(Matlix.MultiplyPoint(Vector3.zero), Matlix.MultiplyVector(Vector3.forward)), Matlix.lossyScale.z * 1);

                    var SpaseConverter = new ParallelProjectionSpase(Matlix.inverse);
                    var Filter = new IslandCullingPPFilter(GetFilter(), new List<IslandSelector>(1) { islandSelecotr });

                    Spases.Add((naileDecalDescription.DecalTexture, SpaseConverter, Filter));
                }
            }

            return Spases;
        }

        public List<TriangleFilterUtils.ITriangleFiltaring<List<Vector3>>> GetFilter()
        {
            return new List<TriangleFilterUtils.ITriangleFiltaring<List<Vector3>>>
            {
                new TriangleFilterUtils.FarStruct(1, false),
                new TriangleFilterUtils.NearStruct(0, true),
                new TriangleFilterUtils.SideStruct(),
                new TriangleFilterUtils.OutOfPorigonStruct(PolygonCulling.Edge, 0, 1, true)
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (TargetAvatar == null) return;

            DrawNailGizm(LeftHand, false);
            DrawNailGizm(RightHand, true);



            void DrawNailGizm(NailSet nailSet, bool IsRight)
            {
                foreach (var NailDD in nailSet)
                {
                    var Finger = NailDD.Item1;
                    var naileDecalDescription = NailDD.Item2;
                    var SorsFingetTF = GetFinger(Finger, IsRight);
                    Matrix4x4 Matlix = GetNailMatrix(SorsFingetTF, naileDecalDescription, nailSet.FingerUpvector, IsRight);

                    Gizmos.matrix = Matlix;
                    Gizmos.DrawWireCube(new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));
                    Gizmos.DrawLine(Vector3.zero, Vector3.forward);
                }
            }
        }

        private Matrix4x4 GetNailMatrix(Transform SorsFingetTF, NailDecalDescription naileDecalDescription, Upvector FingerUpvector, bool InvaersdRight)
        {
            var FingerSize = SorsFingetTF.localPosition.magnitude;
            var SRot = SorsFingetTF.rotation;

            switch (FingerUpvector)
            {
                default:
                case Upvector.Zminus:
                    break;
                case Upvector.Zplus:
                    SRot *= Quaternion.Euler(0, 180, 0);
                    break;
                case Upvector.Yminus:
                    SRot *= Quaternion.Euler(90, 0, 0);
                    break;
                case Upvector.Yplus:
                    SRot *= Quaternion.Euler(-90, 0, 0);
                    break;
                case Upvector.Xminus:
                    SRot *= Quaternion.Euler(0, 90, 0);
                    break;
                case Upvector.Xplus:
                    SRot *= Quaternion.Euler(0, -90, 0);
                    break;

            }

            var NailPos = SorsFingetTF.position;
            NailPos += SRot * (SorsFingetTF.localPosition * 0.9f);
            NailPos += SRot * new Vector3(0, 0, FingerSize * -0.25f);
            NailPos += SRot * (!InvaersdRight ? naileDecalDescription.PositionOffset : PosOffsetInverseRight(naileDecalDescription.PositionOffset));
            var NailRot = SRot * Quaternion.Euler(!InvaersdRight ? naileDecalDescription.RotationOffset : RotOffsetInverseRight(naileDecalDescription.RotationOffset));
            var NailSize = naileDecalDescription.ScaleOffset * FingerSize * 0.75f;

            if (UseTextureAspect && naileDecalDescription.DecalTexture != null) { NailSize.y *= (float)naileDecalDescription.DecalTexture.height / (float)naileDecalDescription.DecalTexture.width; }

            return Matrix4x4.TRS(NailPos, NailRot, NailSize);
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
                case Finger.Littl:
                    return !IsRight ? HumanBodyBones.LeftLittleDistal : HumanBodyBones.RightLittleDistal;
            }
        }
    }

    [Serializable]
    public class NailSet : IEnumerable<(Finger, NailDecalDescription)>
    {
        public Upvector FingerUpvector;

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
            yield return (Finger.Littl, Little);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return (Finger.Thumb, Thumb);
            yield return (Finger.Index, Index);
            yield return (Finger.Middle, Middle);
            yield return (Finger.Ring, Ring);
            yield return (Finger.Littl, Little);
        }

        public void Copy(NailSet Souse)
        {
            FingerUpvector = Souse.FingerUpvector;
            Thumb.Copy(Souse.Thumb);
            Index.Copy(Souse.Index);
            Middle.Copy(Souse.Middle);
            Ring.Copy(Souse.Ring);
            Little.Copy(Souse.Little);
        }
        public void Copy(NailOffSets Souse)
        {
            FingerUpvector = Souse.Upvector;
            Thumb.Copy(Souse.Thumb);
            Index.Copy(Souse.Index);
            Middle.Copy(Souse.Middle);
            Ring.Copy(Souse.Ring);
            Little.Copy(Souse.Little);

        }
        public NailSet Clone()
        {
            var New = new NailSet();
            New.Copy(this);
            return New;
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
            var New = new NailDecalDescription();
            New.Copy(this);
            return New;
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
        Littl,
    }

    public enum Upvector
    {
        Zminus,
        Zplus,
        Yminus,
        Yplus,
        Xminus,
        Xplus,
    }

}


#endif
