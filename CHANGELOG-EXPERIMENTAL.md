# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.7...HEAD)

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
  - 上記に伴いフィールド名を変更 (#347)
  

### Removed

### Fixed

- 内部挙動が大きく変更され クリッピングが以前よりも再現度が高くなりました (#346)
- 内部挙動が大きく変更され LayerFolder の PassThrough が以前よりも再現度が高くなりました (#346)
