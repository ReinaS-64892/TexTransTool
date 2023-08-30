# AvatarDomainDefinition について

## このコンポーネントの概要

このコンポーネントは、各々の [TextureTransformer](TextureTransformer.md) が対象としているレンダラーのマテリアルしか変えれず、アバターとしての総マテリアル数(マテリアルスロット数ではない)やテクスチャーの枚数が増加してしまうのを防ぐ機能と、VRChatAvatar の場合は Build 時に適応する[TexTransGroup](TexTransGroup.md)を指定するマーカーの機能も持ちます。

これは、これまで各々の対象のレンダラーまでしか影響できなかったものが他のレンダラーにも影響するようになるため、[TextureBlender](TextureBlender.md)などは大きな違いが出るようになります。

## 使い方

### 始めに

TexTransTool/Runtime/Build にある AvatarDomainDefinition.cs から、
またはインスペクターのコンポーネントを追加の TexTransTool/AvatarDomainDefinition から
ゲームオブジェクトに追加できます。

## プロパティ

### Generate Custom Mip Map

このツール独自のミップマップを生成するプロパティで、UVの境目で、色がにじまないMipMapを生成します。

ただし、MipMapの生成はあまり早くないので、にじむのが気になる場合にのみチェックを入れましょう。

### PreviewAvatar

プレビューの時アバターの範囲となる GameObject を指定するプロパティです。

