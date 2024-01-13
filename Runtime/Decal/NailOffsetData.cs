using System;
using UnityEngine;

namespace net.rs64.TexTransTool.Decal
{
    [CreateAssetMenu(fileName = "NailOffsetData", menuName = "TexTransTool/NailOffsetData", order = 1)]
    public sealed class NailOffsetData : ScriptableObject , ITexTransToolTag
    {
        [HideInInspector,SerializeField] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        public NailOffSets LeftHand = new NailOffSets();
        public NailOffSets RightHand = new NailOffSets();
    }
    [Serializable]
    public sealed class NailOffSets
    {
        public UpVector UpVector;

        public NailOffset Thumb = new NailOffset();
        public NailOffset Index = new NailOffset();
        public NailOffset Middle = new NailOffset();
        public NailOffset Ring = new NailOffset();
        public NailOffset Little = new NailOffset();

        public void Copy(NailOffSets souse)
        {
            UpVector = souse.UpVector;

            Thumb.Copy(souse.Thumb);
            Index.Copy(souse.Index);
            Middle.Copy(souse.Middle);
            Ring.Copy(souse.Ring);
            Little.Copy(souse.Little);
        }
        public void Copy(NailSet souse)
        {
            UpVector = souse.FingerUpVector;

            Thumb.Copy(souse.Thumb);
            Index.Copy(souse.Index);
            Middle.Copy(souse.Middle);
            Ring.Copy(souse.Ring);
            Little.Copy(souse.Little);
        }
        public NailOffSets Clone()
        {
            var newNailOffsets = new NailOffSets();
            newNailOffsets.Copy(this);
            return newNailOffsets;
        }

    }
    [Serializable]
    public sealed class NailOffset
    {

        public Vector3 PositionOffset = Vector3.zero;
        public Vector3 ScaleOffset = Vector3.one;
        public Vector3 RotationOffset = Vector3.zero;

        public void Copy(NailOffset souse)
        {
            PositionOffset = souse.PositionOffset;
            ScaleOffset = souse.ScaleOffset;
            RotationOffset = souse.RotationOffset;
        }
        public void Copy(NailDecalDescription souse)
        {
            PositionOffset = souse.PositionOffset;
            ScaleOffset = souse.ScaleOffset;
            RotationOffset = souse.RotationOffset;
        }

        public NailOffset Clone()
        {
            var newNailOffset = new NailOffset();
            newNailOffset.Copy(this);
            return newNailOffset;
        }
    }
}
