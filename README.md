# TexTransTool

このツールは非破壊的にテクスチャの転写を行うためのツールです。

## Feature

### SimpleDecal

シンプルなデカールを行うためのコンポーネントです。[詳細はこちら](Manual/SimpleDecal.md)

### AtlasSet

指定した GameObject とその子に含まれる Renderer のマテリアルのメインテクスチャをアトラス化することのできるコンポーネントです。[詳細はこちら](Manual/AtlasSet.md)

### TexTransGroup

SimpleDecal や AtlasSet をまとめて Compile や Appry などを行うことのできるコンポーネントです。[詳細はこちら](Manual/TexTransGroup.md)

### AvatarBuildAppryHook , AvatarMaterialDmain

SimpleDacal などを使用するときアバターの総マテリアル数が増えるのを防ぐことができ、TexTransGroup とセットで使用するコンポーネントです。詳細はこちら[AvatarBuildAppryHook](Manual/AvatarBuildAppryHook.md)、[AvatarMaterialDmain](Manual/AvatarMaterialDmain.md)

## Experimental Features

### CylindricalCurveDecal

腕など円柱状のものに対してカーブ上のデカールを生成できるコンポーネントです。[詳細はこちら](Manual/CylindricalCurveDecal.md)