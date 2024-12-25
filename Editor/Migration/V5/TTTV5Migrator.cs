using System;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V5
{
    [Obsolete]
    internal class TTTV5Migrator : IMigrator
    {
        public int MigrateTarget => 5;
        public bool Migration(ITexTransToolTag texTransToolTag)
        {
            switch (texTransToolTag)
            {
                case SimpleDecal simpleDecal:
                    {
                        SimpleDecalV5.MigrationSimpleDecalV5ToV6(simpleDecal);
                        return true;
                    }
                case SingleGradationDecal singleGradationDecal:
                    {
                        SingleGradationDecalV5.MigrationSingleGradationDecalV5ToV6(singleGradationDecal);
                        return true;
                    }
                default:
                    {
                        MigrationUtility.SetSaveDataVersion(texTransToolTag, 6);
                        return true;
                    }
            }
        }
    }
}
