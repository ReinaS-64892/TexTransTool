#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEditor;

namespace net.rs64.TexTransTool.Deprecated.V0.TextureAtlas
{

    public class AtlasTexture : TextureTransformer
    {
        public GameObject TargetRoot;
        public List<Material> SelectReferenceMat;//OrderedHashSetにしたかったけどシリアライズの都合で
        public List<MatSelector> MatSelectors = new List<MatSelector>();
        public List<AtlasSetting> AtlasSettings = new List<AtlasSetting>() { new AtlasSetting() };
        public bool UseIslandCache = true;

        [SerializeField] bool _isApply = false;
        public override bool IsPossibleApply => TargetRoot != null && AtlasSettings.Count > 0;

        public override List<Renderer> GetRenderers => throw new NotImplementedException();

        public override bool IsApply { get => _isApply; set => _isApply = value; }

        public AvatarDomain RevertDomain;
        public List<MeshPair> RevertMeshes;

        public override void Apply(IDomain avatarMaterialDomain = null)
        {
        }

    }
    [Serializable]
    public class MatSelector
    {
        public Material Material;
        public bool IsTarget = false;
        public int AtlasChannel = 0;
        public float TextureSizeOffSet = 1;
    }
    [CustomEditor(typeof(AtlasTexture))]
    public class MigrationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Migrate"))
            {
                MigrationAtlasTextureV0(target as AtlasTexture);
            }
        }

        public static void MigrationAtlasTextureV0(AtlasTexture atlasTexture)
        {
            if (atlasTexture == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }
            var GameObject = atlasTexture.gameObject;

            if (atlasTexture.AtlasSettings.Count == 1)
            {
                var newAtlasTexture = GameObject.AddComponent<net.rs64.TexTransTool.TextureAtlas.AtlasTexture>();

                CopySetting(atlasTexture, 0, newAtlasTexture);

                EditorUtility.SetDirty(newAtlasTexture);
                DestroyImmediate(atlasTexture);
            }
            else
            {
                var texTransParentGroup = GameObject.AddComponent<TexTransParentGroup>();

                for (int Count = 0; atlasTexture.AtlasSettings.Count > Count; Count += 1)
                {
                    var newGameObject = new GameObject("Channel " + Count);
                    newGameObject.transform.SetParent(GameObject.transform);

                    var newAtlasTexture = newGameObject.AddComponent<net.rs64.TexTransTool.TextureAtlas.AtlasTexture>();
                    CopySetting(atlasTexture, Count, newAtlasTexture);
                    EditorUtility.SetDirty(newAtlasTexture);
                }

                DestroyImmediate(atlasTexture);
            }

        }

        private static void CopySetting(AtlasTexture atlasTexture, int atlasSettingIndex, TexTransTool.TextureAtlas.AtlasTexture newAtlasTexture)
        {
            newAtlasTexture.TargetRoot = atlasTexture.TargetRoot;
            newAtlasTexture.AtlasSetting = atlasTexture.AtlasSettings[atlasSettingIndex];
            newAtlasTexture.AtlasSetting.UseIslandCache = atlasTexture.UseIslandCache;
            newAtlasTexture.SelectMatList = atlasTexture.MatSelectors
            .Where(I => I.IsTarget && I.AtlasChannel == atlasSettingIndex)
            .Select(I => new TexTransTool.TextureAtlas.AtlasTexture.MatSelector()
            {
                Material = I.Material,
                TextureSizeOffSet = I.TextureSizeOffSet
            }).ToList();
        }
    }
}
#endif
