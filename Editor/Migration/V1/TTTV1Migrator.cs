using System;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TextureAtlas;

namespace net.rs64.TexTransTool.Migration.V1
{
    [Obsolete]
    internal class TTTV1Migrator : IMigrator
    {public int MigrateTarget => 1;
        public bool Migration(ITexTransToolTag texTransToolTag)
        {
            switch (texTransToolTag)
            {
                case AtlasTexture atlasTexture:
                    {
                        AtlasTextureV1.MigrationAtlasTextureV1ToV2(atlasTexture);
                        return true;
                    }
                case SimpleDecal abstractDecal:
                    {
                        AbstractDecalV1.MigrationAbstractDecalV1ToV2(abstractDecal);
                        return true;
                    }
                case TextureBlender textureBlender:
                    {
                        TextureBlenderV1.MigrationV1ToV2(textureBlender);
                        return true;
                    }
                default:
                    {
                        MigrationUtility.SetSaveDataVersion(texTransToolTag, 2);
                        return true;
                    }
            }
        }
    }
}
