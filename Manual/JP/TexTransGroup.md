# TexTransGroup について

## このコンポーネントの概要

このコンポーネントは TextureTransformers に入った [TextureTransformer](TextureTransformer.md) を一括で Preview することができるコンポーネントです。

基本的に上から順に Preview され、[TextureTransformer](TextureTransformer.md) のついた GameObject が無効化されている場合、無視されます。

## 使い方

### 始めに

TexTransTool/TexTransGroup/Runtime にある TexTransGroup.cs から、
またはインスペクターのコンポーネントを追加の TexTransTool/TexTransGroup から
ゲームオブジェクトに追加できます。

### 一斉に実行する方法

- 一斉に実行したい [TextureTransformer](TextureTransformer.md) を TextureTransformers という配列のプロパティにセット

Preview でそれらすべてのプレビューができます。

## プロパティ

### TextureTransformers

[TextureTransformer](TextureTransformer.md) の配列で、ここにセットしたものが Preview の対象となります。

