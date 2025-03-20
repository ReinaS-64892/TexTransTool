using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using net.rs64.TexTransTool.TextureAtlas.IslandRelocator;
using UnityEngine.Serialization;
using Unity.Collections;
// using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;
using UnityEngine.Profiling;
using Unity.Mathematics;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore;
// using static net.rs64.TexTransTool.TransTexture;
using net.rs64.TexTransTool.UVIsland;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public sealed partial class AtlasTexture
    {

        #region V0SaveData
        [Obsolete("V0SaveData", true)] public List<AtlasTexture> MigrationV0ObsoleteChannelsRef;
        [Obsolete("V0SaveData", true)] public List<Material> SelectReferenceMat;//OrderedHashSetにしたかったけどシリアライズの都合で
        [Obsolete("V0SaveData", true)] public List<MatSelectorV0> MatSelectors = new List<MatSelectorV0>();
        [Obsolete("V0SaveData", true)][SerializeField] internal List<AtlasSetting> AtlasSettings = new List<AtlasSetting>() { new AtlasSetting() };
        [Obsolete("V0SaveData", true)] public bool UseIslandCache = true;
        #endregion
    }
    public partial class AtlasSetting
    {

        #region V6SaveData
        [Obsolete("V6SaveData", true)][SerializeField][PowerOfTwo] internal int HeightDenominator = 1;
        [Obsolete("V6SaveData", true)][SerializeField] internal bool MergeMaterials = false;
        [Obsolete("V6SaveData", true)][SerializeField] internal Material MergeReferenceMaterial = null;
        [Obsolete("V6SaveData", true)][SerializeField][FormerlySerializedAs("MaterialMargeGroups")] internal List<MaterialMergeGroup> MaterialMergeGroups = new();

        [Serializable][Obsolete("V6SaveData", true)]
        public class MaterialMergeGroup
        {
            [FormerlySerializedAs("MargeReferenceMaterial")] public Material MergeReferenceMaterial;
            public List<Material> GroupMaterials;
        }
        #endregion

        #region V3SaveData
        public bool UseUpScaling = false;
        #endregion
        #region V2SaveData
        [Obsolete("V2SaveData", true)][SerializeField] internal List<TextureFineTuningData> TextureFineTuningDataList = new List<TextureFineTuningData> { new TextureFineTuningData() };
        [Obsolete("V2SaveData", true)][SerializeField] internal float Padding;
        #endregion
        #region V1SaveData
        [Obsolete("V1SaveData", true)][SerializeField] internal bool UseIslandCache = true;
        #endregion

    }

}
