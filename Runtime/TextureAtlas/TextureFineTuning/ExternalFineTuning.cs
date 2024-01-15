using System.Collections.Generic;
using UnityEngine;
namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    internal abstract class ExternalFineTuning : ScriptableObject, IAddFineTuning
    {
        public abstract void AddSetting(List<TexFineTuningTarget> propAndTextures);
    }

}