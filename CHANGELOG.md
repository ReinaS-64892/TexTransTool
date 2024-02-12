# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- プレビューを再度実行できるメニューアイテム & ショートカット(Shift + R)が追加 (#357)
- ローカライズの仕組みが改修され (#360)
  - AtlasTexture と SimpleDecal が正しく実行不可のエラーを NDMF のエラーレポートに出力するようになりました。
  - ほとんどのプロパティにツールチップが追加されました。

### Changed

### Removed

### Fixed

- BlendTypeKey の UI でプレハブオーバーライドの操作が正しく右クリックから呼び出せるようになりました。(#358)
- PropertyName の UI でプレハブオーバーライドの操作が正しく右クリックから呼び出せるようになりました。(#362)

### Deprecated

## [0.5.4]

### Fixed

- LogoTexture が NDMF v1.3.x ではない環境で存在しないためコンパイルエラーになる問題を修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/86183088ec5b362700becad4c5baa283a174b392)
- BlendTypeKey LinearLight と Addition の色合成を修正 `#354`
- AtlasTexture のプロパティベイクが値は同じだけどほかにテクスチャーが存在する場合、既定の値のテクスチャーを生成し忘れていた問題の修正 `#355`
- BlendTypeKey VividLight の色合成を修正 `#356`

## [0.5.3]

### Added

- NDMFのエラーレポートに最低限の対応が追加 `#349`

## [0.5.2]

### Fixed

- TextureSelector が NullReferenceException を吐き続ける問題を修正 `#342`
- ColorDodge,ColorBurn (覆い焼カラー、焼きこみカラー) の色合成を修正 `#344`

## [0.5.1]

### Fixed

- ブレンドタイプキー SoftLight が一般的な画像編集ソフトに近くなりました `#295`

## [0.5.0]

### Added

- 画像編集ソフトのコンポジットを再現する MultiLayerImageCanvas を追加 `#160`
  - それに伴い LayerFolder と RasterLayer を追加
  - 特殊なレイヤーとして SolidLayer を追加
  - PSD の importer が追加され、アセットの右クリックメニューに TexTransTool/TTT PSD Importer が追加
- ReferenceResolver が追加されました `#183`
- BeforeUVModification と UVModification の間にスタックをマージし、デカール系統や MultiLayerImageCanvas の効果が消えないように MidwayMergeStack を追加 `#200`
- SimpleDecal のインスペクターに複数編集を追加 `#203`
- ~~未知のシェーダーですべてのテクスチャーをアトラス化の対象にするオプション UnknownShaderAtlasAllTexture を追加~~ キャンセルされました`#321`
- MultiLayerImageCanvas と TextureBlender にリゾルバー AbsoluteTextureResolver を追加 `#216`
- Preview でもビルドと同じようにアバター全体の影響になる DomainMarkerFinder を追加 `#219`
- 実験的機能として、GrabDecal が SimpleDecal に追加 `#233`
- メッシュとテクスチャーの置き換えも適用する機能を追加 `#232`
- MenuItem の Language に CheckMark の表示を追加 `#233`
- 非常に実験的な機能として UseDepth と DepthInvert が追加 `#150`
- 無効化されたレンダラーもアトラス化の対象にする IncludeDisableRenderer を追加 `#222`
- 非常に実験的な API を追加 `#258`
- AtlasTexture のアトラス化するときに、アップスケーリングを許可するオプションを追加 `#279`
- 最低限のNDMFのErrorReportへの対応 `#293`
- プレビューの場合でもマテリアルの置き換えによる実行不可や設定が外れる問題を解決する ReplacementQuery を追加 `#318`
- グローバル設定として、 UseIslandCache が追加 `#321`
- SimpleDecal のリアルタイムプレビューが複数同時に行われていた場合に、すべてのリアルタイムプレビューを終了するボタンを追加 `#320`
- AtlasTexture の FineTuning の Compress に UseOverrideTextureFormat とそれに伴う様々が追加 `#326`
- AtlasTexture の FineTuning に色空間の設定ができる ColorSpace が追加 `#326`

### Changed

- SimpleDecal のインスペクターのサイズ調整機能が直接トランスフォームを変更する形に変更 `#202`
- Mesh の UV1 に元の UV を書き込む機能 WriteOriginalUV をオプションに変更 `#204`
- "Exit RealTime Previews" は 通常の Preview も終了する "Exit Previews" に変更 `#197`
- Texture2D のプロパティがプレハブオーバーライドをコントロールできるように変更 `#217`
- マテリアルの置き換えをほかのコンポーネントにも適用する機能をオプションに変更 `#232`
  - その機能は実験的機能に変更
- Preview の場合オリジナルのテクスチャを取得しなくなり、結果のテクスチャを圧縮しないように変更 `#231 #186`
- SimpleDecal の IslandCulling を実験的機能に変更 `#229`
- SimpleDecal の Far Culling の基準を変更 `#242`
- PropertyName の保存形式を調整し、データが勝手に変わらないように変更 `#228`
- Texture のブレンドを即時実行するように変更し、VRAM + RAM 容量が Decal などのコンポーネントの最大数にならないように変更 `#188`
- AtlasTexture や Decal などのセーブデータに大きな変更 `#252 #256`
- ~~Unity の最小バージョン指定を 2021.3 に変更 `#260`~~
- AtlasTexture のアトラス化するとき、標準の動作はアップスケーリングできない仕様に変更 `#279`
- 他がプレビュー中のときにプレビューができないのではなく、プレビューを乗っ取るボタンに変更 `#298`
- Unity の最小バージョン指定を 2022.3 に変更 `#310`
- SimpleDecal のリアルタイムプレビューが可変レートで更新されるように変更 `#320`
- AtlasTexture の TextureFineTuning などが Unity 標準の並び替え可能なリスト表示に変更 `#328`
- MenuItem の配置に調整 `#329`
- TexTransTool によって新しく生成されたテクスチャーの isReadable が無効に、StreamingMipmap を有効化するように変更 `#331`
- AtlasTexture の WriteOriginalUV を実験的機能に変更 `#334`

### Removed

- 色合成の改修に伴い ClassicNormal は削除されました `#237`
- TexTransListGroup は削除されました `#230`
- AtlasTexture の改修に伴い EvenlySpaced, NextFitDecreasingHeight は削除されました `#252`
- Decal 系の ExtractDecalCompiledTexture は削除されました `#283`
- VRCAvatarCallBackToProcessAvatar は削除されました `#302`
- AtlasTexture の UseIslandCache グローバル設定に追加され、それに伴って削除されました `#312`

### Fixed

- 一部の色合成が一般的なソフトと大きく異なっている問題を修正 `#237`
- AtlasTexture の FineTuning の Resize の品質が低い問題を修正 `#96`
- Decal などのコンポーネントがオリジナルのテクスチャーを取得する際、不必要に別のインスタンスを生成していた問題を修正 `#249`
- 一部の衣装などで AtlasTexture の NFDHPlasFC が正常に並び替えできない問題を修正 `#255`
- CylindricalCurveDecal でセグメントの座標が重複したり同一のセグメントが複数個入っていた場合に無限ループが発生する問題を修正 `#273`
- 透過合成周りで黒いふちがプレビューで発生する問題を修正 `#274`
- AtlasTexture でアトラス化した時にテクスチャがずれる問題を修正 `#280`
- Shader が Null となり BlendTexture 全般が動作しなくなる問題を修正 `#278`
- Blend 用の Shader が初期化されていないタイミングで BlendTypeKey のプロパティを描画しようとしたときに例外が発生する問題を修正 `#284`
- Target のプロパティを持ったテクスチャーが存在しない場合 ReferenceCopy が動作しない問題を修正 `#289`
- `#278` での変更で Apply On Play に限り Shader が Null となる問題を修正 `#288`
- プレビュー中にプレビューしているコンポーネントを削除したとき、プレビューが解除されない問題を修正 `#300``#337`
- PreviewCancelerPass が追加され、プレビューの状態のままアップロードできてしまう問題を修正 `#299`
- `#202`で発生した FixedAspect がデカールテクスチャーが存在しないときに、正常に動作しない問題を修正 `#303`

### Deprecated

## [0.4.5]

### Fixed

Cherry-Pick

> - AtlasTexture でアトラス化した時にテクスチャがずれる問題を修正 `#280`
> - 一部の衣装などで AtlasTexture の NFDHPlasFC が正常に並び替えできない問題を修正 `#255`

## [0.4.4]

### Fixed

- AtlasTexture の liltoonAtlasSupport のマットキャップマスクの対応漏れ修正 `#210`

## [0.4.3]

### Fixed

- ターゲットプロパティの違うデカールをリアルタイムプレビューしたときに起きるバグを修正 `#190`

## [0.4.2]

### Fixed

- 内部的な色空間の変換ミスを修正しました。[コミット](https://github.com/ReinaS-64892/TexTransTool/commit/012af2aaaad5d53bef87745f7c03cc9bde6b0440)
- PSD をテクスチャーに使用している場合正常にデカールなどが使用できない問題を修正しました [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/909a487491b8986862030ad6206389a6997dfd3e)
- ネイティブサイズを使用しないことで、二のべき乗の解像度ではない画像になることを修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/9288cba0b0f621c85003601e1d14bf5e35026830)
- デカール系や AtlasTexture で圧縮されていないテクスチャーを使用するように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/0faa9177c0138230cce6c40274024edef5a79610)

## [0.4.1]

### Fixed

- 複数同時のリアルタイムプレビューができない問題を修正 `#189`

## [0.4.0]

### Added

- UVtoIsland の高速化 `#137`
- TexTransTool だけのマニュアルベイクアバターを追加 `#152`
- TexTransParentGroup に簡易表示リストを追加 `#156`
- 開発中であるコンポーネントに対して、インスペクターに警告を追加 `#157`
- MatAndTexAbsolute(Relative)Separator を追加 `#151` `#154`
- [マニュアル](Manual/JP/TextureTransformer.md)に書かれたことに沿うように、複数のコンポーネントを付けれないようにする属性を追加 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/b920e634554ebf1cddc7d21885ce791d110487cd)
- Phase と PhaseDefinition の追加 `#159`
- マテリアルの設定を変更できる MaterialModifier の追加 `#61`
- ユニティエディターのプログレスバーを追加 `#98`
- 一部のマテリアルの直接参照を持つコンポーネントがそれらを書き換わっても動くような仕組みを追加 `#173`
- SimpleDecal 以外の Decal 系 component のギズモに、DecalTexture を表示するギズモを追加 `#155`
- SimpleDecal のリアルタイムプレビューが複数同時に使用できる機能を追加 `#144`
- IslandCulling のレイキャスト処理の高速化 `#172`
- デカール系の余白生成が少し改善 `#79`
- 主なコンポーネントに日本語 UI を追加 `#73`
- デカール系に HighQualityPadding を追加 `#180`
- NDMF 対応 `#139`

### Changed

- AtlasTexture がマテリアルのインデックスではなく、直接の参照を持つように変更 `#146`
- 名前変更 TexTransParentGroup => TexTransGroup `#159`
- すべての AddComponent から追加できるコンポーネントの名前に TTT を追加しました。 `#122`
- Decal 系統は DecalTexture がセットされていなくても、単色のデカールを貼り付けれるように変更 `#124`
- liltoon の宝石やファー用のテクスチャーをアトラス化の”対象”に入れるように追加しました。 `#126`
- 自動生成ファイルのディレクトリを同じものが大量に生成されうるものは分けるように変更 `#119`

### Removed

- AtlasTexture の Channel を削除 `#146`
- Decal 系統の IsSeparateMatAndTexture の削除 `#151`
- AvatarDomainDefinition の削除 `#159`
- コンピュートシェーダーを用いた Decal のコンパイルは削除されました `#144`
- SimpleDecal にレンダラーの自動選択機能の追加はキャンセルされました `#94` `#185`

### Fixed

- Mac ですべてのコンポーネントが正常に動かなかったことを修正 (ただし、サポートは今のところしません) `#138`
- Unity のアニメーションのプレビューを使用し、プレハブオーバーライドを生成してしまう問題を修正 `#143`
- 圧縮しない設定ができない問題を修正 `#120`
- AtlasTexture の NextFitDecreasingHeightPlusFloorCeiling アルゴリズムで、横幅が大きい UVIsland が存在する場合うまく処理できない問題を修正 `#168`
- AtlasTexture の NextFitDecreasingHeightPlusFloorCeiling アルゴリズムで、上の余白が多きすぎる問題を修正 `#129`
- 内部的に使用されているレンダーテクスチャなどのフォーマットを調整しました `#187`

### Deprecated

- 名前変更と Deprecated にマーク TexTransGroup => TexTransListGroup `#159`

## [0.3.6]

### Fixed

- TexTransTool のコンポーネントを一切使わず、TexTransToolGenerates/TempDirectory が生成されていない状態でアップロードできない問題を修正 `#147`

## [0.3.5]

### Fixed

- TexTrans(Parent)Group または AvatarDomainDefinition の対象にすでにプレビューされている物があるとき、プレビューを実行できないように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/719ee708506530c1c104d49fa05b13776327c291)

## [0.3.4]

### Fixed

- 一時的な適応である Preview の表記が Apply という意図に反したものになっていたのを修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/2f96c9acc743adb9566c014e153f57ab19744779)
- Decal を Preview をした後に Revert せずに、シーンをセーブした後に再度ロードした時に、正常に Revert できない問題を修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/01b026c2ea4b77c350ec6bbbed499783b55d31e6)
- AtlasTexture が Preview したときに、設定が変更できてしまう不具合を修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/9c8ee4aba0439aefd5c8ccc5ef0a01ea09d6e590)
- SimpleDecal のリアルタイムプレビューが、ほかの SimpleDecal によってされている場合に、警告を出し、中断するように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/ccf2ea1feebaaf4bba26f781ba6ac7e47acc0bf7)
- シーンのリロードやスクリプトのリロードなどでリアルタイムプレビューが継続できなくなったときに自動的にプレビューを中断するように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/ccf2ea1feebaaf4bba26f781ba6ac7e47acc0bf7)

## [0.3.3]

### Added

- AvatarDomainDefinition や TexTrans(Parent)Group で適応した Decal や AtlasTexture などがエラーを発生させた場合に、元に戻す復元措置を追加しました。[コミット 1](https://github.com/ReinaS-64892/TexTransTool/commit/97ffb3eff3fcdc7586e908d79a014adf22701d2d) [コミット 2](https://github.com/ReinaS-64892/TexTransTool/commit/d40158e2f032bfd198f3032a61d48ddb69d0c2fb)

### Fixed

- Decal などを使用せず AtlasTexture だけ使用した場合に発生するエラーを修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/90f290d0054199ef93425123f5e72a9d83729f4e)
- Decal や AtlasTexture に、何らかの問題で正しく適応できない場合に警告を出すように修正しました。[コミット](https://github.com/ReinaS-64892/TexTransTool/commit/7e0296d57221d6ad22de85a5d02f6298442ea821)

## [0.3.2]

### Added

- v0.3.2 から v0.4.0 へのマイグレーションをサポート
- UVtoIsland を実行時に一時的なプログレスバーを追加 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/a7fafcdc9e351105106fdc55d151e33878d8b65d)

### Deprecated

- AtlasTexture Channel [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/567ed551341f2d6888fe9d17b760e9314609d5ab)

### Fixed

- 無効化 or EditorOnly なメッシュ(レンダラー)を無視するように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/63d8428020e4dc9f134eea90f3d9adb41e595052)
- UV の存在しないメッシュを無視するように修正 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/2c9368d2f059597fa35a1e9e6017f0841c1226b5)

## [0.3.1]

### Fixed

- ターゲットアバターのプロパティがなく正常に使用できない問題を修正 `#123`

## [0.3.0]

### Added

- すべてのコンポーネントの大幅な高速化 `#53`
- SimpleDecal
  - UV のひとまとまりだけにデカールをマスクできる、アイランドカリングを追加 `#47`
  - デカールの色を乗算で色調整できる機能を追加 `#69`
  - インスペクターの作り直し + 詳細設定 `#59`
- AvatarDomainDefinition が AvatarBuildApplyHook の削除に伴いビルド時のマーカーの役割が追加 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/4ffc1b9d11e9cf491e485379d5694e048c791737)
- AtlasTexture を追加 `#46`
- CylindricalDecal を追加 `#43`
- NailEditor を追加 `#48`
- CylindricalCurveDecal 改修など `#42`

### Removed

- Compile 機能を高速化に伴い削除
  - Decal [コミット](https://github.com/ReinaS-64892/TexTransTool/pull/53/commits/959064ba5e4f3acc1e6784636e7967ad7aad2602)
  - AtlasTexture `#84`
- AtlasSet は AtlasTexture に作り直されて削除 `#46`
- AvatarBuildApplyHook を削除 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/4ffc1b9d11e9cf491e485379d5694e048c791737)
- SimpleDecal の AdvancedMode を削除[コミット](https://github.com/ReinaS-64892/TexTransTool/commit/351a53a237af61c852f509c99ac6a51444237bc2)

### Fixed

- 様々なスペルミスの修正 `#99 #102`

## [0.2.2]

### Fixed

- NextFitDecreasingHeightPlusFloorCeiling の計算の時に浮動小数点誤差により、正常に並び替えができてない問題を修正しました [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/a15bba3ddc785c6fcc197b1005623ac8a1d1c363)

## [0.2.1]

### Fixed

- VPM 対応の際に Path が変わっていて、 ComputeShader のパスが無効になっていたのを修正しました [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/6e687119d47c0f76c09d394e2e30705589250235)

## [0.2.0]

### Added

- VPM をサポートしました！ [VPM Link!](https://vpm.rs64.net/add-repo)
- SimpleDecal の簡易的なリアルタイムプレビューを追加しました `#25`
- AtlasSet
  - UV にオフセットをかけて大きさの比率を調整できる機能を追加しました `#27`
  - UV 並び替えアルゴリズム NextFitDecreasingHeightPlusFloorCeiling を追加しました `#38`
  - UVtoIsland のキャッシングを追加しました `#31`
- TexTransParentGroup を追加しました `#24 #22`
- TransMapper を最適化し、このツールのコンポーネントが全体的に動作が速くなりました `#35`
- TransCompiler を最適化し、このツールのコンポーネントが全体的に動作が速くなりました `#37`

### Changed

- AvatarMaterialDomain を AvatarDomainDefinition に、MaterialDomain を AvatarDomain に名前を変更しました `#30`

## [0.1.1]

### Fixed

- AtlasSet
  - Null のマテリアルが表示される問題の修正 `#17`
  - Mesh が Null のレンダラーが存在すると正常に実行できない問題の修正 `#20`

## [0.1.0]

### Added

- TexTransGroup の追加 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/4f3b0abe08f232ec0a9a28ea15ac89fda0cf4948)
- SimpleDecal の追加 `#2`
  - ブレンドモードの追加 `#4`
  - AdvansdMode の追加 `#5`
- TransCompile を ComputeShader で作り直しました `#6`
- AvatarMaterialDomain と MaterialDomain、AvatarBuildAppryHook 追加 `#7`
- 開発中の機能 CylindricalCurveDecal を追加しました `#9`
- TextureBlender を追加しました `#14`

### Changed

- ツールの名前を TexturAtlasCompiler から TexTransTool に変更しました `#3`
- Assets から Packages に移動 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/46c8ed48da513914d4e3f2f67b8cdac900d285ae)
- AtlasSet の Atlas 化対象をレンダラーベースではなく、マテリアルの選択ベースに変更 `#8`

### Removed

- AvatarTag 系のコンポーネントを削除しました `#11`

[unreleased]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.4...master
[0.5.3]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.3...v0.5.4
[0.5.3]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.2...v0.5.3
[0.5.2]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.1...v0.5.2
[0.5.1]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.5.0...v0.5.1
[0.5.0]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.5...v0.5.0
[0.4.5]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.4...v0.4.5
[0.4.4]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.3...v0.4.4
[0.4.3]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.2...v0.4.3
[0.4.2]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.1...v0.4.2
[0.4.1]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.4.0...v0.4.1
[0.4.0]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.6...v0.4.0
[0.3.6]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.5...v0.3.6
[0.3.5]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.4...v0.3.5
[0.3.4]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.3...v0.3.4
[0.3.3]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.2...v0.3.3
[0.3.2]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.1...v0.3.2
[0.3.1]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.0...v0.3.1
[0.3.0]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.2.2...v0.3.0
[0.2.2]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.2.1...v0.2.2
[0.2.1]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.2.0...v0.2.1
[0.2.0]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/ReinaS-64892/TexTransTool/compare/0.1.0...v0.1.1
[0.1.0]: https://github.com/ReinaS-64892/TexTransTool/releases/tag/0.1.0
