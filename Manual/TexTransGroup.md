# TexTransGroup について

## このコンポーネントの概要

このコンポーネントは TextureTransformers に入った TextureTransformer を一括で compile したり
Appry したりすることができ、上から順に実行されていくため、UV に変更を与える TextureTransformer(AtlasSet など)の後に実行するなどを行うためにあります。

## 使い方

### 始めに

TexTransTool/Runtime/VRCBulige にある TexTransGroupAvatarTag.cs から、
またはインスペクターのコンポーネントを追加の TexTransTool/TexTransGroup から
ゲームオブジェクトに追加できます。

### 一斉に実行する方法

- 一斉に実行したい TextureTransformer を TextureTransformers という配列のプロパティにセット

Compile でそれらすべてを上から順に compile し、
Appry でそれらすべてのプレビューができます。

## プロパティ

### TextureTransformers

TextureTransformer の配列で、ここにセットしたものが一斉の compile や Appry の対象となります。
