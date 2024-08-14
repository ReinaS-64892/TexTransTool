using System;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TextureAtlas;

namespace net.rs64.TexTransTool.Migration.V0
{
    [Obsolete]
    internal class TTTV0Migrator : IMigrator , IMigratorUseFinalize
    {
        public int MigrateTarget => 0;

        public bool Migration(ITexTransToolTag texTransToolTag)
        {
            switch (texTransToolTag)
            {
                case AtlasTexture atlasTexture:
                    {
                        AtlasTextureV0.MigrationAtlasTextureV0ToV1(atlasTexture);
                        return true;
                    }
                case SimpleDecal abstractDecal:
                    {
                        AbstractDecalV0.MigrationAbstractDecalV0ToV1(abstractDecal);
                        return true;
                    }
                case PhaseDefinition phaseDefinition:
                    {
                        AvatarDomainDefinitionV0.MigrationAvatarDomainDefinitionV0ToV1(phaseDefinition);
                        return true;
                    }
                default:
                    {
                        MigrationUtility.SetSaveDataVersion(texTransToolTag, 1);
                        return true;
                    }
            }
        }
        public bool MigrationFinalize(ITexTransToolTag texTransToolTag)
        {
            switch (texTransToolTag)
            {
                case AtlasTexture atlasTexture:
                    {
                        AtlasTextureV0.FinalizeMigrationAtlasTextureV0ToV1(atlasTexture);
                        return true;
                    }
                case SimpleDecal abstractDecal:
                    {
                        AbstractDecalV0.FinalizeMigrationAbstractDecalV0ToV1(abstractDecal);
                        return true;
                    }
                default:
                    {
                        MigrationUtility.SetSaveDataVersion(texTransToolTag, 1);
                        return true;
                    }
            }
        }
    }
}
