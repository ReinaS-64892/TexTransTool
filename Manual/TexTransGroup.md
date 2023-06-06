# TexTransGroup について

## このコンポーネントの概要

このコンポーネントは TextureTransformers に入った TextureTransformer を一括で compile したり
Apply したりすることができ、上から順に実行されていくため、UV に変更を与える TextureTransformer(AtlasSet など)の後に実行するなどを行うためにあります。

## 使い方

### 始めに

TexTransTool/Runtime にある TexTransGroupAvatarTag.cs から、
またはインスペクターのコンポーネントを追加の TexTransTool/TexTransGroup から
ゲームオブジェクトに追加できます。

### 一斉に実行する方法

- 一斉に実行したい TextureTransformer を TextureTransformers という配列のプロパティにセット

Compile でそれらすべてを上から順に compile し、
Apply でそれらすべてのプレビューができます。

## プロパティ

### TextureTransformers

TextureTransformer の配列で、ここにセットしたものが一斉の compile や Apply の対象となります。

ただし、それらTextureTransformerのついたGameObjectが無効化されている場合、無視されます。