# AvatarMaterialDmain について

## このコンポーネントの概要

このコンポーネントは、各々の TextureTranformar が対象としているレンダラーのマテリアルしか変えれず、アバターとしての総マテリアル数(マテリアルスロット数ではない)が増加してしまうのを防ぐためのコンポーネントです。

## 使い方

### 始めに

TexTransTool/Runtime にある AvatarMaterialDmain.cs から
ゲームオブジェクトに追加できます。

### MaterialDomainUse - Appry

このコンポーネントに表示されている、この Appry を使用すると、MaterialDmain を使用し、TextureTranformar がマテリアルを変更するとき、TextureTranformar が参照しているレンダラー以外のマテリアルも同時に変更されるようになります。

## プロパティ

### Avatar

アバターの範囲となる GameObject をセットできるプロパティです。

各々の TextureTranformer がマテリアルを変更したときの影響範囲がこの GameObject の配下のレンダラーになります。

### TexTransGrop

TexTransGrop の参照をセットできるプロパティです。
