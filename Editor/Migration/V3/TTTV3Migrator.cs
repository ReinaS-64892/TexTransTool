using System;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TextureAtlas;

namespace net.rs64.TexTransTool.Migration.V3
{
    [Obsolete]
    internal class TTTV3Migrator : IMigrator
    {
        public int MigrateTarget => 3;
        public bool Migration(ITexTransToolTag texTransToolTag)
        {
            switch (texTransToolTag)
            {
                case AtlasTexture atlasTexture:
                    {
                        AtlasTextureV3.MigrationAtlasTextureV3ToV4(atlasTexture);
                        return true;
                    }
                case SimpleDecal simpleDecal:
                    {
                        SimpleDecalV3.MigrationSimpleDecalV3ToV4(simpleDecal);
                        return true;
                    }

                default:
                    {
                        MigrationUtility.SetSaveDataVersion(texTransToolTag, 4);
                        return true;
                    }
            }

        }
    }
}
