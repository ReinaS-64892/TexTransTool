using UnityEngine;
using UnityEngine.Serialization;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + NACMenuPath)]
    public sealed class NegotiateAAOConfig : MonoBehaviour, ITexTransToolTag
    {
        internal const string Name = "TTT NegotiateAAOConfig";
        internal const string NACMenuPath = TextureBlender.FoldoutName + "/" + Name;

        public bool UVEvacuationAndRegisterToAAO = true;
        public bool OverrideEvacuationUVChannel = false;
        [Range(1, 7)] public int OverrideEvacuationUVChannelIndex = 7;
        [FormerlySerializedAs("AAORemovalToIslandDisabling")] public bool AAORemovalToIsland = true;



        [HideInInspector, SerializeField] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        int ITexTransToolTag.SaveDataVersion => _saveDataVersion;
    }
}
