#nullable enable
namespace net.rs64.TexTransCore
{
    /// <summary>
    /// ディスクと言っているが、圧縮済みなテクスチャーの事だったり、ディスクの方にソースデータがあるものである。
    /// 基本的に Engin が Read 可能だが　CPU での操作はできない。
    /// 基本的に圧縮されている前提なので Load する必要がある
    ///
    /// Loadされるときの Width Heigh を返す必要がある。
    /// </summary>
    public interface ITTDiskTexture : ITTTexture
    {
    }


}
