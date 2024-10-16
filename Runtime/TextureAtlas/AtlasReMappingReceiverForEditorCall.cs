using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using net.rs64.TexTransCoreEngineForUnity.Island;
using net.rs64.TexTransCoreEngineForUnity.Utils;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.TextureAtlas.AAOCode;
using net.rs64.TexTransTool.Utils;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [AddComponentMenu("/")]
    [RequireComponent(typeof(Renderer))]
    internal class AtlasReMappingReceiverForEditorCall : MonoBehaviour, IAtlasReMappingReceiver , ITexTransToolTag
    {
        public int SaveDataVersion => TexTransBehavior.TTTDataVersion;//こいつはセーブデータを持たない。
        public void ReMappingReceive(ReceiversUVState uvState, Mesh normalizedOriginalMesh, Mesh atlasMesh)
        {
            EditorCall(this, uvState, normalizedOriginalMesh, atlasMesh);
        }

        public Action<AtlasReMappingReceiverForEditorCall, ReceiversUVState, Mesh, Mesh> EditorCall;

    }
}
