# TexTransTool

このツールは非破壊的にテクスチャの転写を行うためのツールです。

## Feature

### SimpleDecal

シンプルなデカールを行うためのコンポーネントです。[詳細はこちら](Manual/SimpleDecal.md)

### AtlasSet

指定した GameObject とその子に含まれる Renderer のマテリアルのメインテクスチャをアトラス化することのできるコンポーネントです。[詳細はこちら](Manual/AtlasSet.md)

### TexTransGroup

SimpleDecal や AtlasSet をまとめて Compile や Appry などを行うことのできるコンポーネントです。[詳細はこちら](Manual/TexTransGroup.md)

### AvatarBuildAppryHook

TexTransGroupとセットで使用し、アバターをビルドするときにAppryし、非破壊的に変更を加えるためのコンポーネントです。[詳細はこちら](Manual/AvatarBuildAppryHook.md)

## Experimental Features

### CylindricalCurveDecal

腕など円柱状のものに対してカーブ上のデカールを生成できるコンポーネントです。[詳細はこちら](Manual/CylindricalCurveDecal.md)