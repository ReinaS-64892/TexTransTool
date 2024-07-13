# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.5...HEAD)

### Added

- AtlasTexture の アイランド再配置結果の詳細を NDMFConsole にレポートする機能を追加 (#531)
- AtlasTexture TextureFineTuning ReferenceCopy の TargetPropertyName がリストに変更されコピー対象を複数指定可能になりました (#532)
- AtlasTexture の _MainTex 以外のプロパティで アトラス化対象だった場合、アトラス化後のサイズが自動的にそのプロパティのテクスチャの最大サイズが割り当てられるようになりました (#550)

### Changed

- AtlasTexture の影響範囲が TargetRoot に影響されなくなり、インスペクターのマテリアル表示制限の機能のみに変更 (#516)
- NDMF の対応バージョンが v1.3.0 以上に変更 (#516)
- 二のべき乗の値を想定する入力欄がポップアップに変更 (#516)
- TargetRoot は LimitCandidateMaterials に変更され、割り当てなくてもマテリアルの選択が行えるように変更 (#518)
- SerializeReference を使用している部分のUIが、[Unity-SerializeReferenceExtensions](https://github.com/mackysoft/Unity-SerializeReferenceExtensions) を使用したものに変更 (#519)
- AtlasTexture TextureFineTuning の PropertyNames でスペース区切りの複数指定が行える仕様は削除され、リストに変更されました (#532)

### Removed

- ReferenceResolver は削除されました (#517)

### Fixed

- lilToon の \[Optional\] 系を誤って 通常のlilToonの対応で認識してしまい、例外が発生する問題を修正 (#520)
- SubMesh よりも多くの MaterialSlot がある場合 AtlasTexture のメッシュノーマライズで、誤ったサブメッシュで複製される問題を修正 (#521)
- AtlasTexture の IslandFineTuning が Null な場合や IslandSelector が Null の場合に例外が発生する問題を修正 (#530)
- SimpleDecal でリアルタイムプレビュー中に IslandSelector を割り当てた時に IslandSelector の調整でプレビューが更新されない問題を修正 (#525)
- AtlasTexture の TextureFineTuning Resize が AtlasTextureSize よりも大きい解像度に変更できていた問題を修正 (#550)

## [v0.7.5](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.4...v0.7.5) - 2024-06-29

### Fixed

- AdditionGlow と ColorDodgeGlow の計算式をより正しいものに修正 (#513)
- Clip系の合成モードが不足していた問題を修正 (#513)
- MeshFilter が存在しない MeshRenderer が存在するとき、 AtlasTexture の TargetRoot が指定できず、例外が発生する問題を修正 (#514)

## [v0.7.4](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.3...v0.7.4) - 2024-06-07

### Fixed

- AtlasTexture の選択対象に None が含まれていると例外が発生する問題を修正 (#493)
- AtlasTexture のアトラス化対象に、Texture Offset Tiling が使用されているとその部分が黒くなる問題を修正 (#494)

## [v0.7.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.2...v0.7.3) - 2024-05-30

### Fixed

- 他ツールがレンダラーを破棄した場合などに、正しく OptimizingPhase が実行できない問題を修正 (#491)

## [v0.7.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.1...v0.7.2) - 2024-05-25

### Fixed

- Marge -> Merge  スペルミス修正 (#486)
- AtlasTexture の実行時に、MipMapを使用するときにエラーが発生する問題を修正 (#487)
- Default の AtlasShaderSupporter が何らかの問題で存在しない場合に例外が発生する問題を修正 (#488)

## [v0.7.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.0...v0.7.1) - 2024-05-23

### Fixed

- 同じプロパティ名が複数存在するシェーダーが存在する場合に例外が発生する問題を修正 (#483)

## [v0.7.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.6.6...v0.7.0) - 2024-05-22

### Added

- Optimizing Phase が追加されました (#410)
- Optimizing Phase が NDMF OptimizePhaseに実行されるようになりました。 (#438)
- GameObject から TexTransTool のほとんどのコンポーネントが追加できるようになりました (#411)
- AtlasTexture に強制的に優先度のサイズに変更する ForcePrioritySize が追加されました (#431)
- AtlasTexture 複数のマテリアルが衝突しないテクスチャを持つ場合に、同一のアイランドが割り当てられるようになりました (#431)
- AtlasTexture が Scale Transition(Tiling) を使用しているマテリアルのテクスチャを逆補正する機能が追加されました (#431 #435)
- NDMF環境でのビルドを行う場合、オブジェクトの置き換えの追跡を NDMF ObjectRegistry を使用するようになりました (#438)
- Clip系の合成モードが追加されました (#444)
- SimpleDecal が高速化しました (#449)
- VRChat Avatar SDK が存在するときに NDMF が存在しない場合に警告を生成すようになりました (#452)

### Changed

- SizeOffset は廃止され、SizePriority に変更されました (#431)
- AtlasTexture のプロパティの並び順が変更されました (#431)
- 内部的に使用される RenderTexture の形式が Win Linux Mac にかかわらず ARGB32 を使用するように変更されました (#461)
- プレビューを行うときに、テクスチャの圧縮が行われるように変更されました (#465)

### Removed

- AtlasTexture の UseUpScale は削除されました (#431)
- プログレスバーの詳細な表示が削除されました (#438)
- SimpleDecal のポリゴンカリング、Edge と EdgeAndCenterRay　は削除されました (#449)

### Fixed

- AtlasTexture で SubMesh よりも多くのマテリアルスロットが存在するメッシュで正しくアトラス化できない問題を修正 (#431)
- AtlasTexture でサブメッシュを超えて同一の頂点を使用するメッシュを正しくアトラス化できない問題を修正 (#431)
- AtlasTexture の「適用時に非アクティブなレンダラーを含める」が有効な時、非アクティブなレンダラーのマテリアルが選択肢に表示されない問題を修正 (#431)
- AtlasTexture で大きさが完全に 0 のアイランドが存在するメッシュの UV を正しく操作できていない問題を修正 (#446)
- AtlasTexture の テクスチャ詳細設定で、色空間 のUIが正しく表示されていない問題を修正 (#462)
- AtlasTexture でテクスチャが縮小される場合に MipMap を使用していない問題を修正 (#463)
- Mac環境でマイグレーションのダイアログが、512Byte以上のマルチバイト文字列であったために、クラッシュする問題の回避を追加 (#466)
- シンボルの誤りにより回避できていなかった問題を修正 (#470)

### Deprecated

## [v0.6.6](https://github.com/ReinaS-64892/TexTransTool/compare/v0.6.5...v0.6.6) - 2024-04-16

### Fixed

- SimpleDecal の 深度デカールを有効化した場合に例外が発生し正しく処理できない問題を修正 (#441)

## [v0.6.5](https://github.com/ReinaS-64892/TexTransTool/compare/v0.6.4...v0.6.5) - 2024-04-15

### Fixed

- エディター起動から一度も、TexTransToolのコンポーネントを表示してない状態で、 NDMFのエラーレポートでエラーが表示されたときに正しくローカライザーがロードされない問題を修正 (#439)

## [v0.6.4](https://github.com/ReinaS-64892/TexTransTool/compare/v0.6.3...v0.6.4) - 2024-04-01

### Added

- UVtoIsland の高速化 (#412)
- Materialの使い回しにより TransTexture と TextureBend が若干の高速化 (#430)

### Removed

- UVtoIsland の高速化 に伴い UseIslandCash は削除されました (#412)

### Fixed

- 0~1 範囲外のUVを持つメッシュに SimpleDecal が正しく使用できない問題を修正 (#430)
- 0~1 範囲外のUVを持つメッシュにデカールのリアルタイムプレビューを使用した時真っ黒になる問題を修正 (#430)
- SimpleDecal のリアルタイムプレビューが不必要にレンダーテクスチャの更新を重複して行っていた問題を修正 (#430)

## [v0.6.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.6.2...v0.6.3) - 2024-03-15

### Fixed

- TexTransToolのコンポーネントを削除する処理で、GameObjectも削除されていた問題を修正 (#414)

## [v0.6.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.6.1...v0.6.2) - 2024-03-13

### Fixed

- AtlasTexture サブメッシュを超えて同一頂点を使用されているメッシュを正しく処理できない問題を、そのサブメッシュ間で同一テクスチャーが使用されている場合でかつ、それらサブメッシュのマテリアルがどちらもアトラス化対象の場合に限り正しく動くように修正 (#407)

## [v0.6.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.6.0...v0.6.1) - 2024-03-12

### Fixed

- Glow系の合成のアルファの計算が正しくないのを修正 (#405)
- Exclusion の計算式が調整されました (#405)

## [v0.6.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.7...v0.6.0) - 2024-03-12

### Added

- プレビューを再度実行できるメニューアイテム ~~& ショートカット(Shift + R)~~ が追加 (#357)
- ショートカット(Shift + R) の追加はキャンセルされました (#384)
- 前回プレビューした物を再度実行できるようになりました (#384)
- ローカライズの仕組みが改修され AtlasTexture と SimpleDecal が正しく実行不可のエラーを NDMF のエラーレポートに出力するようになりました。(#360)
- ほとんどのプロパティにツールチップが追加されました。(#360)
- Dissolve-ディザ合成 の合成モードが追加されました (#213)
- Exclusion-除外 の合成モードが追加されました (#213)
- DarkenColorOnly-比較カラー(暗) の合成モードが追加されました (#213)
- LightenColorOnly-比較カラー(明) の合成モードが追加されました (#213)
- PinLight-ピンライト の合成モードが追加されました (#213)
- HardMix-ハードミックス の合成モードが追加されました (#213)
- AdditionGlow-加算(発光) の合成モードが追加されました (#213)
- ColorDodgeGlow-覆い焼(発光) の合成モードが追加されました (#213)
- AtlasTexture のマテリアル選択のプレハブオーバーライドを操作するための UI を追加しました (#368)
- SimpleDecal の RealTimePreview が複数同時にプレビューを更新したり、複数同時にプレビュー状態にする機能が追加されました。(#368)
- AtlasTexture の AtlasTextureSize に比べて大きいテクスチャーが多数対象にされた場合でも高速に再配置を行えるようになりました (#379)
- AtlasTexture の AtlasTextureSize に比べて小さいテクスチャーが多数対象にされた場合、高さを二のべき乗の高さのステップで小さくする機能が追加されました (#381)
- プレビュー中にビルドが実行された場合にエラーレポートを行うようになりました (#396)
- マテリアルスロットの数がサブメッシュよりも多いレンダラーが存在している状態で AtlasTexture が実行した場合警告をレポートするようになりました (#398)
- 複数のサブメッシュが同一の頂点を使用しているメッシュが存在したとき、AtlasTexture の結果が正しくない可能性があることを警告としてレポートするようになりました (#403)

### Changed

- BlendTypeKey の並び順を変更しました (#365)
- AtlasTexture や SimpleDecal が プレビュー中に操作できるように変更 (#368)
- 上記に伴い AtlasTexture がプレビュー中にマテリアル選択を表示しないように変更 (#368)
- SimpleDecal の DecalTexture の表示が変更されました (#368)
- SimpleDecal の複数同時編集が、何かがプレビューされているときでも可能になりました (#369)
- TexTransGroup と PhaseDefinition の表示される範囲の調整とUIElementに変更 (#372)
- TexTransGroup と PhaseDefinition のプレレビューの実行範囲が表示される範囲と同じになるように変更 (#375)
- AtlasTexture 破壊的な変更として、ターゲットのテクスチャーと AtlasTextureSize から正しくスケーリングし、再配置を行うように変更 (#379)
- AtlasTexture 破壊的な変更として、Padding 値が、テクスチャスケールから、UVスケールに変更 (#379)
- AtlasTexture の TextureFineTuning の保存形式が SerializedReference に破壊的変更 (#389)

### Removed

- AtlasTexture の SorterName の表示は削除されました (#379)
- SimpleDecal の Preview ボタンは削除されました (#385)

### Fixed

- BlendTypeKey の UI でプレハブオーバーライドの操作が正しく右クリックから呼び出せるようになりました (#358)
- PropertyName の UI でプレハブオーバーライドの操作が正しく右クリックから呼び出せるようになりました (#362)
- 特定のケースで SimpleDecal の RealTimePreview が正しく行われない問題を修正 (#368)
- AtlasTextureSize と 元のテクスチャーとの比率の計算が正しく行われていなかった問題を修正 (#381)
- AtlasTexture の AtlasTextureSize などで 二のべき乗ではない値が入力可能だった問題を修正 (#390)
- Hue,Saturation,Color,Luminosity の合成モードが一般的な画像編集ソフトと違う問題を修正 (#392)
- NDMFのエラーレポートウィンドウでローカライズのロードのバグで正しく表示されない問題を修正 (#396)
- マテリアルスロットの数がサブメッシュよりも多いレンダラーが存在している状態で AtlasTexture が実行できない問題を修正 (#398)

## [v0.5.7](https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.6...v0.5.7) - 2024-02-27

### Fixed

- マイグレーションが必要かどうかの検証が正しく行われない問題を修正 (#378)

## [v0.5.6](https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.4...v0.5.6) - 2024-02-25

リリース処理の誤りにより、パッチが一つ上がっており内容は 0.5.5 の物です。

### Fixed

- MatCap の二番目を一番目と見間違えて例外が発生し、正常にアトラス化できない問題を修正しました (#374)

## [v0.5.4](https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.3...v0.5.4) - 2024-02-07

### Fixed

- LogoTexture が NDMF v1.3.x ではない環境で存在しないためコンパイルエラーになる問題を修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/86183088ec5b362700becad4c5baa283a174b392)
- BlendTypeKey LinearLight と Addition の色合成を修正 (#354)
- AtlasTexture のプロパティベイクが値は同じだけどほかにテクスチャーが存在する場合、既定の値のテクスチャーを生成し忘れていた問題の修正 (#355)
- BlendTypeKey VividLight の色合成を修正 (#356)

## [v0.5.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.2...v0.5.3) - 2024-02-02

### Added

- NDMFのエラーレポートに最低限の対応が追加 (#349)

## [v0.5.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.1...v0.5.2) - 2024-01-25

### Fixed

- TextureSelector が NullReferenceException を吐き続ける問題を修正 (#342)
- ColorDodge,ColorBurn (覆い焼カラー、焼きこみカラー) の色合成を修正 (#344)

## [v0.5.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.0...v0.5.1) - 2024-01-20

### Fixed

- ブレンドタイプキー SoftLight が一般的な画像編集ソフトに近くなりました (#295)

## [v0.5.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.5...v0.5.0) - 2024-01-18

### Added

- 画像編集ソフトのコンポジットを再現する MultiLayerImageCanvas を追加 (#160)
- それに伴い LayerFolder と RasterLayer を追加 (#160)
- 特殊なレイヤーとして SolidLayer を追加 (#160)
- PSD の importer が追加され、アセットの右クリックメニューに TexTransTool/TTT PSD Importer が追加 (#160)
- ReferenceResolver が追加されました (#183)
- BeforeUVModification と UVModification の間にスタックをマージし、デカール系統や MultiLayerImageCanvas の効果が消えないように MidwayMergeStack を追加 (#200)
- SimpleDecal のインスペクターに複数編集を追加 (#203)
- ~~未知のシェーダーですべてのテクスチャーをアトラス化の対象にするオプション UnknownShaderAtlasAllTexture を追加~~ キャンセルされました(#321)
- MultiLayerImageCanvas と TextureBlender にリゾルバー AbsoluteTextureResolver を追加 (#216)
- Preview でもビルドと同じようにアバター全体の影響になる DomainMarkerFinder を追加 (#219)
- 実験的機能として、GrabDecal が SimpleDecal に追加 (#233)
- メッシュとテクスチャーの置き換えも適用する機能を追加 (#232)
- MenuItem の Language に CheckMark の表示を追加 (#233)
- 非常に実験的な機能として UseDepth と DepthInvert が追加 (#150)
- 無効化されたレンダラーもアトラス化の対象にする IncludeDisableRenderer を追加 (#222)
- 非常に実験的な API を追加 (#258)
- AtlasTexture のアトラス化するときに、アップスケーリングを許可するオプションを追加 (#279)
- 最低限のNDMFのErrorReportへの対応 (#293)
- プレビューの場合でもマテリアルの置き換えによる実行不可や設定が外れる問題を解決する ReplacementQuery を追加 (#318)
- グローバル設定として、 UseIslandCache が追加 (#321)
- SimpleDecal のリアルタイムプレビューが複数同時に行われていた場合に、すべてのリアルタイムプレビューを終了するボタンを追加 (#320)
- AtlasTexture の FineTuning の Compress に UseOverrideTextureFormat とそれに伴う様々が追加 (#326)
- AtlasTexture の FineTuning に色空間の設定ができる ColorSpace が追加 (#326)

### Changed

- SimpleDecal のインスペクターのサイズ調整機能が直接トランスフォームを変更する形に変更 (#202)
- Mesh の UV1 に元の UV を書き込む機能 WriteOriginalUV をオプションに変更 (#204)
- "Exit RealTime Previews" は 通常の Preview も終了する "Exit Previews" に変更 (#197)
- Texture2D のプロパティがプレハブオーバーライドをコントロールできるように変更 (#217)
- マテリアルの置き換えをほかのコンポーネントにも適用する機能をオプションに変更  その機能は実験的機能に変更 (#232)
- Preview の場合オリジナルのテクスチャを取得しなくなり、結果のテクスチャを圧縮しないように変更 (#231 #186)
- SimpleDecal の IslandCulling を実験的機能に変更 (#229)
- SimpleDecal の Far Culling の基準を変更 (#242)
- PropertyName の保存形式を調整し、データが勝手に変わらないように変更 (#228)
- Texture のブレンドを即時実行するように変更し、VRAM + RAM 容量が Decal などのコンポーネントの最大数にならないように変更 (#188)
- AtlasTexture や Decal などのセーブデータに大きな変更 (#252 #256)
- ~~Unity の最小バージョン指定を 2021.3 に変更 (#260)~~
- AtlasTexture のアトラス化するとき、標準の動作はアップスケーリングできない仕様に変更 (#279)
- 他がプレビュー中のときにプレビューができないのではなく、プレビューを乗っ取るボタンに変更 (#298)
- Unity の最小バージョン指定を 2022.3 に変更 (#310)
- SimpleDecal のリアルタイムプレビューが可変レートで更新されるように変更 (#320)
- AtlasTexture の TextureFineTuning などが Unity 標準の並び替え可能なリスト表示に変更 (#328)
- MenuItem の配置に調整 (#329)
- TexTransTool によって新しく生成されたテクスチャーの isReadable が無効に、StreamingMipmap を有効化するように変更 (#331)
- AtlasTexture の WriteOriginalUV を実験的機能に変更 (#334)

### Removed

- 色合成の改修に伴い ClassicNormal は削除されました (#237)
- TexTransListGroup は削除されました (#230)
- AtlasTexture の改修に伴い EvenlySpaced, NextFitDecreasingHeight は削除されました (#252)
- Decal 系の ExtractDecalCompiledTexture は削除されました (#283)
- VRCAvatarCallBackToProcessAvatar は削除されました (#302)
- AtlasTexture の UseIslandCache グローバル設定に追加され、それに伴って削除されました (#312)

### Fixed

- 一部の色合成が一般的なソフトと大きく異なっている問題を修正 (#237)
- AtlasTexture の FineTuning の Resize の品質が低い問題を修正 (#96)
- Decal などのコンポーネントがオリジナルのテクスチャーを取得する際、不必要に別のインスタンスを生成していた問題を修正 (#249)
- 一部の衣装などで AtlasTexture の NFDHPlasFC が正常に並び替えできない問題を修正 (#255)
- CylindricalCurveDecal でセグメントの座標が重複したり同一のセグメントが複数個入っていた場合に無限ループが発生する問題を修正 (#273)
- 透過合成周りで黒いふちがプレビューで発生する問題を修正 (#274)
- AtlasTexture でアトラス化した時にテクスチャがずれる問題を修正 (#280)
- Shader が Null となり BlendTexture 全般が動作しなくなる問題を修正 (#278)
- Blend 用の Shader が初期化されていないタイミングで BlendTypeKey のプロパティを描画しようとしたときに例外が発生する問題を修正 (#284)
- Target のプロパティを持ったテクスチャーが存在しない場合 ReferenceCopy が動作しない問題を修正 (#289)
- (#278) での変更で Apply On Play に限り Shader が Null となる問題を修正 (#288)
- プレビュー中にプレビューしているコンポーネントを削除したとき、プレビューが解除されない問題を修正 (#300)(#337)
- PreviewCancelerPass が追加され、プレビューの状態のままアップロードできてしまう問題を修正 (#299)
- (#202)で発生した FixedAspect がデカールテクスチャーが存在しないときに、正常に動作しない問題を修正 (#303)

### Deprecated

## [v0.4.5](https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.4...v0.4.5) - 2024-01-10

### Fixed

- Cherry-Pick AtlasTexture でアトラス化した時にテクスチャがずれる問題を修正 (#280)
- Cherry-Pick 一部の衣装などで AtlasTexture の NFDHPlasFC が正常に並び替えできない問題を修正 (#255)

## [v0.4.4](https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.3...v0.4.4) - 2023-11-03

### Fixed

- AtlasTexture の liltoonAtlasSupport のマットキャップマスクの対応漏れ修正 (#210)

## [v0.4.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.2...v0.4.3) - 2023-10-15

### Fixed

- ターゲットプロパティの違うデカールをリアルタイムプレビューしたときに起きるバグを修正 (#190)

## [v0.4.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.1...v0.4.2) - 2023-10-09

### Fixed

- 内部的な色空間の変換ミスを修正しました。[コミット](https://github.com/ReinaS-64892/TexTransTool/commit/012af2aaaad5d53bef87745f7c03cc9bde6b0440)
- PSD をテクスチャーに使用している場合正常にデカールなどが使用できない問題を修正しました [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/909a487491b8986862030ad6206389a6997dfd3e)
- ネイティブサイズを使用しないことで、二のべき乗の解像度ではない画像になることを修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/9288cba0b0f621c85003601e1d14bf5e35026830)
- デカール系や AtlasTexture で圧縮されていないテクスチャーを使用するように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/0faa9177c0138230cce6c40274024edef5a79610)

## [v0.4.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.0...v0.4.1) - 2023-10-08

### Fixed

- 複数同時のリアルタイムプレビューができない問題を修正 (#189)

## [v0.4.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.6...v0.4.0) - 2023-10-07

### Added

- UVtoIsland の高速化 (#137)
- TexTransTool だけのマニュアルベイクアバターを追加 (#152)
- TexTransParentGroup に簡易表示リストを追加 (#156)
- 開発中であるコンポーネントに対して、インスペクターに警告を追加 (#157)
- MatAndTexAbsolute(Relative)Separator を追加 (#151) (#154)
- [マニュアル](Manual/JP/TextureTransformer.md)に書かれたことに沿うように、複数のコンポーネントを付けれないようにする属性を追加 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/b920e634554ebf1cddc7d21885ce791d110487cd)
- Phase と PhaseDefinition の追加 (#159)
- マテリアルの設定を変更できる MaterialModifier の追加 (#61)
- ユニティエディターのプログレスバーを追加 (#98)
- 一部のマテリアルの直接参照を持つコンポーネントがそれらを書き換わっても動くような仕組みを追加 (#173)
- SimpleDecal 以外の Decal 系 component のギズモに、DecalTexture を表示するギズモを追加 (#155)
- SimpleDecal のリアルタイムプレビューが複数同時に使用できる機能を追加 (#144)
- IslandCulling のレイキャスト処理の高速化 (#172)
- デカール系の余白生成が少し改善 (#79)
- 主なコンポーネントに日本語 UI を追加 (#73)
- デカール系に HighQualityPadding を追加 (#180)
- NDMF 対応 (#139)

### Changed

- AtlasTexture がマテリアルのインデックスではなく、直接の参照を持つように変更 (#146)
- 名前変更 TexTransParentGroup => TexTransGroup (#159)
- すべての AddComponent から追加できるコンポーネントの名前に TTT を追加しました。 (#122)
- Decal 系統は DecalTexture がセットされていなくても、単色のデカールを貼り付けれるように変更 (#124)
- liltoon の宝石やファー用のテクスチャーをアトラス化の”対象”に入れるように追加しました。 (#126)
- 自動生成ファイルのディレクトリを同じものが大量に生成されうるものは分けるように変更 (#119)

### Removed

- AtlasTexture の Channel を削除 (#146)
- Decal 系統の IsSeparateMatAndTexture の削除 (#151)
- AvatarDomainDefinition の削除 (#159)
- コンピュートシェーダーを用いた Decal のコンパイルは削除されました (#144)
- SimpleDecal にレンダラーの自動選択機能の追加はキャンセルされました (#94) (#185)

### Fixed

- Mac ですべてのコンポーネントが正常に動かなかったことを修正 (ただし、サポートは今のところしません) (#138)
- Unity のアニメーションのプレビューを使用し、プレハブオーバーライドを生成してしまう問題を修正 (#143)
- 圧縮しない設定ができない問題を修正 (#120)
- AtlasTexture の NextFitDecreasingHeightPlusFloorCeiling アルゴリズムで、横幅が大きい UVIsland が存在する場合うまく処理できない問題を修正 (#168)
- AtlasTexture の NextFitDecreasingHeightPlusFloorCeiling アルゴリズムで、上の余白が多きすぎる問題を修正 (#129)
- 内部的に使用されているレンダーテクスチャなどのフォーマットを調整しました (#187)

### Deprecated

- 名前変更と Deprecated にマーク TexTransGroup => TexTransListGroup (#159)

## [v0.3.6](https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.5...v0.3.6) - 2023-09-12

### Fixed

- TexTransTool のコンポーネントを一切使わず、TexTransToolGenerates/TempDirectory が生成されていない状態でアップロードできない問題を修正 (#147)

## [v0.3.5](https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.4...v0.3.5) - 2023-09-08

### Fixed

- TexTrans(Parent)Group または AvatarDomainDefinition の対象にすでにプレビューされている物があるとき、プレビューを実行できないように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/719ee708506530c1c104d49fa05b13776327c291)

## [v0.3.4](https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.3...v0.3.4) - 2023-09-07

### Fixed

- 一時的な適応である Preview の表記が Apply という意図に反したものになっていたのを修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/2f96c9acc743adb9566c014e153f57ab19744779)
- Decal を Preview をした後に Revert せずに、シーンをセーブした後に再度ロードした時に、正常に Revert できない問題を修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/01b026c2ea4b77c350ec6bbbed499783b55d31e6)
- AtlasTexture が Preview したときに、設定が変更できてしまう不具合を修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/9c8ee4aba0439aefd5c8ccc5ef0a01ea09d6e590)
- SimpleDecal のリアルタイムプレビューが、ほかの SimpleDecal によってされている場合に、警告を出し、中断するように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/ccf2ea1feebaaf4bba26f781ba6ac7e47acc0bf7)
- シーンのリロードやスクリプトのリロードなどでリアルタイムプレビューが継続できなくなったときに自動的にプレビューを中断するように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/ccf2ea1feebaaf4bba26f781ba6ac7e47acc0bf7)

## [v0.3.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.2...v0.3.3) - 2023-09-07

### Added

- AvatarDomainDefinition や TexTrans(Parent)Group で適応した Decal や AtlasTexture などがエラーを発生させた場合に、元に戻す復元措置を追加しました。[コミット 1](https://github.com/ReinaS-64892/TexTransTool/commit/97ffb3eff3fcdc7586e908d79a014adf22701d2d) [コミット 2](https://github.com/ReinaS-64892/TexTransTool/commit/d40158e2f032bfd198f3032a61d48ddb69d0c2fb)

### Fixed

- Decal などを使用せず AtlasTexture だけ使用した場合に発生するエラーを修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/90f290d0054199ef93425123f5e72a9d83729f4e)
- Decal や AtlasTexture に、何らかの問題で正しく適応できない場合に警告を出すように修正しました。[コミット](https://github.com/ReinaS-64892/TexTransTool/commit/7e0296d57221d6ad22de85a5d02f6298442ea821)

## [v0.3.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.1...v0.3.2) - 2023-09-07

### Added

- v0.3.2 から v0.4.0 へのマイグレーションをサポート
- UVtoIsland を実行時に一時的なプログレスバーを追加 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/a7fafcdc9e351105106fdc55d151e33878d8b65d)

### Deprecated

- AtlasTexture Channel [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/567ed551341f2d6888fe9d17b760e9314609d5ab)

### Fixed

- 無効化 or EditorOnly なメッシュ(レンダラー)を無視するように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/63d8428020e4dc9f134eea90f3d9adb41e595052)
- UV の存在しないメッシュを無視するように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/2c9368d2f059597fa35a1e9e6017f0841c1226b5)

## [v0.3.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.0...v0.3.1) - 2023-09-05

### Fixed

- ターゲットアバターのプロパティがなく正常に使用できない問題を修正 (#123)

## [v0.3.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.2.2...v0.3.0) - 2023-09-04

### Added

- すべてのコンポーネントの大幅な高速化 (#53)
- SimpleDecal UV のひとまとまりだけにデカールをマスクできる、アイランドカリングを追加 (#47)
- SimpleDecal デカールの色を乗算で色調整できる機能を追加 (#69)
- SimpleDecal インスペクターの作り直し + 詳細設定 (#59)
- AvatarDomainDefinition が AvatarBuildApplyHook の削除に伴いビルド時のマーカーの役割が追加 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/4ffc1b9d11e9cf491e485379d5694e048c791737)
- AtlasTexture を追加 (#46)
- CylindricalDecal を追加 (#43)
- NailEditor を追加 (#48)
- CylindricalCurveDecal 改修など (#42)

### Removed

- Compile 機能を高速化に伴い削除 Decal [コミット](https://github.com/ReinaS-64892/TexTransTool/pull/53/commits/959064ba5e4f3acc1e6784636e7967ad7aad2602)
- Compile 機能を高速化に伴い削除 AtlasTexture (#84)
- AtlasSet は AtlasTexture に作り直されて削除 (#46)
- AvatarBuildApplyHook を削除 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/4ffc1b9d11e9cf491e485379d5694e048c791737)
- SimpleDecal の AdvancedMode を削除[コミット](https://github.com/ReinaS-64892/TexTransTool/commit/351a53a237af61c852f509c99ac6a51444237bc2)

### Fixed

- 様々なスペルミスの修正 (#99 #102)

## [v0.2.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.2.1...v0.2.2) - 2023-06-23

### Fixed

- NextFitDecreasingHeightPlusFloorCeiling の計算の時に浮動小数点誤差により、正常に並び替えができてない問題を修正しました [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/a15bba3ddc785c6fcc197b1005623ac8a1d1c363)

## [v0.2.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.2.0...v0.2.1) - 2023-06-23

### Fixed

- VPM 対応の際に Path が変わっていて、 ComputeShader のパスが無効になっていたのを修正しました [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/6e687119d47c0f76c09d394e2e30705589250235)

## [v0.2.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.1.1...v0.2.0) - 2023-06-23

### Added

- VPM をサポートしました！ [VPM Link!](https://vpm.rs64.net/add-repo)
- SimpleDecal の簡易的なリアルタイムプレビューを追加しました (#25)
- AtlasSet UV にオフセットをかけて大きさの比率を調整できる機能を追加しました (#27)
- AtlasSet UV 並び替えアルゴリズム NextFitDecreasingHeightPlusFloorCeiling を追加しました (#38)
- UVtoIsland のキャッシングを追加しました (#31)
- TexTransParentGroup を追加しました (#24 #22)
- TransMapper を最適化し、このツールのコンポーネントが全体的に動作が速くなりました (#35)
- TransCompiler を最適化し、このツールのコンポーネントが全体的に動作が速くなりました (#37)

### Changed

- AvatarMaterialDomain を AvatarDomainDefinition に、MaterialDomain を AvatarDomain に名前を変更しました (#30)

## [v0.1.1](https://github.com/ReinaS-64892/TexTransTool/compare/0.1.0...v0.1.1) - 2023-06-06

### Fixed

- AtlasSet
- - Null のマテリアルが表示される問題の修正 (#17)
- - Mesh が Null のレンダラーが存在すると正常に実行できない問題の修正 (#20)
-

## [v0.1.0](https://github.com/ReinaS-64892/TexTransTool/releases/tag/0.1.0) - 2023-06-02

### Added

- TexTransGroup の追加 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/4f3b0abe08f232ec0a9a28ea15ac89fda0cf4948)
- SimpleDecal の追加 (#2)
- ブレンドモードの追加 (#4)
- AdvansdMode の追加 (#5)
- TransCompile を ComputeShader で作り直しました (#6)
- AvatarMaterialDomain と MaterialDomain、AvatarBuildAppryHook 追加 (#7)
- 開発中の機能 CylindricalCurveDecal を追加しました (#9)
- TextureBlender を追加しました (#14)

### Changed

- ツールの名前を TexturAtlasCompiler から TexTransTool に変更しました (#3)
- Assets から Packages に移動 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/46c8ed48da513914d4e3f2f67b8cdac900d285ae)
- AtlasSet の Atlas 化対象をレンダラーベースではなく、マテリアルの選択ベースに変更 (#8)

### Removed

- AvatarTag 系のコンポーネントを削除しました (#11)
