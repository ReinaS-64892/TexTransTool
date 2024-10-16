#if CONTAINS_AAO
using System;
using System.Collections.Generic;
using System.Linq;
using Anatawa12.AvatarOptimizer.API;
using nadena.dev.ndmf;
using net.rs64.TexTransCoreEngineForUnity.Island;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.TextureAtlas.AAOCode;
using net.rs64.TexTransTool.Utils;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool.NDMF.AAO
{
    internal class AtlasRemappingUVUsageTransmitter : TTTPass<AtlasRemappingUVUsageTransmitter>
    {
        protected override void Execute(BuildContext context)
        {
            if (TTTContext(context).PhaseAtList.SelectMany(i => i.Value).Any(i => i is AtlasTexture) is false) { return; }

            var renderers = context.AvatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var r in renderers) { r.gameObject.AddComponent<AtlasReMappingReceiverForEditorCall>().EditorCall = Transmit; }
        }

        void Transmit(AtlasReMappingReceiverForEditorCall receiver, ReceiversUVState uvState, Mesh normalized, Mesh atlasMesh)
        {
            var smr = receiver.GetComponent<SkinnedMeshRenderer>();
            if (UVUsageCompabilityAPI.IsTexCoordUsed(smr, 0) is false) { return; }

            if (uvState.WriteIsAtlasTexture)
            {
                UVUsageCompabilityAPI.RegisterTexCoordEvacuation(smr, 0, uvState.AlreadyOriginWriteable.Value);
                return;//これ　ユーザー指示の UV で破棄してほしくない場合どうすればいいんだろう
            }

            if (uvState.AlreadyOriginWriteable is not null) { UVUsageCompabilityAPI.RegisterTexCoordEvacuation(smr, 0, uvState.AlreadyOriginWriteable.Value); return; }

            var uvSaveIndex = 1;
            while (atlasMesh.HasVertexAttribute((UnityEngine.Rendering.VertexAttribute)(4 + uvSaveIndex)))
            {
                uvSaveIndex += 1;
                if (uvSaveIndex > 7) { TTTLog.Error("UVの空きがないよ"); }
            }


            var uv = new List<Vector2>();

            normalized.GetUVs(0, uv);
            atlasMesh.SetUVs(uvSaveIndex, uv);
            uvState.AlreadyOriginWriteable = uvSaveIndex;
            UVUsageCompabilityAPI.RegisterTexCoordEvacuation(smr, 0, uvState.AlreadyOriginWriteable.Value);

        }
    }
}
#endif
