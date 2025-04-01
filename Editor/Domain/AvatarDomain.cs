using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool
{
    /// <summary>
    /// This is an IDomain implementation that applies to whole the specified GameObject.
    /// </summary>
    internal class AvatarDomain : RenderersDomain
    {
        public AvatarDomain(GameObject avatarRoot, IAssetSaver assetSaver)
        : base(avatarRoot.GetComponentsInChildren<Renderer>(true).ToList(), assetSaver)
        { _avatarRoot = avatarRoot; }

        [SerializeField] GameObject _avatarRoot;
        public GameObject AvatarRoot => _avatarRoot;

        public void ReFindRenderers() { _renderers = _avatarRoot.GetComponentsInChildren<Renderer>(true).ToList(); }

    }

}
