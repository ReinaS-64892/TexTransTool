# TexTransTool

このツールは非破壊的でデカールによる直感的なテクスチャの貼り付けや色改変、アトラス化による VRAM 削減ができるツールです！

[English readme](README_EN.md)

## Install

VRChatAvatar で使用する場合は VPM を推奨しています。[Add-VPM-Link](https://vpm.rs64.net/add-repo)

それ以外は[最新のリリース](https://github.com/SASIKI-64892/TexTransTool/releases/latest)から。

## Tutorial

### Init Setup

アバター直下に新しい GameObject を生成し、[TexTransParentGroup](Manual/JP/TexTransParentGroup.md)と[AvatarDomainDefinition](Manual/JP/AvatarDomainDefinition.md)追加してください。

### Modification Setup

前述で生成した GameObject の子に GameObject を生成し、AddComponent しましょう

スタンプのようなデカールや髪の毛などにグラデーションを入れたい場合[こちら](Manual/JP/SimpleDecal.md)

テクスチャーのアトラス化による VRAM 削減は[こちら](Manual/JP/AtlasTexture.md)

## Main Features

### SimpleDecal

シンプルなデカールを行うためのコンポーネントです。[詳細はこちら](Manual/JP/SimpleDecal.md)

### AtlasTexture

指定したマテリアルのメインテクスチャーをアトラス化できるコンポーネントです。[詳細はこちら](Manual/JP/AtlasTexture.md)
