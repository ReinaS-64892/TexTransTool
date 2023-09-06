#if UNITY_EDITOR

using System;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.Decal
{
    [CreateAssetMenu(fileName = "NailOffsetData", menuName = "TexTransTool/NailOffsetData", order = 1)]
    public class NailOffsetData : ScriptableObject , ITexTransToolTag
    {
        [HideInInspector,SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        public NailOffSets LeftHand = new NailOffSets();
        public NailOffSets RightHand = new NailOffSets();
    }
    [Serializable]
    public class NailOffSets
    {
        public UpVector UpVector;

        public NailOffset Thumb = new NailOffset();
        public NailOffset Index = new NailOffset();
        public NailOffset Middle = new NailOffset();
        public NailOffset Ring = new NailOffset();
        public NailOffset Little = new NailOffset();

        public void Copy(NailOffSets Souse)
        {
            UpVector = Souse.UpVector;

            Thumb.Copy(Souse.Thumb);
            Index.Copy(Souse.Index);
            Middle.Copy(Souse.Middle);
            Ring.Copy(Souse.Ring);
            Little.Copy(Souse.Little);
        }
        public void Copy(NailSet Souse)
        {
            UpVector = Souse.FingerUpVector;

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
    public class NailOffset
    {

        public Vector3 PositionOffset = Vector3.zero;
        public Vector3 ScaleOffset = Vector3.one;
        public Vector3 RotationOffset = Vector3.zero;

        public void Copy(NailOffset Souse)
        {
            PositionOffset = Souse.PositionOffset;
            ScaleOffset = Souse.ScaleOffset;
            RotationOffset = Souse.RotationOffset;
        }
        public void Copy(NailDecalDescription Souse)
        {
            PositionOffset = Souse.PositionOffset;
            ScaleOffset = Souse.ScaleOffset;
            RotationOffset = Souse.RotationOffset;
        }

        public NailOffset Clone()
        {
            var New = new NailOffset();
            New.Copy(this);
            return New;
        }
    }
}


#endif
