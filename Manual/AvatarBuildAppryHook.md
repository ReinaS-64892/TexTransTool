# AvatarBuildAppryHook について

## このコンポーネントの概要

VRChat のアバタービルド時のコールバックを受けるためにあるコンポーネントで、そのコールバックを受けたとき参照している TexTransGrop を Appry します。

そして、Appry は[AvatarMaterialDmain](AvatarMaterialDmain.md)と同じものとなり、

非破壊的に適応するためにはそのアバターには一つは必須となります。（これが存在しない場合は適応されません。）

## 使い方

### 始めに

TexTransTool/Runtime/VRCBulige にある AvatarBuildAppryHook.cs から、
またはインスペクターのコンポーネントを追加の TexTransTool/AvatarBuildAppryHook から
ゲームオブジェクトに追加できます。

## プロパティ

### TTGAvatarTag

TexTransGrop の参照をセットできるプロパティです。
