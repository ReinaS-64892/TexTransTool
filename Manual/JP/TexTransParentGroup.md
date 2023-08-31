# TexTransParentGroup について

## このコンポーネントの概要

このコンポーネントは 子の GameObject の [TextureTransformer](TextureTransformer.md)を一括で
Apply することができるコンポーネントです。

基本的に上から順に Apply されますが、[TextureTransformer](TextureTransformer.md) のついた GameObject が無効化されている場合、無視されます。

## 使い方

### 始めに

TexTransTool/TexTransGroup/Runtime にある TexTransParentGroup.cs から、
またはインスペクターのコンポーネントを追加の TexTransTool/TexTransParentGroup から
ゲームオブジェクトに追加できます。

### 一斉に実行する方法

- 一斉に実行したい TextureTransformer 子のオブジェクトにする。

Apply でそれらすべてのプレビューができます。
