# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased](https://github.com/ReinaS-64892/TexTransTool/compare/v1.0.1...HEAD)

## [v1.0.1](https://github.com/ReinaS-64892/TexTransTool/compare/v1.0.0...v1.0.1) - 2025-12-26

## [v1.0.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.10...v1.0.0) - 2025-06-22

## [v0.10.10](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.9...v0.10.10) - 2025-06-20

## [v0.10.9](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.8...v0.10.9) - 2025-06-17

## [v0.10.8](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.7...v0.10.8) - 2025-06-15

## [v0.10.7](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.6...v0.10.7) - 2025-06-14

## [v0.10.6](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.5...v0.10.6) - 2025-05-28

## [v0.10.5](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.4...v0.10.5) - 2025-05-27

### Fixed

- LayerFolder が Clipping と PassThrough が同時に有効化された場合に不正な挙動をしていた問題を修正しました (#990)

## [v0.10.4](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.3...v0.10.4) - 2025-05-24

### Fixed

- EverythingUnlitTexture が Mesh を持たないレンダラーが存在すると例外を発生させる問題を修正 (#985)
- PreviewIslandSelector が Mesh を持たないレンダラーが存在すると例外を発生させる問題を修正 (#985)

## [v0.10.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.2...v0.10.3) - 2025-05-17

## [v0.10.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.1...v0.10.2) - 2025-05-17

## [v0.10.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.0...v0.10.1) - 2025-05-16

### Fixed

- TTT PSD Importer が 0文字 のレイヤー名を持つレイヤーのレイヤー追加情報を正常に読み込めない問題を修正 (#970)
- TTT PSD Importer が レイヤー追加情報`luni` が存在しない場合に通常のレイヤー名を使用するフォールバックが正しくできておらず例外が発生していた問題を修正 (#970)

## [v0.10.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.9.3...v0.10.0) - 2025-05-14

### Added

- AtlasTexture に アトラス化対象のテクスチャの最大サイズを割り当てる AutoTextureSizeSetting が追加されました (#900)
- AtlasShaderSupport の代わりに ITTShaderTextureUsageInformation が追加されました (#900)
- MaterialModifier が RenderQueue をオーバーライドできるようになりました (#922)
- TTT UVCopy という UV をコピーすることが可能なコンポーネントが追加されました (#926)
- TTCE-Wgpu がプロジェクトに存在する時に TTT の ConfigMenu から Backend として Wgpu を選択することが可能になりました (#934)
- IslandSelector や AtlasTexture のターゲティングに IsActiveInheritBreaker が干渉できるようになりました (#945)

### Changed

- AtlasTexture の持つ実験的機能のほぼすべてが AtlasTextureExperimentalFeature に移動しました (#900)
- SimplDecal の持つ実験的機能のすべてが SimpleDecalExperimentalFeature に移動しました (#924)

### Removed

- AtlasShaderSupport は削除されました (#900)
- AtlasTexture WriteOriginalUV と OriginalUVWriteTargetChannel は削除されました (#900)

### Fixed

- MaterialModifier の Utility などで Shader の変更が加味されていなかった問題が修正されました (#921)

## [v0.9.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.9.1...v0.9.2) - 2025-03-03

### Fixed

- PSD のビルドや Preview の生成時にメモリリークが発生していた問題を修正 (#887)

## [v0.9.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.9.2...v0.9.3) - 2025-03-11

## [v0.9.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.9.0...v0.9.1) - 2025-03-02

## [v0.9.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.13...v0.9.0) - 2025-02-17

### Added

- MultiLayerImageCanvas の処理時に必要な VRAM 使用量が大幅に削減されました (#672)
- TTT PSD Importer の PSD の出力元ソフトウェアの判定をオーバーライドできる、 ImportMode 設定が追加されました (#675)
- TTT PSD Importer の PSD の出力元ソフトウェアに応じて PassThrough や Clipping が調整されるようになりました (#675)
- 16Bit 32Bit の PSD をインポートとビルドができるようになりました (#675)
- 実験的な機能として、HLSLで特定の関数を記述し `.ttblend` にすることで ScriptedImporter 経由で 合成モードの追加が可能になりました (#676)
- クリスタの `色相・彩度・明度` 色調調整レイヤーを再現する HSVAdjustmentLayer が追加されました(#678)
- PSD の ImportMode が ClipStudioPaint に自動決定またはオーバライドされていた場合 HSLAdjustmentLayer が HSVAdjustmentLayer としてインポートされるようになりました (#678)
- ~~テクスチャやマテリアルの範囲を切り分けることができる~~ DomainDefinition が追加されました (#694 #802)
- 親の GameObject が無効な場合でも それの配下のコンポーネントが動作するようになる IsActiveInheritBreaker が追加されました (#694)
- TTT PSD Importer はインポートのタイミングでプレビューを生成せず、必要になったタイミングで生成するようなりインポート自体の速度は大幅に高速化しました。(#727)
- PSD からインポートされたレイヤーは ComputeShader で解凍されるようになり、プレビューの生成やビルドが高速化しました (#727)
- TTCE-Wgpu が プロジェクトに存在した場合、PSD からインポートされたレイヤーのプレビュー生成が並列で行われるようになり大幅に高速化するようになりました (#727)
- SingleGradationDecal にも RendererSelectMode が追加され SimpleDecal のような レンダラーを手動で指定する事が可能になりました (#753)
- シーンビューからエイムすることでアイランドを選択できる AimIslandSelector が追加されました (#764)
- マテリアルの参照をベースにアイランドを選択できる MaterialIslandSelector が追加されました (#764)
- SimpleDecal の DepthDecal 機能が内部実装の変更により、レンダラーごとではなくすべてのレンダラーで統一された Depthバッファー を参照するようになりました (#764)
- TTT PSD Importer は PSD の ImageDataSection の画像を PSDImportedImageDataSectionImage としてインポートするようになりました (#772)
- TTT PSD Importer は 32bit PSD と 16bit PSD のプレビューが可能になりました (#772)
- 一つの子となる IslandSelector を基に IslandSelect の範囲を広げて選択できる、 IslandSelectorLink系 コンポーネントが 4つ 追加されました (#777)
- TTT PSD Importer に PSD ImportMode SAI が追加され、出力元が SAI であるとみられる場合に自動判定されるようになりました (#781)
- MaterialModifier がゼロからリメイクされ、マテリアルをその場で変更し差分をオーバーライドとして非破壊的に適用できる機能を持って、新規コンポーネントとして復活しました (#788 #807)
- マテリアルのコンテキストメニューから MaterialOverrideTransfer と MaterialModifier が追加できる MenuItem が追加されました (#792)
- 非常に実験的なコンポーネントとして、ポリゴンの最接近点からテクスチャを転写するようなことができる NearTransTexture が追加されました (#816)
- 二つの色を指定し、その色差をテクスチャに適用する ColorDifferenceChanger が追加されました (#827)
- TextureBlender や ColorDifferenceChanger を MultiLayerImageCanvas のレイヤーとして扱うことのできる AsLayer が追加されました (#834)
- MultiLayerImageCanvas に連なるレイヤーの LayerMask が、 TTT ImportedLayerMask マスクなどの別種類のレイヤーマスクを選択することが可能になりました (#834)
- MaterialModifier や TextureConfigurator を右クリックをした GameObject 配下のマテリアルやテクスチャすべてに対して コンポーネントを一括で生成できる `Generate` が追加されました (#833)
- SimpleDecal や SingleGradationDecal が AsLayer でレイヤーとして扱うことが可能になりました (#837)
- レイヤーに対して使用できるレイヤーマスクとして、 IslandSelector をマスクにすることが可能な IslandSelectAsLayerMask が追加されました (#838)
- 距離ベースでグラデーションをかけることができる、 DistanceGradationDecal が追加されました (#847)
- (Imported)RasterLayer や LayerFolder(非Passthrough) などの、ImageLayer に限り、LayerMask 等も加味した単体プレビューが表示されるようになりました (#849)
- VRAMの増加と引き換えに元のテクスチャの解像度を超越できる、ParallelProjectionWithLilToonDecal が追加されました (#851)
- EverythingUnlitTexture が追加されました (#859)
- PreviewIslandSelector が追加されました ~~が、NDMF 側のバグにより一時無効化されています。~~(#859 #879)
- MultiLayerImageCanvas もレイヤーのようなプレビューが表示されるようになりました (#870)

### Changed

- SingleGradationDecal がデフォルト設定では 無効なレンダラーに対して描画しないようになりました (#753)
- RayCastIslandSelector は PinIslandSelector に名前が変更されました (#764)
- Decal系は Preview の場合 HighQualityPadding が無効化されるように変更されました (#764)
- SubMeshIslandSelector が SubMeshIndexIslandSelector に名前が変更されました (#777)
- MaterialOverrideTransfer は UnDefinedPhase から MaterialModificationPhase に属するフェーズが変更されました (#826)

### Fixed

- PSD の古い 色相/彩度 の色調調整レイヤーの追加情報 KeyCode "hue " が誤っていて認識されていなかった可能性のある問題を修正 (#675)
- RendererIslandSelector が NDMF Preview で正常に動作しない問題を修正 (#764)
- SingleGradationDecal や YAsixFixedGradientLayer にて、境界の色がにじむ問題を修正 (#786)

## [v0.8.13](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.12...v0.8.13) - 2025-01-09

### Fixed

- IslandSelectorAND(OR,XOR) が子として連なる IslandSelector が存在しない場合に Null 例外を発生させる問題を修正 (#766)

## [v0.8.12](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.11...v0.8.12) - 2024-12-28

## [v0.8.11](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.10...v0.8.11) - 2024-12-24

## [v0.8.10](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.9...v0.8.10) - 2024-12-18

## [v0.8.9](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.8...v0.8.9) - 2024-12-09

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
