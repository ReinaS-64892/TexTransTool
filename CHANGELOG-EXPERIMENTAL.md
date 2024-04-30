# Changelog

v0.5.2 以降の実験的機能の変更記録です。
[Keep a Changelog](https://keepachangelog.com/en/1.0.0/)のフォーマットにある程度乗っ取りますが、そのさじ加減は適当に決められ、完全にそのフォーマットではないことをご了承ください。

## Unreleased

### Added

- IslandSelector が追加されました (#422)
- SimpleDecal の実験的なカリング機能のとして IslandSelector が使用できるようになりました (#422)
  - IslandCulling からのマイグレーションが追加されました (#422)
- TTT PSD Importer のプレビューの生成が大幅に高速化されました (#424 #443)
- AtlasTexture に アイランド詳細調整 が追加されました (#431)
- IslandSelectorNOT と IslandRendererSelector が追加されました (#431)
- AtlasTexture のシェーダーサポートの追加が ScriptableObject で可能になりました (#431)
- AtlasTexture に MaterialMargeGroup が追加されました (#432)
- ClipStudioPaint から出力されたと思われる PSD を TTT PSD Importer で読み込んだ時、Clip系の色合成にインポートするようになりました (#444)
- SubMeshIslandSelector , IslandSelectorXOR , ~~IslandSelectorRelay~~ が追加されました (#447)
  - IslandSelectorRelayはキャンセルされました (#450)
- RealTimePreview が大幅に改修され、MultiLayerImageCanvas もリアルタイムプレビュー可能になりました (#448)
- SingleGradationDecal が追加されました (#449)
  - オプション GradationClamp が追加 (#451)

### Changed

- IslandRendererSelector と IslandSelectOR の名前がそれぞれ RendererIslandSelector と IslandSelectorOR に変更されました (#447)
- IslandSelectorNOT は子のオブジェクトの一番目を使用するように変更 (#450)

### Removed

- IslandSelector が使用できるようになったことに伴い SimpleDecal の IslandCulling は削除されました (#422)
- ObjectReplaceInvoke は削除されました (#438)
- CylindricalDecal , CylindricalCurveDecal , CylindricalCoordinatesSystem , NailEditor , NailOffsetData は削除されました (#449)

### Fixed

- Library が存在しないときにも正しく TTT PSD Importer がインポートできるようになりました (#427)
- TTT PSD Importer が PSD の ImageResourceBlock を正しく読み込めるようになりました (#443)

### Deprecated

## 0.6.0

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

## 0.5.3

### Added

- TTT PSD Importer の高速化 `#346`
- いくつかのレイヤーが追加されました `#346`
  - 色相・彩度・明度 の色調調整レイヤー HSVAdjustmentLayer が追加
  - バイナリ内のレイヤーイメージを指すオブジェクトを使用する RasterImportedLayer が追加
- レイヤーの追加に伴い、TTT PSD Importer が HSVAdjustmentLayer のインポート機能を追加 `#346`
- TTT PSD Importer が SolidColorLayer のインポート機能を追加 `#346`
- TextureSelector にモードが追加され、Absolute が追加 `#347`
- TextureSelector にアバター内のテクスチャだけを列挙し、選択できる DomainTexturesSelector を追加 `#347`

### Changed

- TTT PSD Importer はコンテキストメニューから、ScriptedImporter に変更 `#346`
- SolidLayer は SolidColorLayer に名称変更 `#346`
- TextureSelector にモードが追加され、以前までのデータは Relative に変更`#347`
  - 上記に伴いフィールド名を変更 `#347`

### Removed

### Fixed

- 内部挙動が大きく変更され `#346`
  - クリッピングが以前よりも再現度が高くなりました
  - LayerFolder の PassThrough が以前よりも再現度が高くなりました
