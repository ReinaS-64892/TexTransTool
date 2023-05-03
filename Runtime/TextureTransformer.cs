#if UNITY_EDITOR
using UnityEngine;
namespace Rs.TexturAtlasCompiler
{
    public abstract class TextureTransformer : MonoBehaviour
    {
        public abstract bool IsAppry { get; }
        public abstract bool IsPossibleAppry { get; }
        public abstract void Compile();
        public abstract void Appry();
        public abstract void Revart();
    }
}
#endif