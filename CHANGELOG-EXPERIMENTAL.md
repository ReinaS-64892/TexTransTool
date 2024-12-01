# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.8...HEAD)

## [v0.8.8](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.7...v0.8.8) - 2024-12-01

## [v0.8.7](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.6...v0.8.7) - 2024-11-23

## [v0.8.6](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.5...v0.8.6) - 2024-11-14

## [v0.8.5](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.4...v0.8.5) - 2024-11-13

### Added

- AtlasTexture に 縦幅の除算 が追加されました (#718)

### Fixed

- AtlasTexture の TextureIndividualFineTuning が Component 生成時に初期化されていないという、潜在的な問題を修正 (#714)

## [v0.8.4](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.3...v0.8.4) - 2024-11-05

## [v0.8.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.2...v0.8.3) - 2024-10-30

## [v0.8.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.1...v0.8.2) - 2024-10-19

## [v0.8.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.0...v0.8.1) - 2024-10-09

## [v0.8.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.7...v0.8.0) - 2024-09-30

### Added

- AtlasTexture に TextureFineTuning を個別に行う TextureIndividualFineTuning が追加 (#532)
- TextureIndividualFineTuning の調整用ウィンドウ TextureFineTuningManager が追加 (#532)
- SingleGradationDecal に SimpleDecal と同じ詳細設定が追加されました (#539)
- AtlasTexture WriteOriginalUV の書き込むチャンネルを指定できる OriginalUVWriteTargetChannel が追加されました (#540)
- 簡易的なグラデーションマップとして UnityGradationMapLayer が追加されました (#545)
- テクスチャの使用率をざっくりと調べることができる DomainTextureAnalyzer が追加されました (#546)
- AtlasTexture に同じテクスチャが割り当てれる場合に ReferenceCopy を自動で設定する AutoReferenceCopySetting が追加されました (#550)
- AtlasTexture TextureFineTuning に別のプロパティとテクスチャを適当に統合する MergeTexture が追加されました (#555)
- AtlasTexture にテクスチャが衝突しない場合に MergeTexture を自動で設定する AutoMergeTextureSetting が追加されました (#555)
- AtlasTexture に DownScalingAlgorism が表示されるようになりました (#558)
- AtlasTexture TextureFineTuning にアルファチャンネルを最大値に塗りつぶし、アルファの情報を破棄する DiscardAlphaChannel が追加されました (#561)
- TextureConfigurator にダウンスケール時にアルファを加味してサイズを縮小するかどうかのオプション DownScalingWithLookAtAlpha が追加されました (#573)
- 縦方向固定でグラデーションをそのままキャンバスに描画するレイヤー YAsixFixedGradientLayer が追加されました (#577)
- SimpleDecal に キャンバスの内容をそのままデカールにできる OverrideDecalTextureWithMultiLayerImageCanvas が追加されました (#579)
- AtlasTargetDefine に MipMap の生成時などに使用される IsNormalMap が追加されました (#589)
- Gimp の着色と同じ効果を持つ ColorizeLayer が追加されました (#601)
- AtlasTexture に アトラス化後のマテリアルに対して スケール(タイリング)とオフセットをリセットする TextureScaleOffsetReset が追加されました (#636)
- AtlasTexture に アトラス化後のマテリアルに対して ベイクされたプロパティに最大値を割り当てる BakedPropertyWriteMaxValue が追加されました (#636)
- AtlasTexture に アトラス化後のマテリアルに対して 特定のテクスチャに対して割り当てを行わないようにできる UnsetTextures が追加されました (#637)

### Changed

- SolidColorLayer Color の Alpha を無効化 (#544)
- TextureConfigurator の圧縮設定のオーバーライドでフォーマットクオリティで決定されるフォーマットがアルファの使用有無を加味したものになるようになりました (#558)
- TextureConfigurator の初期設定を変更 (#562)
- AtlasShaderSupportScriptableObject AtlasTargetDefine の BakePropertyNames は BakePropertyDescriptions に変更され、UseMaxValue が有効ではない場合 BakeShader に Bake時に最大値が割り当てられなくなりました (#636)

### Fixed

- SingleGradationDecal でリアルタイムプレビュー中に IslandSelector を割り当てた時に IslandSelector の調整でプレビューが更新されない問題を修正 (#525)
- LayerFolder に 空の GameObject が含まれると、実行時に例外が発生する問題を修正 (#538)
- TextureBlender TextureSelector が Absolute の場合、SelectTexture を割り当てても実行できない問題を修正 (#560)
- AtlasTexture TextureFineTuning MergeTexture で MergeParent を存在しないものに指定した場合に例外が発生する問題を修正 (#561)
- TextureConfigurator で OverrideCompression は有効だが、OverrideTextureSetting が無効な場合に解像度や MipMap の有無が正しくないテクスチャが生成される問題を修正 (#573)
- TextureConfigurator でもともとのテクスチャよりも大きい解像度を指定した場合に MipMap の0番あたりが黒くなってしまう問題を修正 (#580)
- TexTransToolPSDImporter がインポートするレイヤーのプレビューTexture2D の圧縮形式が誤って DXT5 になっていた問題を修正 (#604)
- SingleGradientDecal の適用対象のレンダラーのマテリアルに Null が含まれている場合に例外が発生する問題を修正 (#612)
- TTT PSD Importer 一部の PSD で ImageResourceBlock に Name が含まれている物の読み取りに失敗する問題を修正 (#632)
- AtlasTexture MaterialMergeGroup の MergeReferenceMaterial が Null の場合例外が発生する問題を修正 (#636)
- TTT PSD Importer 一部の PSD で 重複した色チャンネルを持つと主張する PSD の読み取りに失敗する問題を回避 (#638)
- IslandSelectorOR などの子のコンポーネントを使用する IslandSelector が、子のコンポーネントの削除や増加を監視し忘れていた問題を修正 (#659)
- TextureBlender BlendTexture が空の状態で実行できない問題を修正 (#665)
- TextureBlender などの TextureSelector が AbsoluteMode だった場合、レンダラー Null 例外や存在しないプロパティへのアクセスが発生していた問題を修正 (#665)

## [v0.7.7](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.6...v0.7.7) - 2024-07-24

## [v0.7.6](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.1...v0.7.6) - 2024-07-14

## [v0.7.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.0...v0.7.1) - 2024-05-23

### Fixed

- PSDImportedRasterImage がアルファのないレイヤーをロードできない問題を修正 (#484)

## [v0.7.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.6.0...v0.7.0) - 2024-05-22

### Added

- IslandSelector が追加されました (#422)
- Box(Sphere)IslandSelector　に IsAll オプションが追加されました (#468)
- SimpleDecal の実験的なカリング機能のとして IslandSelector が使用できるようになりました (#422)
- SimpleDecal IslandCulling からのマイグレーションが追加されました (#422)
- TTT PSD Importer のプレビューの生成が大幅に高速化されました (#424 #443)
- AtlasTexture に アイランド詳細調整 が追加されました (#431)
- IslandSelectorNOT と IslandRendererSelector が追加されました (#431)
- AtlasTexture のシェーダーサポートの追加が ScriptableObject で可能になりました (#431)
- 新規作成したオブジェクトが即座に、AtlasTexture に認識されるようになりました(#472)
- AtlasTexture に MaterialMargeGroup が追加されました (#432)
- ClipStudioPaint から出力されたと思われる PSD を TTT PSD Importer で読み込んだ時、Clip系の色合成にインポートするようになりました (#444)
- SubMeshIslandSelector , IslandSelectorXOR , ~~IslandSelectorRelay~~ が追加されました (#447)
- IslandSelectorRelayはキャンセルされました (#450)
- RealTimePreview が大幅に改修され、MultiLayerImageCanvas もリアルタイムプレビュー可能になりました (#448)
- SingleGradationDecal が追加されました (#449)
- SingleGradationDecal に オプション GradationClamp が追加 (#451)
- MaterialOverrideTransfer が追加されました (#456)
- TextureConfigurator が追加されました (#469)

### Changed

- IslandRendererSelector と IslandSelectOR の名前がそれぞれ RendererIslandSelector と IslandSelectorOR に変更されました (#447)
- IslandSelectorNOT は子のオブジェクトの一番目を使用するように変更 (#450)

### Removed

- IslandSelector が使用できるようになったことに伴い SimpleDecal の IslandCulling は削除されました (#422)
- ObjectReplaceInvoke は削除されました (#438)
- CylindricalDecal , CylindricalCurveDecal , CylindricalCoordinatesSystem , NailEditor , NailOffsetData は削除されました (#449)
- MatAndTexAbsoluteSeparator , MatAndTexRelativeSeparator , MaterialModifier は削除されました (#456)

### Fixed

- Library が存在しないときにも正しく TTT PSD Importer がインポートできるようになりました (#427)
- TTT PSD Importer が PSD の ImageResourceBlock を正しく読み込めるようになりました (#443)
- TTT PSD Importer から アルファのないラスターレイヤーが正しくインポートできない問題を修正 (#479)

### Deprecated

## [v0.6.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.3...v0.6.0) - 2024-03-12

### Added

- PSD のすべてのブレンドモードのインポートに対応 (#366)
- PSD から 加算(発光),覆い焼き(発光) のインポートに対応 (#366)
- 通過レイヤーフォルダーに対してクリッピングが行るようになりました (#370)
- "lsdk" が使われている PSD をインポートできるようになりました (#370)
- LevelAdjustmentLayer が追加されました (#370)
- PSD から LevelAdjustmentLayer がインポートできるようになりました (#370)
- SelectiveColorAdjustment が追加されました (#370)
- PSD から SelectiveColorAdjustment がインポートできるようになりました (#370)
- 配下の TexTransBehavior をすべてプレビューでき、フェーズごとに順番を確認できる UI を表示する、ビルド時には効果のない PreviewGroup が追加されました (#375)
- 同じ GameObject のレンダラーを対象とするアバター内の TexTransBehavior をプレビューできる PreviewRenderer が追加されました (#375)
- AtlasTexture に実験的機能として AtlasIslandRelocator が追加されました (#379)
- AtlasTexture に実験的機能として、アイランドの移動をピクセル単位のステップにノーマライズする機能が追加されました (#382)

### Changed

- 名前修正 HSVAdjustmentLayer -> HSLAdjustmentLayer (#391)

### Removed

- SimpleDecal の GrabDecal は削除されました (#385)

### Fixed

- 一色しかないのマスクのインポートができていなかった問題を修正 (#370)
- (#382) で追加されたピクセルノーマライズがソースのノーマライズでソースとなるテクスチャの解像度ではない問題を修正 (#386)
- グループからプレビューかビルドの場合で UseDepth を使用した SimpleDecal が二つ以上存在し、二つ目以降に実行された SimpleDecal が正しくはられない問題を修正 (#394)
- いくつかのクリッピングの状態で例外の発生や意図した挙動ではない状態になっていた問題を修正 (#395)

## [v0.5.3](https://github.com/ReinaS-64892/TexTransTool/releases/tag/v0.5.3) - 2024-02-02

### Added

- TTT PSD Importer の高速化 (#346)
- 色相・彩度・明度 の色調調整レイヤー HSVAdjustmentLayer が追加 (#346)
- バイナリ内のレイヤーイメージを指すオブジェクトを使用する RasterImportedLayer が追加 (#346)
- レイヤーの追加に伴い、TTT PSD Importer が HSVAdjustmentLayer のインポート機能を追加 (#346)
- TTT PSD Importer が SolidColorLayer のインポート機能を追加 (#346)
- TextureSelector にモードが追加され、Absolute が追加 (#347)
- TextureSelector にアバター内のテクスチャだけを列挙し、選択できる DomainTexturesSelector を追加 (#347)

### Changed

- TTT PSD Importer はコンテキストメニューから、ScriptedImporter に変更 (#346)
- SolidLayer は SolidColorLayer に名称変更 (#346)
- TextureSelector にモードが追加され、以前までのデータは Relative に変更(#347)
- 上記に伴い TextureSelector のフィールド名を変更 (#347)

### Removed

### Fixed

- 内部挙動が大きく変更され クリッピングが以前よりも再現度が高くなりました (#346)
- 内部挙動が大きく変更され LayerFolder の PassThrough が以前よりも再現度が高くなりました (#346)
