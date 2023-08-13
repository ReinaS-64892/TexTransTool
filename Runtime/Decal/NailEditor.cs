#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

namespace Rs64.TexTransTool.Decal
{
    public class NailEditor : AbstractDecal
    {
        public Animator TargetAvatar;

        public NailSet LeftHand;
        public NailSet RightHand;

        public Upvector FingerUpvector;


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
                            TargetPropatyName,
                            GetOutRengeTexture,
                            DefaultPading
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
                                    TargetPropatyName,
                                    GetOutRengeTexture,
                                    DefaultPading
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
                foreach (var NaileDD in nailSet)
                {
                    var Finger = NaileDD.Item1;
                    var naileDecalDescripstion = NaileDD.Item2;
                    if (naileDecalDescripstion.DecalTexture == null) continue;
                    var SorsFingetTF = GetFinger(Finger, IsRight);
                    Matrix4x4 Matlix = GetNailMatrix(SorsFingetTF, naileDecalDescripstion);

                    var ray = new Ray(Matlix.MultiplyPoint(Vector3.zero), Matlix.MultiplyVector(Vector3.forward));

                    var SpaseConverter = new ParallelProjectionSpase(Matlix.inverse);
                    var Filter = new IslandCullingPPFilter(GetFilter(), new List<Ray>(1) { ray });

                    Spases.Add((naileDecalDescripstion.DecalTexture, SpaseConverter, Filter));
                }
            }

            return Spases;
        }

        public List<TrainagelFilterUtility.ITraiangleFiltaring<List<Vector3>>> GetFilter()
        {
            return new List<TrainagelFilterUtility.ITraiangleFiltaring<List<Vector3>>>
            {
                new TrainagelFilterUtility.FarStruct(1, false),
                new TrainagelFilterUtility.NearStruct(0, true),
                new TrainagelFilterUtility.SideStruct(),
                new TrainagelFilterUtility.OutOfPorigonStruct(PolygonCaling.Edge, 0, 1, true)
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (TargetAvatar == null) return;

            DrawNailGizm(LeftHand, false);
            DrawNailGizm(RightHand, true);



            void DrawNailGizm(NailSet nailSet, bool IsRight)
            {
                foreach (var NaileDD in nailSet)
                {
                    var Finger = NaileDD.Item1;
                    var naileDecalDescripstion = NaileDD.Item2;
                    var SorsFingetTF = GetFinger(Finger, IsRight);
                    Matrix4x4 Matlix = GetNailMatrix(SorsFingetTF, naileDecalDescripstion);

                    Gizmos.matrix = Matlix;
                    Gizmos.DrawWireCube(new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));
                    Gizmos.DrawLine(Vector3.zero, Vector3.forward);
                }
            }
        }

        private Matrix4x4 GetNailMatrix(Transform SorsFingetTF, NaileDecalDescripstion naileDecalDescripstion)
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
            NailPos += SRot * naileDecalDescripstion.PositionOffset;
            var NailRot = SRot * Quaternion.Euler(naileDecalDescripstion.RotationOffset);
            var NailSize = naileDecalDescripstion.ScaileOffset * FingerSize * 0.75f;

            return Matrix4x4.TRS(NailPos, NailRot, NailSize);
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
    public class NailSet : IEnumerable<(Finger, NaileDecalDescripstion)>
    {
        public NaileDecalDescripstion Thumb;
        public NaileDecalDescripstion Index;
        public NaileDecalDescripstion Middle;
        public NaileDecalDescripstion Ring;
        public NaileDecalDescripstion Little;




        IEnumerator<(Finger, NaileDecalDescripstion)> IEnumerable<(Finger, NaileDecalDescripstion)>.GetEnumerator()
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
    }

    [Serializable]
    public class NaileDecalDescripstion
    {
        public Texture2D DecalTexture;

        public Vector3 PositionOffset = Vector3.zero;
        public Vector3 ScaileOffset = Vector3.one;
        public Vector3 RotationOffset = Vector3.zero;
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