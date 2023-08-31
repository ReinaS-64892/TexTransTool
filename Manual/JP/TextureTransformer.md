# TextureTransformer について

まず、使用可能なコンポーネントの TextureTransformer は存在しないのですが、このツールのコンポーネントのほとんどはこの TextureTransformer を継承し、TextureTransformer として扱われます。

つまりこのツールのほとんどのコンポーネントに共通する事柄をここでは取り扱います。

## TextureTransformer は一つの GameObject に一つまで、

例えば、[SimpleDecal](SimpleDecal.md)はつけられたゲームオブジェクトのスケールを直接調整したりすることがあります。この時ほかの Decal の類のコンポーネントがついていると正常な挙動になりませんし、[TexTransParentGroup](TexTransParentGroup.md)は GameObject 一つ目の　TextureTransformer だけを実行します。
