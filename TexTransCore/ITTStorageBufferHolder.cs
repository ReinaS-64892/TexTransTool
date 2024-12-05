#nullable enable
namespace net.rs64.TexTransCore
{
    public interface ITTStorageBufferHolder : ITTObject
    {
        // これが true の場合は take 系の関数が使える。そうでない場合は使えない、所有権的な何かだと考えてよい。
        bool Owned { get; }
    }
}
