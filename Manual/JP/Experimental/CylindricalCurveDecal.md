# CylindricalCurveDecal について

# **このコンポーネントは開発中で実験的です**

## このコンポーネントの概要

SimpleDecal の派生で、カーブ状にデカールを張ることができ、腕などの円柱状のものに適していて、逆に円柱はない場所に差し掛かると爆発するのでご注意ください。

## 使い方

### 始めに

TexTransTool/Runtime/Decal/Curve/Cylindrical にある CylindricalCurveDecal.cs から、
またはインスペクターのコンポーネントを追加の TexTransTool/Experimental/CylindricalCurveDecal から
ゲームオブジェクトに追加できます。

### カーブデカールの張り方

- 対象となる TargetRenderer をセット
- DecalTexture をセット
- 必要であれば UseFirsAndEnd にチェックを入れ夫々をセット
- 腕の中心などに配置した [CylindricalCoordinatesSystem](../CylindricalCoordinatesSystem.md)をセットします
- カーブとなる[セグメント](CurveSegment.md)を作成して Segments に追加し、セグメントの位置やロールを調整
- サイズとループカウントをお好みの値に設定

Apply ボタンを押すとそのカーブデカールをプレビューすることができます。

## プロパティ

### TargetRenderer

ターゲットとなるレンダラーをセットするプロパティ。

### UseFirstAndEnd

このチェックを入れると、カーブデカールの最初と最後だけデカールのテクスチャを変更することができます。

#### FirstTexture

最初に使用されるテクスチャーをセットするプロパティ。

#### EndTexture

最後に使用されるテクスチャーをセットするプロパティ。

### DecalTexture

貼り付けるデカールをセットするプロパティ。

### TargetPropertyName

デカールを張るテクスチャーをマテリアルのどのプロパティにするかを選択するプロパティです。

### Segments

ベジュ曲線のセグメントとなる[CurveSegment](CurevSegment.md)をセットする配列のプロパティ。

### CylindricalCoordinatesSystem

円柱座標系を定義する[CylindricalCoordinatesSystem](CylindricalCoordinatesSystem.md)の参照のプロパティ。

### Size

カーブのに沿っていくデカールの一マスのサイズのプロパティ。

### Loop Count

カーブのに沿っていくデカールのマス目数のプロパティ。

### Roll Mode

カーブの傾きの計算方法を指定するプロパティ。

### Draw Gizmo Always

ベジュ曲線やマス目のギズモを選択中でないときも表示するチェックのプロパティ。

セグメントの位置調整を行う時などにチェックを入れると見やすくなります。
