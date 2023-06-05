# TexTransTool

このツールは非破壊的にテクスチャの転写・並び替え・合成などを行うためのツールです。

## How to use

### Install
[最新のリリース](https://github.com/SASIKI-64892/TexTransTool/releases/latest)からunitypackageをダウンロードでき、unityにインポートできます。

### Modification Setup

改変したいように[SimpleDecal](Manual/SimpleDecal.md)や[AtlasSet](Manual/AtlasSet.md)などを設定し、

適当なGameObjectに[TexTransGroup](Manual/TexTransGroup.md)を追加し、前述で生成した[SimpleDecal](Manual/SimpleDecal.md)や[AtlasSet](Manual/AtlasSet.md)をTextureTransformersに追加し、表示されているCompileを実行します。(グレーアウトして実行できない場合追加したもののどれかがCompileできない場合なので、できる状態になるように設定するか、TextureTransformersから削除してください。)

VRChatアバターの場合は[AvatarBuildAppryHook](Manual/AvatarBuildAppryHook.md)を[TexTransGroup](Manual/TexTransGroup.md)を付けたGameObjectに追加。

そうでない場合は[AvatarMaterialDmain](Manual/AvatarMaterialDmain.md)を[TexTransGroup](Manual/TexTransGroup.md)を付けたGameObjectに追加。

### Build And Appry

VRChatアバターの場合、アバター配下に前述で生成した[AvatarBuildAppryHook](Manual/AvatarBuildAppryHook.md)がある場合はそのまま、ない場合はアバター配下に移動してから VRChat SDKから「Build & Publich for ~ 」を行えば、適応されたものがアップロードされます。

そうでない場合は前述で生成した[AvatarMaterialDmain](Manual/AvatarMaterialDmain.md)の「MaterialDomainUse - Appry」を実行すれば適応でき、任意の何かしらにご使用ください。

## Feature

### SimpleDecal

シンプルなデカールを行うためのコンポーネントです。[詳細はこちら](Manual/SimpleDecal.md)

### AtlasSet

指定した GameObject とその子に含まれる Renderer のマテリアルのメインテクスチャをアトラス化することのできるコンポーネントです。[詳細はこちら](Manual/AtlasSet.md)

### TextureBlender

画像編集ツールのレイヤー機能のように画像を合成できるコンポーネントです。[詳細はこちら](Manual/TextureBlender.md)

### TexTransGroup

SimpleDecal や AtlasSet をまとめて Compile や Appry などを行うことのできるコンポーネントです。[詳細はこちら](Manual/TexTransGroup.md)

### AvatarBuildAppryHook

TexTransGroupとセットで使用し、アバターをビルドするときにAppryし、非破壊的に変更を加えるためのコンポーネントです。[詳細はこちら](Manual/AvatarBuildAppryHook.md)

## Experimental Features

### CylindricalCurveDecal

腕など円柱状のものに対してカーブ上のデカールを生成できるコンポーネントです。[詳細はこちら](Manual/CylindricalCurveDecal.md)
