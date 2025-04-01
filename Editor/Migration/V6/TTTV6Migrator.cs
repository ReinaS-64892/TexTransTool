using System;
using System.Linq;
using net.rs64.TexTransTool.Decal;
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

                default:
                    {
                        MigrationUtility.SetSaveDataVersion(texTransToolTag, 7);
                        return true;
                    }
            }
        }
    }
}
