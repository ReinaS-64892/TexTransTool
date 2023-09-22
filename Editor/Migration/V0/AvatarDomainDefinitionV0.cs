#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.MatAndTexUtils;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Utils;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Migration.V0
{
    [Obsolete]
    internal static class AvatarDomainDefinitionV0
    {
        public static void MigrationAvatarDomainDefinitionV0ToV1(PhaseDefinition avatarDomainDefinition)
        {
            if (avatarDomainDefinition == null) { Debug.LogWarning("マイグレーションターゲットが存在しません。"); return; }

            var aTTGs = avatarDomainDefinition.gameObject.GetComponents<AbstractTexTransGroup>().Where(I => !(I is PhaseDefinition));

            foreach (var aTTG in aTTGs)
            {
                UnityEngine.Object.DestroyImmediate(aTTG);
            }



        }
    }
}
#endif