using System;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V4
{
    [Obsolete]
    internal class TTTV4Migrator : IMigrator
    {
        public int MigrateTarget => 4;
        public bool Migration(ITexTransToolTag texTransToolTag)
        {
            switch (texTransToolTag)
            {
                case AtlasTexture atlasTexture:
                    {
                        AtlasTextureV4.MigrationAtlasTextureV4ToV5(atlasTexture);
                        return true;
                    }

                default:
                    {
                        MigrationUtility.SetSaveDataVersion(texTransToolTag, 5);
                        return true;
                    }
            }
        }
    }
}
