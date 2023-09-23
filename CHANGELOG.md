# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- UVtoIsland の高速化 `#137`
- TexTransTool だけのマニュアルベイクアバターを追加 `#152`
- TexTransParentGroup に簡易表示リストを追加 `#156`
- 開発中であるコンポーネントに対して、インスペクターに警告を追加 `#157`
- MatAndTexAbsolute(Relative)Separator を追加 `#151` `#154`
- [マニュアル](Manual/JP/TextureTransformer.md)に書かれたことに沿うように、複数のコンポーネントを付けれないようにする属性を追加 [コミット](https://github.com/ReinaS-64892/TexTransTool/commit/b920e634554ebf1cddc7d21885ce791d110487cd)
- Phase と PhaseDefinition の追加 `#159`

### Changed

- AtlasTexture がマテリアルのインデックスではなく、直接の参照を持つように変更 `#146`
- 名前変更 TexTransParentGroup => TexTransGroup `#159`
- すべての AddComponent から追加できるコンポーネントの名前に TTT を追加しました。 `#122`
- Decal 系統は DecalTexture がセットされていなくても、単色のデカールを貼り付けれるように変更 `#124`
- liltoonの宝石やファー用のテクスチャーをアトラス化の”対象”に入れるように追加しました。 `#126`
- 自動生成ファイルのディレクトリを同じものが大量に生成されうるものは分けるように変更 `#119`

### Removed

- AtlasTexture の Channel を削除 `#146`
- Decal 系統の IsSeparateMatAndTexture の削除 `#151`
- AvatarDomainDefinition の削除 `#159`

### Fixed

- Mac ですべてのコンポーネントが正常に動かなかったことを修正 (ただし、サポートは今のところしません) `#138`
- Unity のアニメーションのプレビューを使用し、プレハブオーバーライドを生成してしまう問題を修正 `#143`
- 圧縮しない設定ができない問題を修正 `#120`
- AtlasTexture の NextFitDecreasingHeightPlusFloorCeiling アルゴリズムで、横幅が大きい UVIsland が存在する場合うまく処理できない問題を修正 `#168`

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

[unreleased]: https://github.com/ReinaS-64892/TexTransTool/compare/v0.3.6...master
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
