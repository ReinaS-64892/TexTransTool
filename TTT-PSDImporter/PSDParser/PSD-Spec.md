# ReinaS' PSD Specification

これは、私(Reina_Sakiria) が C# で 独自の PSD Parser を書く時に得た知見、そして PSD がいかに adobe の脳内でしかなく、非常に扱いずらい存在であるかのメモ書きです。

基本的には元の Spec と同じですが、パーサーを書く時に得た知見を基に再構築しています。

途中で刺しこまれる疑似コードは基本的に C# をベースに描かれているのでご注意ください。

[不完全なSpec](https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/)

## 基本構造

- FileHeader
- ColorModeData
- ImageResources
- LayerAndMaskInformation
- ImageData

の五つ、これは基本的には変わらない。

これらが上から順に並んでいる。

ちなみにほとんどの数値などは BigEndian で記述されています。

この Spec では指定されていない場合は unsigned (符号なし) として数字を扱います。

## FileHeader

固定長で特に踏み抜きやすい罠はない。

|Byte|Description|
|---|---|
|4(ASCII-String)|シグネチャ: "8BPS" という 恐らく ASCII で シグネチャが確定で記述されている。これに一致しなかった場合は読み取らないように、という警告が元のSpecにも記述されている。 |
|2(ushort)|バージョン: PSD だと 1 、PSB だと 2 になっている。上と同様にそれぞれサポートしていないなら読み取らないように|
|6|予約済み: 必ずゼロで埋める必要がある領域のようだ|
|2(ushort)|ChannelCount: 含まれるチャンネル数の数、通常の RGB+A を持つ場合は 4。 1 ~ 56 間でサポートされている。[ImageData](#imagedata) に入っている色のチャンネル数となる。|
|4(uint)|キャンバスサイズの縦幅: ここを読み取れば PSD の Height がわかる。 1 ~ 30000 まで、 PSB だと最大 300000|
|4(uint)|キャンバスサイズの横幅: ここを読み取れば PSD の Width がわかる。1 ~ 30000 まで、 PSB だと最大 300000|
|2(ushort)|ビット深度: [後述](#bitdepth) |
|2(ushort-Enum)|カラーモード: [後述](#colormode) |

### BitDepth

一つのチャンネルが持つ Bit 数。

|Value|Name|Type|
|---|---|---|
|1|1Bit|bit|
|8|8Bit|byte|
|16|16Bit|ushort|
|32|32Bit|float|

特筆すべき点は 32Bitの時だけ float になる。
後々に、レイヤーの画像データを引き出すときに影響します。

### ColorMode

|Value|Name|Description|
|---|---|---|
|0|BitMap|謎 1BitPSDの場合にこれが割り当たっているようだ|
|1|GrayScale|謎 情報求む|
|2|Indexed|謎 情報求む|
|3|RGB|一般的な PSD はこれ、通常の RGB|
|4|CMYK|CMYKカラーで格納された場合なのかもしれないが謎 情報求む|
|5|Unknown||
|6|Unknown||
|7|MultiChannel|謎 情報求む|
|8|Duotone|謎 情報求む|
|9|Lab|謎 情報求む|

## ColorModeData

上記 FileHeader の ColorMode と関係があるようだ。

|Byte|Description|
|---|---|
|4(uint)|ColorModeData-Length: この 4Byte(uint) の後に続く全データの長さ|
|Variable = ColorModeData-Length|ColorData: カラーモードデータ、[後述](#colormodedataの詳細)の場合以外はゼロ。 |

### ColorModeDataの詳細

元の Spec によると FileHeader.ColorMode の値が

- 2-Indexed
- 8-Duotone

の場合に、情報を持つようです。

それ以外の場合はこの領域は存在しないので、ColorModeData-Length を読み取ったら[次](#imageresources)に移ってよい。

#### IndexedColor

元の Spec によると 768 の長さを持ち、画像のカラーテーブルが、 非インターリーブ順序で含まれているらしいが...謎。
情報求む。

#### DuotoneColor

この形式に対する明文化された Spec はないらしい...は???

情報求む。

ほかのアプリケーションは、 グレースケールとして扱い。 個々のデータを維持すれば良いと元の Spec には記述されている。

## ImageResources

この PSD に添加される情報。

出力元のソフトウェアにに応じてかなり入っている内容が変わる。

|Byte|Description|
|---|---|
|4(uint)|ImageResources-Length: この 4Byte(uint) の後に続く全データの長さ|
|Variable = ImageResources-Length|ImageResourceBlockArray: ImageResources-Length の長さが尽きるまで、 ImageResourceBlock がゼロから複数個並んでいる。[後述](#imageresourceblock)|

### ImageResourceBlock

それぞれ様々な情報が埋め込まれているようです。
場合によっては Yaml が埋め込まれていて、出力元の Photoshop のバージョンやOSが書き込まれていることもあるようです。

罠がある(主にPascal-String)ので気を付けてください....

|Byte|Description|
|---|---|
|4(ASCII-String)|Signature: "8BIM" のシグネチャ、特に特筆すべき項目はなし。|
|2(ushort)|ImageResourceID: [後述参照](#imageresourceid)|
|Variable = [後述](#pascalstring-imageresourceblockname)|PascalString-ImageResourceBlockName: パスカル文字列で、この ResourceBlock の名前が格納されています。 非常にわかりずらい罠があるので読み取るときは非常に気を付けて！ [後述参照！](#pascalstring-imageresourceblockname)|
|4(uint)|ActualDataSizeFollows: ここに記述される値は正しくない。[後述参照！](#actualdatasizefollows)|
|Variable = [ActualDataSizeFollows 後述参照！](#actualdatasizefollows)|ResourceData: 内容は個々のリソースタイプのセクションで説明されているようだ。[元スペック参照](https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/#50577409_38034)|

#### ImageResourceID

さすがに多すぎるし、検証されていないため、元 Spec 参照。

<https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/#50577409_38034>

#### PascalString-ImageResourceBlockName

Pascal String で 名前が格納されてい、文字コードは 日本語圏の PSD では "shift-jis" が使われているようですが、他言語圏でどのような文字コードになっているかは謎です。

他文字コードだった場合にここの情報が誤りになる可能性もあります。

|Byte|Description|
|---|---|
|1(byte)|PascalStringLength: パスカル文字列の長さ|
|Variable != PascalStringLength| 下記参照 |

PascalStringLength の長さで、文字列としては読み取ってよい。

そして、以下のような感じで null 文字が存在します...

```csharp
if (PascalString-Length == 0) { /* null が 1byte 存在します。*/ }  
else if ( ( PascalString-Length % 2 ) == 0) { /* null が 1byte 存在します。*/ }  
else { /* null が 存在しません。*/ }
```

つまり、 長さゼロの時 or 長さが偶数の時にnull文字が 1byte 存在します。

気を付けてください....

#### ActualDataSizeFollows

[元の Spec](https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/#50577409_46269) には 「サイズを均一にするためのパディングが入っています(かなり適当な訳)」と記述されていますが、半分誤りで二の倍数にするためのパディングが入っています。

なので二の倍数に丸める必要があり、具体的には以下のようにする必要があります。

```csharp
var trueLength = (ActualDataSizeFollows % 2 ) == 0  ? ActualDataSizeFollows : ActualDataSizeFollows + 1 ;
```

## LayerAndMaskInformation

PSD の一番重要な レイヤー情報やそのレイヤーが持つピクセル情報などが含まれる一番重要なセクションです！

一番複雑でもありますが...

|Byte|Description|
|---|---|
|PSD 4(uint) or PSB 8(ulong)|LayerAndMaskInformationLength: 下記三つのの info すべての長さ。|
|Variable = [後述参照](#layerinfo)|LayerInfo: レイヤー情報。 [後述参照](#layerinfo)|
|Variable = [後述参照](#globallayermaskinfo)|GlobalLayerMaskInfo: [後述参照](#globallayermaskinfo)|
|Variable = Unknown|CanvasType-AdditionalLayerInformationArray: レイヤー追加情報がキャンバスに対して付与されることがあるようです...は???  恐らく LayerAndMaskInformationLength が尽きるまで並んでいる。[後述参照](#additionallayerinformation)|

このセクションには LayerInfo だけが含まれている場合もあるので、長さが尽きたら GlobalLayerMaskInfo や CanvasType-AdditionalLayerInformationArray を読み始めてしまわないように注意！

### LayerInfo

|Byte|Description|
|---|---|
|PSD 4(uint) or PSB 8(ulong)|LayerInfo-Length: この長さ以外のレイヤーインフォメーション全体の長さ。 ここがゼロだった場合後に続く情報はないので切り上げる必要がある。 (二の倍数に切り上げられるようだが...謎、少なくとも Readする場合には特に何も影響しないらしい...?) |
|2(short)|LayerCount:この値がマイナスだった場合 Alphaチャンネルが最初に含まれるとのことらしい...恐らく。この値を絶対値にすることでにレイヤーの数を知ることができる。|
|Variable = Abs(LayerCount) \* [LayerRecode](#layerrecode) | LayerRecodeArray: LayerRecodeが LayerCount の絶対値分だけ並んでいます。 [後述参照](#layerrecode)|
|Variable = Abs(LayerCount) \* (Maybe)FileHeader.ChannelCount \* [ChannelImageData](#channelimagedata) |ChannelImageDataArray: レイヤーのラスターデータがすべて並んでいます。[後述参照](#channelimagedata)|

#### LayerRecode

レイヤー一つの情報が入っている。

|Byte|Description|
|---|---|
|16([RectTangle](#recttangle))|RectTangle: [後述](#recttangle)|
|2(ushort)|LayerInChannelCount: 下記 ChannelInformation の数。FileHeader.ChannelCount とは特に一致しない場合もある。|
|6([ChannelInformation](#channelinformation)) \* LayerInChannelCount|ChannelInformationArray: [ChannelInformation](#channelinformation) がチャンネル数だけ並んでいます。詳細は[後述参照](#channelinformation) (ただ、ここに記述される [ChannelID](#channelid) がすべてユニークである保証はないようです。治安の悪い PSD を出力する実装が存在するだけの可能性もありますが...) |
|4(ASCII-String)|Signature: "8BIM" |
|4(ASCII-String)|BlendModeKey: そのレイヤーの合成モード。[後述](#blendmodekey)|
|1(byte)|Opacity: 0 が完全な透明、 255 が不透明|
|1(byte)|Clipping: ただの bool で、 0 はクリッピングを行っていない、1 はクリッピングをすするレイヤー|
|1([byte-FlagEnum](#layerflag))|LayerFlag: [後述](#layerflag)|
|1(byte)|Filler: 0 です。|
|4(uint)|ExtraDataFieldLength: これから後に続くすべての情報の長さの全体調|
|Variable = [後述](#layermaskadjustmentlayerdata)|LayerMaskAdjustmentLayerData: レイヤーマスクや調整情報データーのフィールド [後述](#layermaskadjustmentlayerdata) |
|Variable = [後述](#layerblendingrangedataarray)|LayerBlendingRangeDataArray: [後述](#layerblendingrangedataarray)|
|Variable = [後述](#pascalstring-layername)|PascalsString-LayerName: [後述](#pascalstring-layername)|
|Variable = Unknown * [後述](#additionallayerinformation)|AdditionalLayerInformationArray: [レイヤーの追加情報](#additionallayerinformation)が、 ExtraDataLength の限界まで続きます。|

##### RectTangle

|Byte|Description|
|---|---|
|4(int)|Top:|
|4(int)|Left:|
|4(int)|Bottom:|
|4(int)|Right:|

PSD の座標系は 左上原点、右下が 1 になるようになっている。

なので、2048 × 2048 のキャンバスに全体に張り付くような矩形の値は

Top = 0,
Left = 0,
Bottom = 2048,
Right = 2048,

であることに注意すること。

##### ChannelInformation

|Byte|Description|
|---|---|
|2([short-Enum](#channelid))|ChannelID: [後述参照](#channelid)|
|PSD 4(uint) or PSB 8(ulong)|CorrespondingChannelDataLength: 対応するチャンネルデータの圧縮された状態での長さ。|

###### ChannelID

ただの 符号付き列挙型。以下のような値が割り当たっています。

- Red = 0
- Green = 1
- Blue = 2
- Transparency = -1
- UserLayerMask = -2
- RealUserLayerMask = -3 (ユーザーの作ったマスクとベクターマスクが存在する場合要らしい...謎)

ただし、これが FileHeader.ColorMode が RGB の時であり、ほかの場合がどうなるかは謎です。情報求む！

##### LayerFlag

それぞれの bit がこのように対応している。

- TransparencyProtected = 1 (0bit)
- NotVisible = 2 (1bit)
- Obsolete = 4 (2bit)
- UsefulInformation4Bit = 8 (3bit)
- NotDocPixelData = 16 (4bit)

2bit 廃止...謎

3bit 4bit目にある情報に意味があるということを示唆するようだが...

4bit の情報は謎...元のSpecにそれらしい情報は私には見つけられなかった。

##### LayerMaskAdjustmentLayerData

|Byte|Description|
|---|---|
|4(uint)|SizeOfData: これが 0 だった場合先に続くフィールドは存在しないから切り上げる必要がある。|
|16([RectTangle](#recttangle))|RectangleForLayerMask: レイヤーマスクの情報を持つ範囲を示す矩形。この矩形の範囲外は下の DefaultColor で埋められることを想定する。|
|1(byte)|DefaultColor: このマスクのデフォルトカラーで 0 ~ 255|
|1([byte-FlagEnum](#maskoradjustmentflag))|MaskOrAdjustmentFlag: [後述](#maskoradjustmentflag)|
|Variable = MaskOrAdjustmentFlag.UserOrVectorMasksHave ? [後述](#maskparametersfragandfollows)  : 0 |MaskParametersFragAndFollows:  MaskOrAdjustmentFlag の 4bit が有効な場合にのみ存在する。 [後述](#maskparametersfragandfollows)|
|Variable = SizeOfData == 20 ? 20になるまで : 0 |SizeOfData が 20 の場合に、ここに Padding が存在します。(元の Spec には 2byte 存在すると表記されているが、正しくは ここまで消費した byte 数を加味しして Padding を詰める必要がある。(具体的には MaskOrAdjustmentFlag 4bit が有効な場合は MaskParametersFragAndFollows が 1byte 消費するので 1byte そうではない場合は 2byte 詰めることになるだろう。))|
|1(byte-FragEnum)|RealFlag: [MaskOrAdjustmentFlag](#maskoradjustmentflag) と同じらしい。|
|1(byte)|ReadUserMaskBackground: 謎|
|16([RectTangle](#recttangle))|RealRectTangleLayerMask: 謎|

###### MaskOrAdjustmentFlag

- PosRelToLayer = 1 (0bit)
- MaskDisabled = 2 (1bit)
- InvertMask = 4 (2bit)
- UserMaskActuallyCame = 8 (3bit)
- UserOrVectorMasksHave = 16 (4bit)

それぞれ、

- レイヤーに対する相対的な座標であるか
- レイヤーマスクが無効化されているか
- 合成時にマスクを反転するか (廃止)
- ユーザーマスクが、ほかのデータからレンダリングされたものか
- ユーザーマスクかベクターマスクに追加のパラメーターが適用されているか

になっているようだが...あまりよくわからない...謎。

###### MaskParametersFragAndFollows

先に MaskParametersFrag の情報

- UserMaskDensity = 1 (0bit)
- UserMaskFeather = 2 (1bit)
- VectorMaskDensity = 4 (2bit)
- VectorMaskFeather = 8 (3bit)

それぞれ density または feather が UserMask(恐らく通常のマスク) または VectorMask(謎) にかかる値のが後に存在するか否かのフラグ。

|Byte|Description|
|---|---|
|1(byte-FragEnum)|MaskParametersFrag: 後に続く情報のそれぞれを持つか否かのフラグ|
|Variable = MaskParametersFrag.UserMaskDensity ? 1(byte) : 0 |UserMaskDensity:|
|Variable = MaskParametersFrag.UserMaskFeather ? 8(double) : 0 |UserMaskFeather:|
|Variable = MaskParametersFrag.VectorMaskDensity ? 1(byte) : 0 |VectorMaskDensity:|
|Variable = MaskParametersFrag.VectorMaskFeather ? 8(double) : 0 |VectorMaskFeather:|

まぁ...つまり、一つもフラグがついてなかったら読み取るものはないし、
1bit と 2bit だけ有効だったら UserMaskFeather の double と VectorMaskDensity の byte が並んでるってことだね！

たぶんね！

##### LayerBlendingRangeDataArray

|Byte|Description|
|---|---|
|4(uint)|LayerBlendingRangeDataArrayLength: [LayerBlendingRangeData](#layerblendingrangedata) が後にどれだけ並んでいるかを示す|
|Variable = 8([LayerBlendingRangeData](#layerblendingrangedata)) * (LayerBlendingRangeDataArrayLength / 8)| [LayerBlendingRangeData](#layerblendingrangedata) が LayerBlendingRangeDataArrayLength を尽きるまで並んでいる。|

###### LayerBlendingRangeData

|Byte|Description|
|---|---|
|1(byte)|CompositeGrayBlendSourceBlack1: 合成グレーブレンド元...? 謎だけど黒側が入っているらしい|
|1(byte)|CompositeGrayBlendSourceBlack2: 上の二つ目の値|
|1(byte)|CompositeGrayBlendSourceWhite1: 謎だけど 白側の値が入っているらしい|
|1(byte)|CompositeGrayBlendSourceWhite2: 謎だけど 白側二つ目の値が入っているらしい|
|4(uint)|CompositeGrayBlendDestinationRange: 合成グレーブブレンド先範囲...? 謎|

##### PascalString-LayerName

4の倍数で丸められている パスカル文字列、 リソースブロックの方とは違うから気を付けて。

文字コードは 日本語圏の PSD では "shift-jis" が使われているようですが、他言語圏でどのような文字コードになっているかは謎。

他文字コードだった場合にここの情報が誤りになる可能性もある。

|Byte|Description|
|---|---|
|1(byte)|PascalStringLength: パスカル文字列の長さ|
|Variable != PascalStringLength| 下記参照 |

PascalStringLength の長さで、文字列としては読み取ってよい。

そして、以下のような感じで null 文字が存在します。

```csharp
var readLength = PascalStringLength + 1;//1は長さを示す部分との帳尻合わせ。  
if ((readLength % 4) != 0)  
{  
    var paddingLength = 4 - (readLength % 4);  
    // paddingLengthの分だけ null がある。  
}
```

実装するときはくれぐれも気を付けて。

#### ChannelImageData

FileHeader.BitDepth と ChannelImageData.Compression に応じて中身や展開方法が異なる。

展開方法は[後述参照](#compressedimagedata)。

|Byte|Description|
|---|---|
|2(ushort-Enum)|Compression: [後述](#compression)|
|Variable = LayerRecode.ChannelInformationArray\[Unknown\].CorrespondingChannelDataLength - 2 |CompressedImageData: [後述](#compressedimagedata)|

##### Compression

以下のようになっています。展開後のサイズの予測は[後述参照](#compressedimagedata)

RawData = 0
RLECompressed = 1
ZIPWithoutPrediction = 2
ZIPWithPrediction = 3

### GlobalLayerMaskInfo

|Byte|Description|
|---|---|
|4(uint)|GlobalLayerMaskInfoLength: このセクションの長さ。 0 だったら切り上げてよいだろう。|
|2(?)|OverlayColorSpace: 情報が全くなく...謎。情報求む|
|2(?) * 4| 2Byte 長の色情報らしい。それ以上の情報がなく、謎。情報求む |
|2(?)|Opacity: 0 だったら透明 100 だったら不透明のようだが...なぜ 2byte なのかが謎で、何もわからない。(多分 ushort として読み取るべきなのだろうか...?)|
|1(?)|kind: 0 だと色が選択されている、つまり反転。 1だと色の保護。 128 だとレイヤーごとの色を使用する...などと書かれているが謎。情報求む。|

## AdditionalLayerInformation

レイヤー追加情報の大枠の仕様はこのような感じ

|Byte|Description|
|---|---|
|4(ASCII-String)|Signature: "8BIM" or "8B64" (私は "8BIM" しか見たことがないが、 "8B64" もあるらしい)|
|4(ASCII-String)|Key: その追加情報がなんであるかを識別できるキーコード。|
|4(uint) or 一部例外 8(ulong) |Length: この追加情報の持つ任意の情報の長さ。レイヤーレコードに付属する追加情報の場合はそのまま使用できるが、キャンバスに付属するタイプは4の倍数に切り上げられるようです。|
|Variable = Length|Data: Key に応じた任意のデータが入っています。|

一部例外: PSB の場合で Key が "LMsk", "Lr16", "Lr32", "Layr", "Mt16", "Mt32", "Mtrn", "Alph", "FMsk", "lnk2, "FEid", "FXid", "PxSD" の場合は Length の Byte長が 8Byte(ulong) になるようです。

これから、レイヤー追加情報を適当な順に記しておきます。

### SelectionDivider

KeyCode: "lsct" or "lsdk"

この PSD において レイヤーフォルダーをつかさどる 追加レイヤー追加情報。

後者の "lsdk" は [元のSpec](https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/) に存在せず、 最新の Photshop にて、深い階層を出力させたとき(恐らく6階層以降)記述されることが確認されている。

内容は "lsct" と全く同じのようです。

ちなみにこの "lsdk" の存在する PSD は互換性問題を起こしやすく ClipStudioPaint などは読み取りに失敗し、レイヤー構造が崩壊します。
なお、 ClipStudioPaint は深い階層の場合、"lsdk" を全く使用せず、 "lsct" で出力するため、互換性問題が起きづらく、 ClipStudioPaint が出力した階層の深い PSD は 10階層を越えなければ Photshop で正しく読み込むことが可能です。

つまり ClipStudioPaint の方が治安のいい PSD を出力するってわけですね！

|Byte|Description|
|---|---|
|4([uint-Enum](#selectiondividertype))|SelectionDividerType: [後述](#selectiondividertype)|
||AdditionalLayerInformation.Length が 12 またはそれ以上だった場合に以下の物が存在します。|
|4(ASCII-String)|Signature: "8BIM" |
|4(ASCII-String)|BlendModeKey: レイヤーレコードがそもそも持つ者よりも優先すべきかは謎だがそうしたほうが良いだろう... BlendModeKey については[後述](#blendmodekey)|
||AdditionalLayerInformation.Length が 16 またはそれ以上だった場合に以下の物が存在します。|
|4(uint)|SubType: 0 がノーマル、1がシーングループのようだ...アニメーションのタイムラインに影響するらしい...?|

#### SelectionDividerType

ここの値で、フォルダがどのようになっているかや開始位置か否かを知ることが可能になります。

- AnyOther = 0
- OpenFolder = 1
- ClosedFolder = 2
- BoundingSectionDivider = 3

AnyOther 他タイプのレイヤー...らしいが謎。

BoundingSectionDivider これを観測したら、そのレイヤーレコードがレイヤーフォルダーの始まりを示す。
OpenFolder と ClosedFolder は lsct の終わりを示し、そのフォルダが開かれていたか閉じられていたかを示すようだ。

### TransparencyShapesLayer

透明シェイプレイヤー、おそらく Photshop の古い挙動を呼び起こすための物でもあるようだが...あまりよくわからない。

|Byte|Description|
|---|---|
|1(byte)| 1 は エフェクトの形状として、以前のバージョンとなじ挙動らしい、 0 はブレンドモードの調整などの不透明度としての扱い、現行のデフォルトらしい。|
|3|Padding|

どうやらここに ClipStudioPaint は 加算(発光) 覆い焼(発光) の情報を埋め込むようです。
1 の時に 加算 や 覆い焼き だった場合、それぞれを (発光) の物に読み替えるようです。

### SolidColor

べた塗レイヤーこと単色塗りつぶしのレイヤーであることを示すレイヤー追加情報。

Key -> "SoCo"

|Byte|Description|
|---|---|
|4(uint)|Version: 16 ではなかった場合は読み取らないこと、未知の可能性がある。|
|Variable = [DescriptorStructure](#descriptorstructure)|下記参照|

これに含まれる [DescriptorStructure](#descriptorstructure) の内容を Json で表現するならこんな感じになっています。

```json
{
    "ClassNameID": "",
    "ClassID": "null",
    "DescriptorCount": 1,
    "FollowingDescriptors" :
    [
        {
            "Key": "Clr ",
            "OSTypeKey": "Objc",

            "DescriptorStructure": {
                "ClassNameID": "",
                "ClassID": "RGBC",
                "DescriptorCount": 3,
                "FollowingDescriptors" :
                [
                    {
                        "Key": "Rd  ",
                        "OSTypeKey": "doub",

                        "DoubleStructure": {
                            "ActualValue": 255.0
                        }
                    },
                    {
                        "Key": "Grn ",
                        "OSTypeKey": "doub",

                        "DoubleStructure": {
                            "ActualValue": 125.0
                        }
                    },
                    {
                        "Key": "Bl  ",
                        "OSTypeKey": "doub",

                        "DoubleStructure": {
                            "ActualValue": 0
                        }
                    }
                ]
            }
        }
    ]
}
```

`ActualValue` の部分は実際の色が 0 ~ 255 の範囲で double で格納されているようです。

### HueSaturation

色相/彩度 の色調調整レイヤーである事を示すレイヤー追加情報。

Key -> "hue2" or "hue "

2 の方が新しいバージョンで、ついてないほうが古い。

どちらであっても Version の値は 2 のようだ。

|Byte|Description|
|---|---|
|2(ushort)|Version: 2 では無い場合、読み取らないこと|
|1|Padding|
|1(byte)|Colorization: 0 = 色相調整(つまり通常), 1 = 着色のような Colorization|
||以下三つは Colorization が有効な場合に使用される値 (Colorization有効でなくてもフィールドは存在します。) |
|2(short)|HueWithColorization: newVersion -180 ~ 180 oldVersion -100 ~ 100|
|2(short)|SaturationWithColorization: -100 ~ 100|
|2(short)|LightnessWithColorization: -100 ~ 100|
||以下三つは Colorization が有効ではない場合に使用される値 (Colorization 無効でもフィールドは存在します。) |
|2(short)|Hue: newVersion -180 ~ 180 oldVersion -100 ~ 100|
|2(short)|Saturation: -100 ~ 100|
|2(short)|Lightness: -100 ~ 100|
|6 * 14([HueSaturationWithRange](#huesaturationwithrange))|HueSaturationWithRangeArray: Red,Yellow,Green,Cyan,Blue,Magentaの順でそれぞれの設定が並んでいるようだが、UIのラベルと初期値の違い意外でそれぞれに効果の違いはないと考えてよい。つまり範囲設定のある色相/彩度が追加で六つ並んでるだけ。|

#### HueSaturationWithRange

Photshop の UI で言えば中央の赤 が 0 それから右に行くにつれ増えて端が 180、反対側から中央までが 180 ~ 360 だと思われる。

|Byte|Description|
|---|---|
||以下四つは範囲設定|
|2(ushort)|RangeFallOffEndLeft: 0 ~ 360|
|2(ushort)|RangeFallOffStartLeft: 0 ~ 360|
|2(ushort)|RangeFallOffStartRight: 0 ~ 360|
|2(ushort)|RangeFallOffEndRight: 0 ~ 360|
||以下三つは、その範囲の色調調整の設定|
|2(short)|Hue: newVersion -180 ~ 180 oldVersion -100 ~ 100|
|2(short)|Saturation: -100 ~ 100|
|2(short)|Lightness: -100 ~ 100|

## DescriptorStructure

どうやら任意の形で情報を仕込める 多少の型を付けた Json みたいなもののようです。

Jsonよりも読み取るのが困難ですが...はい

|Byte|Description|
|---|---|
|Variable = [UnicodeString](#unicodestring)|ClassIDName: クラスIDの名前...?|
|Variable = 4(uint)StingLength + ASCII-String |ClassID: 最初の 4Byte が文字列長。なお 0 だった場合は 4 と置き換える。このクラスID が何を意味するのかは謎。|
|4(uint)|DescriptorCount: 子として連なる Descriptor の数。|
||以下を DescriptorCount の分だけ繰り返します。|
|Variable = 4(uint)StingLength + ASCII-String |Key: 最初の 4Byte が文字列長。なお 0 だった場合は 4 と置き換える。|
|4(ASCII-String)|OSTypeKey: [後述参照](#ostypekey)|
|Variable = Unknown|OSTypeKey に応じて、内容は無限に変わります。|

### OSTypeKey

確認された実装の範囲しかの型のみしか網羅できていない

- "obj " -> ReferenceStructure is Unknown
- "Objc" -> [DescriptorStructure](#descriptorstructure) 入れ子のような状態になります。
- "VlLs" -> ListStructure is Unknown
- "doub" -> [DoubleStructure](#doublestructure) ただの 倍精度浮動小数点が格納されています。
- "UntF" -> UnitFloatStructure is Unknown
- "TEXT" -> StringStructure is Unknown
- "enum" -> EnumeratedStructure is Unknown
- "long" -> IntegerStructure is Unknown
- "comp" -> LargeIntegerStructure is Unknown
- "bool" -> BooleanStructure is Unknown
- "GlbO" -> GlobalObjectSameAsDescriptorStructure is Unknown
- "type" -> ClassStructure is Unknown
- "GlbC" -> ClassStructure is Unknown
- "alis" -> AliasStructure is Unknown
- "tdta" -> RawDataStructure is Unknown

### DoubleStructure

|Byte|Description|
|---|---|
|8(double)|ActualValue: 倍精度浮動小数点が格納されている。|

## ImageData

基本的に [ChannelImageData](#channelimagedata)と同じ。

|Byte|Description|
|---|---|
|2(ushort-Enum)|Compression: [Compression](#compression)|
|Variable = Unknown|CompressedImageData: 長さはファイルの終わりまでのようです。|

CompressedImageData の中身は FileHeader.ChannelCount の分だけデータがそのまま並んでいるような扱いだそう。

## CompressedImageData

[圧縮](#compression)に関する情報をすべてここに書き残しておきます。

### 0-RawData

そのまま展開されていると思われる。情報がある状態でこの形式で保存されたデータを確認したことがないため謎です。

#### RawDataWithImageData

1Bit PSD (1 channel)にて確認されていて、ただそのまま並んでいるようです。

1 pixel の最小単位が byte の環境に出力する場合 1byte に詰められた 8 pixel 分の情報を展開する必要があります。

16Bit PSD , 32Bit PSD でも確認されており、

ushort や float が Rの画像,Gの画像,Bの画像,Aの画像, そのまま並んでいる要です。

### 1-RLECompressed

現状 8Bit PSD or 8Bit PSB でしか確認されておらず、それ以外のケースではどのようなってになっているかは謎です。

RunLengthEncoding こと RLE 圧縮。PSD の場合二つのパートに分かれて圧縮されています。

|Byte|Description|
|---|---|
|PSD 2(ushort) or PSB 4(uint) * Header.Height|CompressedWidthLengthArray: RLE圧縮された状態での画像の横一列の長さが縦幅のピクセル数分だけ並んでいます。|
|Variable = CompressedWidthLengthArray.Sum() |RLECompressedPixels: RLE圧縮については下記参照。 CompressedWidthLength の示す長さで区切って横一列の展開をするよとい。 |

RLE の展開方法は、

1Byte を sbyte として読み取りる。ここでは、これを Length と呼びます。

Length が 0 かそれ以上だった場合は、不連続なByte列が Length + 1 分だけ続くのでそれを出力。

Length が -1 かそれ以下だった場合は 次の値が Length + 1 分出力。

具体的なコードで示すならこんな感じ。

```csharp

private static void DecompressRLEWidthLine(ReadOnlySpan<byte> read, Span<byte> write)
{
    var writePos = 0;
    var readPos = 0;

    while (readPos < read.Length)
    {
        var runLength = (sbyte)read[readPos++];
        if (runLength >= 0)
        {
            var count = runLength + 1;

            read.Slice(readPos, count).CopyTo(write.Slice(writePos, count));

            writePos += count;
            readPos += count;
        }
        else
        {
            var count = (-runLength) + 1;

            write.Slice(writePos, count).Fill(read[readPos++]);

            writePos += count;
        }
    }

}

```

#### RLECompressedWithImageData

[ImageData](#imagedata) の場合の注意点ですが、CompressedWidthLengthArray がチャンネル数の分だけ増加するので注意。

展開が終わると RGBA だった場合 R の画像 , G の画像 , B の画像 , A の画像 が並んだ状態になります。

### 2-ZIPWithoutPrediction

謎です、現状これで圧縮された PSD は確認されていません。

### 2-ZIPWithPrediction

現状 16Bit PSD 、32Bit PSD でしか確認されておらず、それ以外については謎です。

基本的には zlib で圧縮されています。(ZIP とは???)

そして Prediction とついているように、 少し特殊なことが行われており、一つ左のピクセルと足し算をすることでデータの解凍ができます。

ただし、 現状確認されている 16Bit と 32Bit で、それぞれ展開する方法が異なります！ご注意！

#### 16Bit-Prediction

BigEndian で short として読み取り、左から順にそのまま足した結果を書き込んでいくことでできます。

横一列分だけを展開する部分をコードにするとこんな感じ
```csharp
for (var i = 1; width > i; i += 1)
{
    var x = i * 2;
    var left = widthSpan.Slice(x, 2);
    var right = widthSpan.Slice(x - 2, 2);

    BinaryPrimitives.WriteInt16BigEndian(left, (short)(BinaryPrimitives.ReadInt16BigEndian(left) + BinaryPrimitives.ReadInt16BigEndian(right)));
}
```

展開ができたら BigEndian UShort の配列として使うことができます。

#### 32Bit-Prediction

1byte ずつ、そのまま 左と足した結果を書き込んでいくこと Prediction の展開だけはできます。

Prediction の展開だけをコードにするならこんな感じ。

```csharp
for (var i = 1; widthBuffer.Length > i; i += 1)
{
    widthBuffer[i] += widthBuffer[i - 1];
}
```

ただし、そのままでは使用することができず、 BigEndian float として使うためには、展開した配列を 四分の一にして、それぞれを float 1byte目 , float 2byte目 ... のように並べなおさないとできません。

コードにするなら大体こんな感じ。

```csharp
var zero = widthBuffer.Slice(width * 0, width);
var one = widthBuffer.Slice(width * 1, width);
var tow = widthBuffer.Slice(width * 2, width);
var three = widthBuffer.Slice(width * 3, width);

for (var x = 0; width > x; x += 1)
{
    var writeIndex = index + (x * 4);
    write[writeIndex + 0] = zero[x];
    write[writeIndex + 1] = one[x];
    write[writeIndex + 2] = tow[x];
    write[writeIndex + 3] = three[x];
}
```

## UnicodeString

|Byte|Description|
|---|---|
|4(uint)|StringLength: 文字数、後のバイト数とは一致しない|
|Variable = StringLength * 2|String: BigEndian UTF16 |

文字列の終わりに、 2byte の null が存在すると 元のSpec には書かれているが...確認できず、謎。
