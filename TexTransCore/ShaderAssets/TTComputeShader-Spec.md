# TTComputeShader-Spec

これは TexTransTool 内部 TexTransCore のシェーダー表現にて使用される形式について、私が忘れないようにするための仕様書です。

## 基本構文

example

```hlsl
/*
BEGIN__TT_COMPUTE_SHADER_HEADER

Language HLSL
LanguageVersion 2018

TTComputeType General

// This is comment

END__TT_COMPUTE_SHADER_HEADER
*/

RWTexture2D<float4> SourceTex;
RWTexture2D<float4> TargetTex;

[numthreads(32, 32, 1)] void CSMain(uint3 id : SV_DispatchThreadID)
{
    float4 col = TargetTex[id.xy];
    col.a = SourceTex[id.xy].a;
    TargetTex[id.xy] = col;
}

```

そのままべたに書いてもよいですが...現実問題として既存のシンタックスハイライトやアナライザー系をそのまま使用できなくなってしまうのでコメントの中にねじ込みます。

その言語の複数行コメントで囲うようにしてください。基本的に `HLSL` `GLSL` `WGSL` は `/* */` のようなので基本的にはこれでよいと思われます。

次に、二つのキーワードの間にあるテキストを TTComp のヘッダーとして扱います。

`BEGIN__TT_COMPUTE_SHADER_HEADER` `END__TT_COMPUTE_SHADER_HEADER`

これはシグネチャとなるためほかの場所でこの文字列を使用しないでください。もしそうした場合は壊れることが正常動作となります。

基本的に Key Value の形で記述しますが、 処理系を作ることに負担をあまりかけたくないのでこのようなルールで記述します。

- 行先頭から次の空白までを Key とします。
- Key と その空白以降、改行文字までを Value とします。
- 空行 が存在してもよい。
- 行先頭から `//` が存在した場合はその行を 空行 として扱います。
- 重複する Key は 重複が許容される特別な場合を除いて存在してはならない。

ちなみに、複数行コメントは実装コストが高いので無かったことにします。それと同様に複数行にまたがる Value は存在できません。

最後にそれらを記述し、シェーダーコードとともに `.ttcomp` の拡張子で扱います。

## 必須情報

これらは上記 Key Value のルールで確定で書き込まないといけない情報です。

### Language

そのシェーダーが何の言語で記述されているかを示す存在です。

example

```text
Language HLSL
```

現状の選択肢はこれらが使用できます。

- `HLSL`

一つしかありませんね！現状拡張性のために書いておくだけの存在にすぎません。

### LanguageVersion

そのシェーダーのバージョン指定になります。

上記 [Language](#language) に応じて選択肢が変わります。

example

```text
LanguageVersion 2018
```

現状の選択肢はこれらが使用できます。

- `HLSL`
  - `2018`

[Language](#language) と同様に拡張性のために書いておく存在です。
Unityの都合 `2018` しか使用できません。今後直に Unityに読ませるのではなく、ダウンコンバートができるようになったら選択肢ができるようになるかもしれませんね...

### TTComputeType

その ComputeShader がなんであるかを示す情報です。

タイプによって追加で必要な情報が増える場合があるので注意。

example

```text
TTComputeType General
```

選択肢は現状はこれらが使用できます。

- `General`
- `GrabBlend`
- `Blending`

## ComputeShader の記述

記述方法には [TTComputeType](#ttcomputetype) に応じて例外的に変わることがあります。

基本的にそれぞれの言語での ComputeShader の定義方法に従います。

基本的に EntryPoint は `CSMain` 固定で、`numthreads` (`workgroup`) は (32,32,0) または、合計値 256 以下にしてください。そして、それぞれの最大値は (256,256,60) になります。

## TT-GrabBlend

TexTransTool 内部で GrabBlend と呼ばれている物を記述するための存在、基本的にはただのコンピュートシェーダー。

必須情報として下記 [ColorSpace](#colorspace) が必要になること以外は General と同じ。

## TT-Blending

TexTransTool の 合成モードを追加できる仕組みでもある。

`TTComputeType` が `Blending` である場合これら追加必須情報が要求されます。

ちなみに、この TTBlending の場合に限り、 ファイル拡張子 `.ttblend` が許容されます。

example

```hlsl
/*
BEGIN__TT_COMPUTE_SHADER_HEADER

// SimpleAdded.ttblend
// This sample code is license under CC0

Language HLSL
LanguageVersion 2018

TTComputeType Blending

ColorSpace Gamma

Key SampleBlend/SimpleAdded

KeyName en SampleBlend/SimpleAdded
KeyName ja サンプルブレンド/シンプル加算

END__TT_COMPUTE_SHADER_HEADER
*/


float4 ColorBlend(float4 BaseColor, float4 AddColor)
{
    return BaseColor + AddColor;
}
```

### 追加の必須情報

Blending として使用するために必要な追加の必須情報です。

#### ColorSpace

引数や戻り値の色空間を指定する存在です。

example

```text
ColorSpace Gamma
```

選択肢はこれらが使用できます。

- `Gamma`
- `Linear`

`Gamma` というのはいわゆる sRGB です。

#### Key

TexTransTool 内で BlendTypeKey としてセーブデータに保存される文字列を指定する存在です。

example

```text
Key SampleBlend/SimpleAdded
```

選択肢

- 英数字 と `/` のみが使用可能です。
  - 英数字というと、 ASCII で 65~90 (大文字アルファベット)、97~122 (小文字アルファベット)、48~57 (数字) ということです。

`/` の区切り文字にて名前空間のような 階層が表現可能です。

TexTransTool デフォルトの物以外の場合は、この階層が必ず 1階層 必要になります。

#### KeyName

TexTransTool 内で インスペクターに表示する場合の表示名を変更できる存在です。

```text
KeyName en SampleBlend/SimpleAdded
KeyName ja サンプルブレンド/シンプル加算
```

この KeyName の場合に限り Value の中身の扱いが少し変わり、Value の開始から一つ目の 空白 までは LanguageCode として扱い、その先を KeyName の Value つまり表示名という扱いになります。

LanguageCode の選択肢

- ja
- en

これは TexTransTool の言語設定に基づいていて、今後増える可能性もあります。
それぞれの言語にて、定義されていない場合は [Key](#key) にフォールバックされるので気を付けましょう。

そしてこの表示名に使える文字は...定義するのが非常に難しいので未定義ということにしておきます。

常識の範囲内の文字を使用してください。

まぁ C# string の 内部表現である UTF16 の範囲内だったら大丈夫なんじゃないかな知らんけど。

### 合成関数の記述

合成関数というのは常に、 二つの色 (アルファ含む) を何らかの方法でもう一つの 色にすること、と定義します。

基本的にその言語で float が 四つの形式 二つを引数にとり、 float 四つの形式を返す関数を、 `ColorBlend` という名前で定義する必要があります。

example

```hlsl
float4 ColorBlend(float4 BaseColor, float4 AddColor)
{
    return BaseColor + AddColor;
}
```
