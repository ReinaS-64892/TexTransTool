using UnityEngine;
using System;

namespace net.rs64.TexTransTool
{
    internal struct RTActiveSaver : IDisposable
    {
        readonly RenderTexture PreRT;
        public RTActiveSaver(bool empty = false)
        {
            PreRT = RenderTexture.active;
        }
        public void Dispose()
        {
            RenderTexture.active = PreRT;
        }
    }

}
