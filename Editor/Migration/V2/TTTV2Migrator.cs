using System;
using net.rs64.TexTransTool.TextureAtlas;

namespace net.rs64.TexTransTool.Migration.V2
{
    [Obsolete]
    internal class TTTV2Migrator : IMigrator
    {
        public int MigrateTarget => 2;
        public bool Migration(ITexTransToolTag texTransToolTag)
        {
            switch (texTransToolTag)
            {
                case AtlasTexture atlasTexture:
                    {
                        AtlasTextureV2.MigrationAtlasTextureV2ToV3(atlasTexture);
                        return true;
                    }

                default:
                    {
                        MigrationUtility.SetSaveDataVersion(texTransToolTag, 3);
                        return true;
                    }
            }
        }
    }
}
