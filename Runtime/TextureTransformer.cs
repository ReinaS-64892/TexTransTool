#if UNITY_EDITOR
using UnityEngine;
namespace Rs64.TexTransTool
{
    public abstract class TextureTransformer : MonoBehaviour
    {
        public abstract bool IsAppry { get; }
        public abstract bool IsPossibleAppry { get; }
        public abstract bool IsPossibleCompile { get; }
        public abstract void Compile();
        public abstract void Appry();
        public abstract void Revart();
    }
}
#endif