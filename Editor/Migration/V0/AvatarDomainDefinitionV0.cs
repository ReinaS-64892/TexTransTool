using System;
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V0
{
    [Obsolete]
    internal static class AvatarDomainDefinitionV0
    {
        public static void MigrationAvatarDomainDefinitionV0ToV1(PhaseDefinition avatarDomainDefinition)
        {
            if (avatarDomainDefinition == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }

            var aTTGs = avatarDomainDefinition.gameObject.GetComponents<TexTransGroup>();

            foreach (var aTTG in aTTGs)
            {
                UnityEngine.Object.DestroyImmediate(aTTG);
            }



        }
    }
}
