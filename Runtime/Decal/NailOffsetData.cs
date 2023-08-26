#if UNITY_EDITOR

using System;
using UnityEngine;

namespace net.rs64.TexTransTool.Decal
{
    [CreateAssetMenu(fileName = "NailOffsetData", menuName = "TexTransTool/NailOffsetData", order = 1)]
    public class NailOffsetData : ScriptableObject , ITexTransToolTag
    {
        [HideInInspector,SerializeField] int _saveDataVersion = Utils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        public NailOffSets LeftHand = new NailOffSets();
        public NailOffSets RightHand = new NailOffSets();
    }
    [Serializable]
    public class NailOffSets
    {
        public Upvector Upvector;

        public NaileOffset Thumb = new NaileOffset();
        public NaileOffset Index = new NaileOffset();
        public NaileOffset Middle = new NaileOffset();
        public NaileOffset Ring = new NaileOffset();
        public NaileOffset Little = new NaileOffset();

        public void Copy(NailOffSets Souse)
        {
            Upvector = Souse.Upvector;

            Thumb.Copy(Souse.Thumb);
            Index.Copy(Souse.Index);
            Middle.Copy(Souse.Middle);
            Ring.Copy(Souse.Ring);
            Little.Copy(Souse.Little);
        }
        public void Copy(NailSet Souse)
        {
            Upvector = Souse.FingerUpvector;

            Thumb.Copy(Souse.Thumb);
            Index.Copy(Souse.Index);
            Middle.Copy(Souse.Middle);
            Ring.Copy(Souse.Ring);
            Little.Copy(Souse.Little);
        }
        public NailOffSets Clone()
        {
            var New = new NailOffSets();
            New.Copy(this);
            return New;
        }

    }
    [Serializable]
    public class NaileOffset
    {

        public Vector3 PositionOffset = Vector3.zero;
        public Vector3 ScaileOffset = Vector3.one;
        public Vector3 RotationOffset = Vector3.zero;

        public void Copy(NaileOffset Souse)
        {
            PositionOffset = Souse.PositionOffset;
            ScaileOffset = Souse.ScaileOffset;
            RotationOffset = Souse.RotationOffset;
        }
        public void Copy(NaileDecalDescripstion Souse)
        {
            PositionOffset = Souse.PositionOffset;
            ScaileOffset = Souse.ScaileOffset;
            RotationOffset = Souse.RotationOffset;
        }

        public NaileOffset Clone()
        {
            var New = new NaileOffset();
            New.Copy(this);
            return New;
        }
    }
}


#endif