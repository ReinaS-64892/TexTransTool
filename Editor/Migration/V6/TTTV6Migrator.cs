using System;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V6
{
    [Obsolete]
    internal class TTTV6Migrator : IMigrator
    {
        public int MigrateTarget => 6;
        public bool Migration(ITexTransToolTag texTransToolTag)
        {
            switch (texTransToolTag)
            {
                case AtlasTexture atlasTexture:
                    {
                        AtlasTextureV6.MigrationAtlasTextureV6ToV7(atlasTexture);
                        return true;
                    }
                case SimpleDecal simpleDecal:
                    {
                        SimpleDecalV6.MigrationSimpleDecalV6ToV7(simpleDecal);
                        return true;
                    }
                case ColorDifferenceChanger:
                case TextureBlender:
                case TextureConfigurator:
                case MultiLayerImageCanvas:
                    {
                        TextureSelectorV6.MigrationTextureSelectorV6ToV7(texTransToolTag as TexTransMonoBase);
                        return true;
                    }
                default:
                    {
                        MigrationUtility.SetSaveDataVersion(texTransToolTag, 7);
                        return true;
                    }
            }
        }
    }
}
