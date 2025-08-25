# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased](https://github.com/ReinaS-64892/TexTransTool/compare/v1.0.0...HEAD)

### Added

- TexTransTool が NDMF によるビルドの場合に MA MaterialSwap によって追加されるマテリアルに対して ほぼすべてのコンポーネントが影響を与えられるようになりました (#1037)
- UVDisassemblyPhase が追加されました (#1047)
- NDMF Preview にて 同一フェーズ ではなくても、 MaterialModifier などのマテリアル改変系コンポーネントの影響でプレビュー範囲が変わるコンポーネントが正しい範囲で行われるようになりました (#1051)

### Changed

- Migrator などの TTT の拡張 Window が全て TTT Menu に集約されました (#1066)

### Dependency

- TexTransCore v0.3.x を要求するようになりました。 (#1050)

## [v1.0.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.10...v1.0.0) - 2025-06-22

### Added

- NDMF によるビルド時に、マイグレーションを行っていないコンポーネントが存在した場合に警告を発生させるようになりました (#1010)
- TexTransTool v0.9.x またはそれ以前のバージョンからのマイグレーションが行われていない場合の警告を発生させるようになりました (#1021)

### Removed

- TexTransTool の Minor アップデートのときに表示される ProjectMigrationDialog を削除しました (#1008)
- TexTransTool の v0.8.x またはそれ以前のセーブデータを全て削除しました。その対象のセーブデータをマイグレーションする場合には古いバージョンを経由してください。(#1010)

## [v0.10.10](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.9...v0.10.10) - 2025-06-20

### Fixed

- IslandSelectorAND(OR,XOR) にて無効化されている IslandSelector を無視できていなかった問題を修正 (#1024)

## [v0.10.9](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.8...v0.10.9) - 2025-06-17

### Fixed

- AtlasTexture と AAO Remove Mesh By *** が同一のメッシュを対象としたときに、NDMF によるビルドの際に例外発生する問題を修正 (#1023)

## [v0.10.8](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.7...v0.10.8) - 2025-06-15

### Added

- SingleGradationDecal の グラデーションの長さ が、設定値が 0 ~ 2 の間はスライダーになるようになり Transform から直接操作した場合に従来の描画になるようになりました (#1018)
- SingleGradationDecal の ギズモ表示 が、グラデーションの設定された色を表示するようになりました (#1018)

### Fixed

- SingleGradationDecal の グラデーションの長さ が操作できない問題を修正しました (#1018)

### Dependency

- TTCE-Wgpu の オプション依存関係が v0.2.0 を要求するようになりました。(#1019)

## [v0.10.7](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.6...v0.10.7) - 2025-06-14

### Fixed

- AtlasTexture の TextureFineTuning を特定の状態になるように設定すると、実行時に TTCE-Wgpu バックエンドでのみ例外が発生する問題を修正 (#1012)
- BlendTypeKey が正しく言語設定のに応じて翻訳されない問題を修正 (#1014)
- 内部的なシェーダーの再インポートが必要なのにもかかわらず、強制的に発火させれていなかった問題を修正しました。(#1015)

### Dependency

- TexTransCore v0.2.x を要求する用意なりました。 (#1016)

## [v0.10.6](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.5...v0.10.6) - 2025-05-28

### Fixed

- AtlasTexture が ManualBake などで原点からアバターが離れていて NDMF-Preview ではない場合に IslandSelector が正しく動作しない問題を修正 (#999)
- AtlasTexture などが動作する OptimizingPhase が lilycalInventory よりも後に動作してしまいエラーが発生する問題を修正 (#1003)

## [v0.10.5](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.4...v0.10.5) - 2025-05-27

### Fixed

- AtlasTexture が UV0 以外を対象としていた時に、対象となる UV  を持たないメッシュがアトラスか対象になった時に例外が発生する問題を修正 (#995)
- AtlasTexture などの TexTransTool のコンポーネントが Linear なテクスチャーをロードすることに失敗する問題を修正 (#996)
- AtlasTexture TextureFineTuning Compress が未知のターゲットプラットフォーム設定の環境の場合に常に例外を発生させる問題を修正 (#997)

## [v0.10.4](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.3...v0.10.4) - 2025-05-24

### Fixed

- AtlasTexture が Mesh の Normalize が発生した時に、MaterialSlot が SubMesh よりも小さい場合に、SubMesh が MaterialSlot の数まで減ってしまう問題を修正 (#983)
- AtlasTexture が SubMesh よりも MaterialSlot が少ない場合に、例外が発生する問題を修正 (#983)
- TexTransTool が NDMF によるビルドの場合に MA MaterialSetter や Animation によって missing や null が追加されている場合に例外が発生する問題を修正 (#986)
- TexTransTool のコンポーネントが編集したテクスチャの黒に近い部分の階調が、Linear で一時的に保存されていたために失われる問題を修正 (#988)
- 一部のメッシュ軽量化系よりもあとに動作してしまう問題を修正しました (#989)

## [v0.10.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.2...v0.10.3) - 2025-05-17

### Fixed

- AtlasTexture が Assert の記述ミスによって常に例外が発生する問題を修正しました (#973)

## [v0.10.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.1...v0.10.2) - 2025-05-17

### Fixed

- AtlasTexture の ターゲットとなるマテリアルに、無効なレンダラーだけが持つマテリアルが存在した場合に例外が発生する問題を修正 (#971)

## [v0.10.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.10.0...v0.10.1) - 2025-05-16

## [v0.10.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.9.3...v0.10.0) - 2025-05-14

### Added

- AtlasTexture の MergeReference に対して割り当てられていた場合にも置き換えが登録されるようになり、アトラス化後のマテリアルを対象にコンポーネントが動作可能になりました (#900)
- AtlasTexture がアトラス化するとき、大幅に重なったアイランドが、結合可能なときは結合されて扱われるようになりました (#900)
- AtlasTexture TextureFineTuning の MipMapRemove は廃止され、 MiMap が追加されました (#900)
- AtlasTexture MargeMaterialGroup が stable としてマークされました (#900)
- TexTransTool に 日本語と英語 以外の言語を追加できる API が追加されました (#959)
- TexTransTool が NDMF によるビルドの場合に Animation によって追加されるマテリアルに対して ほぼすべてのコンポーネントが影響を与えられるようになりました (#962)
- インスペクターから 実験的機能の警告 や VRAMに影響を与えることを示すIcon などの表示を非表示にできる設定が追加されました(非表示にできるだけであり、実態には何ら影響しません) (#963)
- TexTransTool が NDMF によるビルドの場合に MA MaterialSetter によって追加されるマテリアルに対して ほぼすべてのコンポーネントが影響を与えられるようになりました (#965)

### Changed

- AtlasTexture の TextureFineTuning の初期設定が NormalMap などを考慮に入れた設定になりました (#900)
- AtlasTexture MergeMaterial は削除され、割り当てたときに、すべてのマテリアルをそれに結合する AllMaterialMergeReference に変更されました (#900)
- TexTransTool の Project に対する設定 (例えば言語設定など) が `Tools/TexTransTool/Menu` から開くことが可能な Window に移動しました (#932)
- TexTransGroup の削除に伴い、PhaseDefine 配下にないコンポーネントは上から順にフェーズごとに実行される、TexTransGroup が存在するときと同様の実行順になるように変更されました (#941)
- NormalMap は TTT の内部では常に RG で扱われるようになりました (#943)
- NDMF の対応バージョンが v1.7.0 以上に変更 (#962)

### Removed

- AtlasTexture BakeProperty は削除されました。代わりとなる機能は ドキュメントを参照してください。 (#900)
- AtlasTexture の "_MainTex" 以外のプロパティに自動的に最大サイズを割り当てる機能は削除されました (#900)
- AtlasTexture の LimitCandidateMaterials は削除されました (#923)
- TextureBlender などの テクスチャ選択の部分での選択モード Relative は削除され Absolute だけになりました (#938)
- Decal系の HighQualityPadding は削除されました (#939)
- TexTransGroup は削除されました (#941)

### Fixed

- AtlasTexture が非正方形なテクスチャを対象にアトラス化したときに Padding の計算が正しく行えていない問題を修正しました (#900)
- DirectX11 環境で GTX10 や GTX9 系で様々なコンポーネントが正常に動作しない問題を修正しました (#929)
- IslandSelector は、実行化時に無効化されていた場合に、無効化されるようになりました (#945)
- MeshData から Mesh の解放し忘れによって leak が発生していた問題を修正しました (#969)

### Dependency

- TexTransTool のコードの Core である部分が TexTransCore に移動し、TexTransCore に依存するようになりました。 (#888)

## [v0.9.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.9.2...v0.9.3) - 2025-03-11

### Fixed

- ShaderKeywords に影響を受けるシェーダーがプレビューの対象となったとき、マテリアルの複製時に ShaderKeywords がコピーされず表示がおかしくなってしまっていた問題を修正 (#893)

## [v0.9.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.9.1...v0.9.2) - 2025-03-03

## [v0.9.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.9.0...v0.9.1) - 2025-03-02

### Fixed

- SimpleDecal の ポリゴンカリング の実装が誤っていて、意図しないポリゴンにデカールが描画されてしまいちらつくことがある問題を修正しました (#885)

## [v0.9.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.13...v0.9.0) - 2025-02-17

### Added

- TexTransTool のほとんどのコンポーネントの処理使用される VRAM使用量が削減されました (#672)
- BlendTypeKey の ポップアップ のラベルが日本語化されるようになりました (#676)
- AAO:Avatar Optimizer の RemoveMeshBy*** と併用した時に AAO の API を用いて AtlasTexture が不要な領域をアトラス化しないようにする連携機能が追加されました (#670)
- AAO:Avatar Optimizer と AtlasTexture を併用した時に UV を退避し AAO の API に報告し、 AAO の UV を使用する機能と互換性を保つ機能が追加されました (#687)
- SimpleDecal や SingleGradationDecal の内部実装が通常のレンダリングを用いたものから ComputeShader 実装になり、パディング生成が v0.2.x の頃のような高品質なものになりました (#727)
- 一部の場合で Material が一時アセットだった場合に複製せずにテクスチャを置き換えるようになりました (#744)
- SimpleDecal に Select Mode が追加され 既存の方法は Manual に そして新規に、範囲内であれば自動で選択され、マテリアルでフィルタリングも可能な Auto が追加されました (#753)
- TexTransTool の内部処理で使用される RenderTexture の Format を指定できる設定 InternalTextureFormat が `Tools/TexTransTool` に追加されました (#774)
- NDMF Preview にて 同一フェーズ に限り、 MaterialOverrideTransfer などのマテリアル改変系コンポーネントの影響でプレビュー範囲が変わるコンポーネントが正しい範囲で行われるようになりました (#806 #828 #830)
- 色合成をしない特殊な色合成、 ExtraColorBlending に属するいくつかの色合成が追加されました (#812)
- MaterialModificationPhase と PostProcessingPhase が追加されました (#826)
- VRAM容量 (テクスチャーメモリやメッシュ) に影響を与えうる設定項目に対して、アイコンが表示されるようになりました (#839)
- SingleGradationDecal が Experimental ではなく、 Stable としてマークされました (#848)
- IslandSelectorAND(NOT,OR,XOR) と Box(Sphere,Pin)IslandSelector が Experimental ではなく、 Stable としてマークされました (#848 #865)
- TextureBlender が Experimental ではなく、 Stable としてマークされました (#857)
- SimpleDecal の IslandSelector が Experimental ではなく、Stable としてマークされました (#865)

### Removed

- SimpleDecal の MultiRendererMode は削除され、デフォルトで複数レンダラーを選択可能になりました (#753)
- SimpleDecal の PolygonCulling は修正に伴い削除されました (#856)

### Changed

- TexTransGroup や PhaseDefine がお互いどちらかが子になるような条項が発生した場合、一番上段に存在するほうの効果が優先されるようになりました (#694)
- SimpleDecal の SideCulling が BackCulling に名前変更されました (#692)
- コンポーネントを入れ子の状態にすると、入れ子にされたコンポーネントは動作しなくなるようになりました (#694)
- TextureStack のマージタイミングが全フェーズの直後に行われるように変更されました (#694)
- TextureStack のマージタイミングの変更に伴い NDMF-Preview のオンオフできるフェーズの単位が細かくなりました (#694)
- TexTransGroup や PhaseDefine が 子の一段目までを保証していたのが、再帰的にすべての子まで保証するようになりました (#802)

### Fixed

- AtlasTexture が誤って Renderer.enabled が無効なレンダラーを IncludeDisableRenderer が無効な場合に含めてしまっていた問題を修正 (#756)
- Migrator ウィンドウにて、マイグレーションする必要のない Prefab が選択できたり、実行対象に含まれてしまう問題を修正 (#779)
- Packages 配下にある Scene を誤ってマイグレーションしてしまっていた問題を修正 (#779)
- SimpleDecal がポリゴンに対して非常に小さい場合に、PolygonCulling が有効な場合正しく張り付けることができない問題が修正されました (#851)
- SingleGradationDecal のインスペクターのインデントが一部正しくなかった問題が修正されました (#868)

## [v0.8.13](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.12...v0.8.13) - 2025-01-09

### Fixed

- AAO:Avatar Optimizer との API を用いた連携の時に、 ポリゴンが存在しないサブメッシュが対象に含まれていると例外が発生する問題を修正 (#771)

## [v0.8.12](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.11...v0.8.12) - 2024-12-28

### Added

- AtlasTexture のインスペクターにマテリアルを 全選択 と 反転 できるボタンが追加されました (#763)

### Fixed

- AAO:Avatar Optimizer との API を用いた連携の時に、Meshの複製が Stack領域の不足が予期されるという例外が発生する問題を回避 (#761)
- TTT NegotiateAAOConfig の AAO の削除する領域をアトラス化しないようにする機能の設定のフィールド名が誤っていた問題を修正 (#762)

## [v0.8.11](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.10...v0.8.11) - 2024-12-24

### Fixed

- Mantis LOD Editor よりも後に実行される可能性があった問題を修正しました (#755)

## [v0.8.10](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.9...v0.8.10) - 2024-12-18

### Added

- Cherry-Pick AAO:Avatar Optimizer の RemoveMeshBy*** と併用した時に AAO の API を用いて AtlasTexture が不要な領域をアトラス化しないようにする連携機能が追加されました (#670 #749)
- Cherry-Pick AAO:Avatar Optimizer と AtlasTexture を併用した時に UV を退避し AAO の API に報告し、 AAO の UV を使用する機能と互換性を保つ機能が追加されました (#687 #749)

## [v0.8.9](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.8...v0.8.9) - 2024-12-09

### Fixed

- ファイルパスが非常に長いテクスチャーのソース画像にアクセスしようとしたとき GDI+ からの例外が発生する問題を修正しました (#742)
- AtlasTexture の lilToon 対応にて MatCap 関連が正しく処理できていなかった問題を修正 (#743)

## [v0.8.8](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.7...v0.8.8) - 2024-12-01

### Added

- NDMF v1.6.0 にて追加された AssetSaver API に対応 (#731)

## [v0.8.7](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.6...v0.8.7) - 2024-11-23

### Added

- ほかツールとの互換性向上のため、TTT が生成した Texture を基に置き換えが登録されたテクスチャーが存在した場合に圧縮が行われていなかったら TTT が持っている情報を基に圧縮を行うようになりました (#726)

## [v0.8.6](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.5...v0.8.6) - 2024-11-14

### Fixed

- AtlasTexture の オリジナルテクスチャーのロードだけが プレビューに誤って行われていた問題を修正 (#721)
- AtlasTexture が System.Drawing (Windows GDI) が存在しない環境で例外が発生し動作しない問題を修正しました (#722)

## [v0.8.5](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.4...v0.8.5) - 2024-11-13

### Added

- 二のべき乗の数値を指定するプロパティで直接入力が行えるモードに切り替えるトグルを追加 (#718)

### Fixed

- AtlasTexture の ForceSizePriority が縮小の必要がないケースにおいて縮小が強制的に行われなかった問題を修正 (#716)

## [v0.8.4](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.3...v0.8.4) - 2024-11-05

### Added

- AtlasTexture のオリジナルテクスチャーのロードが並列で行われるようになり高速化されました (#707)
- AtlasTexture でサブメッシュを超えて同一頂点を使用しているメッシュの正規化処理が高速化されました (#708)

### Fixed

- Decal系で対象となるレンダラーに マテリアルスロット数 が サブメッシュ数 よりも少ないレンダラーが存在する場合に例外が発生する問題を修正 (#709)

### Added

- NDMF v1.6.0 にて追加された AssetSaver API に対応 (#731)

## [v0.8.3](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.2...v0.8.3) - 2024-10-30

### Fixed

- AtlasTexture の内部で使用している RenderTexture で、 Depth&Stencil の初期化忘れにより破綻したテクスチャーが生成される問題を修正 (#700)
- AtlasTexture の lilToonShaderSupport が誤って lilToonLite を対応していると判定してしまい、対象範囲に存在した場合に例外が発生する問題を修正 (#701)
- ParticleSystem が何かしらで対象に含まれてしまうと、 NDMF Preview が動作しなくなる問題を修正 (#702)

## [v0.8.2](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.1...v0.8.2) - 2024-10-19

### Fixed

- narazaka/FloorAdjuster よりも後に TTT を実行してしまい、 Decalなどがずれてしまう問題を 先に動作させることで修正 (#693)

## [v0.8.1](https://github.com/ReinaS-64892/TexTransTool/compare/v0.8.0...v0.8.1) - 2024-10-09

### Fixed

- VRChatSDK が存在しない環境でコンパイルエラーが発生する問題を修正 (#674)

## [v0.8.0](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.7...v0.8.0) - 2024-09-30

### Added

- NDMF-Preview に対応 (#516)
- NDMF-Preview の場合が テクスチャースタックをマージした時 Texture2D へのコンバートを行わないようにし高速化 (#526)
- AtlasTexture の アイランド再配置結果の詳細を NDMFConsole にレポートする機能を追加 (#531)
- AtlasTexture TextureFineTuning ReferenceCopy の TargetPropertyName がリストに変更されコピー対象を複数指定可能になりました (#532)
- AtlasTexture の _MainTex 以外のプロパティで アトラス化対象だった場合、アトラス化後のサイズが自動的にそのプロパティのテクスチャの最大サイズが割り当てられるようになりました (#550)
- AtlasTexture で生成されるテクスチャーの MipMap 生成が Unity標準ではなく、アルファを加味したもを使用するようになりました (#558)
- AtlasTexture に BackGroundColor が追加され、初期設定で白色に背景色が設定されるようになります (#558)
- AtlasTexture TextureFineTuning MipMapRemove に MipMap を削除しないように上書きすることを可能にするための IsRemove プロパティを追加 (#561)
- AtlasTexture の選択候補の表示が、マテリアルのグループ化された状態で表示されるようになりました (#564)
- AtlasTexture の PixelNormalize 実験的機能から外れました！ (#585)
- AtlasTexture のアイランド再配置に失敗した場合にエラーログを出力するようになりました (#586)
- ~~AtlasTexture の NFDHPlasFC が外側のパディングを消去し再配置効率が上がりました (#588)~~
- NDMF v1.5.0 以上がプロジェクトに存在する場合 Preview、RealTimePreview のボタンは NDMF-Preview の フェーズ単位で有効化、無効化のボタンになるようになりました (#593)
- AtlasTexture が レンダラーのマテリアルに Null が含まれている場合でもアトラス化の対象にできるようになりました (#612)
- AtlasTexture TextureFineTuning Remove に 削除しない等に上書きすることを可能にするために IsRemove プロパティを追加 (#613)
- 部分的マイグレーションが可能なウィンドウ追加と、ツールバーとマイグレーション通知から開けるようになりました (#620)
- v0.9.0 にて廃止される予定のコンポーネントの入れ子状態に警告を発生させるようになりました (#629)
- 実行できない場合などの information などが以前よりも細かく出力されるようになりました (#630)
- SimpleDecal のスケールが反転しているときにヘルプボックスを表示するようになりました (#631)
- AtlasTexture の NFDHPlasFC が外側のパディングが半分になり、再配置効率が上がりました (#640 #648)
- NDMF の v1.3.0 以降で NDMF 要求バージョンを満たしていない場合に NDMF Console に警告を表示するようになりました (#643)
- NDMF の v1.3.0 以降で NDMF 要求バージョンを満たしていない場合に NDMF Console に表示される警告にデスクリプションを表示するようになりました (#644)

### Changed

- AtlasTexture の影響範囲が TargetRoot に影響されなくなり、インスペクターのマテリアル表示制限の機能のみに変更 (#516)
- ~~NDMF の対応バージョンが v1.3.0 以上に変更 (#516)~~
- 二のべき乗の値を想定する入力欄がポップアップに変更 (#516)
- TargetRoot は LimitCandidateMaterials に変更され、割り当てなくてもマテリアルの選択が行えるように変更 (#518)
- SerializeReference を使用している部分のUIが、[Unity-SerializeReferenceExtensions](https://github.com/mackysoft/Unity-SerializeReferenceExtensions) を使用したものに変更 (#519)
- AtlasTexture TextureFineTuning の PropertyNames でスペース区切りの複数指定が行える仕様は削除され、リストに変更されました (#532)
- AtlasTexture TextureFineTuning Compress のフォーマットクオリティで決定されるフォーマットがアルファの使用有無を加味したものになるようになりました (#558)
- AtlasTexture TextureFineTuning Compress を新しく生成した時の値を変更しました (#561)
- NDMF の対応バージョンが v1.5.0 以上に変更 (#593)

### Removed

- ReferenceResolver は削除されました (#517)

### Fixed

- lilToon の [Optional] 系を誤って 通常のlilToonの対応で認識してしまい、例外が発生する問題を修正 (#520)
- SubMesh よりも多くの MaterialSlot がある場合 AtlasTexture のメッシュノーマライズで、誤ったサブメッシュで複製される問題を修正 (#521)
- AtlasTexture の IslandFineTuning が Null な場合や IslandSelector が Null の場合に例外が発生する問題を修正 (#530)
- SimpleDecal でリアルタイムプレビュー中に IslandSelector を割り当てた時に IslandSelector の調整でプレビューが更新されない問題を修正 (#525)
- AtlasTexture の TextureFineTuning Resize が AtlasTextureSize よりも大きい解像度に変更できていた問題を修正 (#550)
- AtlasTexture でアトラス化されたテクスチャが他UVを参照していたり無効だった場合に割り当てないようになりました (#565)
- 一部のコンポーネントの子となり取り扱われるコンポーネントが NDMF-Preview で追加や削除が正しく反応しない問題を修正 (#569)
- プレイモードに入るときなどのビルド時にコンポーネントが新しく生成したが、最終的に使用されていないテクスチャが Null となりテクスチャ圧縮のタイミングで例外が発生する問題を修正 (#581)
- AtlasTexture の アイランド再配置 NFDHPlasFC が 上の列から見た空き空間の計算を誤った値にしてしまいアイランドが重なって見た目が変わってしまう問題を修正 (#591)
- lilToon の宝石シェーダーが誤って 通常の lilToon としてサポートされていた問題を修正 (#598)
- GameObject/TexTransTool から生成した GameObject がレコードされて終らず、元に戻すを行っても消えない問題を修正 (#602)
- AtlasTexture の lilToon _MainTexHSVG のベイクが正しく行われない問題を修正 (#611)
- SimpleDecal の適用対象のレンダラーのマテリアルに Null が含まれている場合に例外が発生する問題を修正 (#612)
- NDMF v1.4.1 などの NDMF 非対応バージョンがプロジェクトが存在する場合に警告が発生しない問題を修正 (#619)
- マイグレーションの時に、拡張子が `.Unity` となっているシーンが存在するとマイグレーションに必ず失敗する問題を修正 (#620)
- AtlasTexture lilToonSupport の TextureBake で MatCap が使用されていない場合 MatCapBlendMask が白色になってしまう問題を修正 (#634)
- AtlasTexture lilToonSupport の _Main3rdTex の使用可否判定が誤っていた問題を修正 (#634)
- UnsafeNativeArrayUtility の不要な using で Android ビルドにてコンパイルエラーが発生する問題を修正 (#641)
- 圧縮設定を None などの圧縮しない形式にした場合に誤って MipMap が Unity標準の物で再生成されることがあった問題を修正 (#649)
- AtlasTexture の TextureFineTuning Compress が ReferenceCopy などが行われた場合に誤った形式で圧縮される問題を修正 (#654)
- マイグレーション終了時のシーン復元処理で、すべてのシーンがロード状態で復元されてしまう問題を修正 (#657)

## [v0.7.7](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.6...v0.7.7) - 2024-07-24

### Fixed

- SingleGradationDecal のマテリアル選択候補の表示に Missing や Clone のマテリアルが表示される問題を修正 (#563)
- GameObject TexTransTool のメニューで生成される GameObject が常にワールド原点に生成されていた問題を修正 (#574)
- GameObject TexTransTool のメニューで生成される GameObject が自動で選択されない仕様を修正 (#574)

## [v0.7.6](https://github.com/ReinaS-64892/TexTransTool/compare/v0.7.5...v0.7.6) - 2024-07-14

### Fixed

- マイグレーション時のメッセージテキストが調整されました (#549)
- v0.6.x からのマイグレーションで、 AtlasTexture の TextureSizeOffset が正しく SizePriority にマイグレーションされるようになりました (#553)

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

- AtlasSet Null のマテリアルが表示される問題の修正 (#17)
- AtlasSet Mesh が Null のレンダラーが存在すると正常に実行できない問題の修正 (#20)

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
