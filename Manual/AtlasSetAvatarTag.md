# AtlasSetAvatarTag について

## 使い方

#### 始めに

Rs/TexturAtlasCompiler/VRCBulige にある AtlasSetAvatarTag.cs から、
またはインスペクターのコンポーネントを追加のTexturAtlasCompiler/AtlasSetAvatarTag から
ゲームオブジェクトに追加できます。

#### アトラス化の仕方

アトラス化したい対象を AtlasTargetMeshs(AtlasTargetStaticMeshs)に追加して、
TexturAtlasCompile! ボタンをすとアトラス化しされたテクスチャを生成でき、
Appry ボタンを押すとそのアトラス化されたテクスチャに変えることができます。

そしてそのコンポーネントがアバター配下にある場合はアバターのビルド時に自動でAppryされた状態になります。

#### アトラス化する対象について

AtlasTargetMeshs(AtlasTargetStaticMeshs)に追加したRendererに含まれるマテリアルのテクスチャすべてが対象となります。
今のところ特定のマテリアルだけ、などの指定はできないのでご注意ください。




## プロパティ

#### AtlasTargetMeshs & AtlasTargetStaticMeshs

SkindMeshRenderer(MeshRenderer)の配列で、この配列に入れた SkindMeshRenderer(MeshRenderer)がアトラス化の対象になります。

#### AtlasTextureSize

アトラス化したテクスチャの解像度で X が横 Y 縦になります。

#### Pading

アトラス化したテクスチャの UV の外側にどれくらいテクスチャを広げるかの数値です。

#### PadingType

Pading の計算方法を指定します。

EdgeBase - 一番近い辺のポイントからの距離
VartexBase - 一番近い頂点からの距離

#### ClientSelect

アトラス化などのコンパイルの処理を何で行うかを指定します。

CPU - CPU ですべてを行います。(非推奨)
AsyncCPU - CPU で非同期的に行います。(何かしらで ComputeSheder が使えない環境以外は非推奨)
ComputeSheder - ComputeSheder でできる部分はそれで行いそのほか CPU で非同期的に行います。

#### SortingType

アトラス化するときの自動生成 UV の並べ方を指定します。

EvenlySpaced - 適当な順番でマス目にしたがって並べます。(非推奨)
NextFitDecreasingHeight - 高さ順に左下から敷き詰めます。

#### Contenar

アトラス化、コンパイルしたデータの参照です。

この時アバター配下のゲームオブジェクトに追加した場合、アバターのビルド時に自動的に追加されます。

#### PostProsess

コンパイルしたテクスチャに解像度を指定する、などの設定を指定する配列。


