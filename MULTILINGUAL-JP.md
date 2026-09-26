# 多言語辞書入力

他の言語：[中文](MULTILINGUAL.md) ｜ [English](MULTILINGUAL-EN.md) ｜ [한국어](MULTILINGUAL-KO.md)

中国語の全拼（フルピンイン）、簡拼、双拼、簡体字／繁体字辞書はそのまま利用できます。追加した言語はそれぞれ独立した「コード → 語句」辞書を使います。例：`nihongo` → `日本語`、`hangugeo` → `한국어`。同じ方法で他の言語も追加できます。プレハブ自体には語句も索引も含まれていないため、使いたい言語ごとに以下の 2 つの手順を手動で実行する必要があります。

これは辞書駆動の語句単位入力です。完全なコードと前方一致の候補に対応しますが、任意のローマ字をかなに変換する機能、日本語の文解析、韓国語の 2 式キーボード組み立てには対応していません。コードは取り込んだ辞書と一致している必要があります。辞書に定義されていれば、編集欄にかなやハングルを直接入力することもできます。

## Unity での設定

`HXIME_Pinyin.prefab` には語句も索引も一切含まれていません。プレハブ内の 4 つの辞書コンポーネント（`SimpDictPool`、`TradDictPool`、`JapaneseDictionary`、`KoreanDictionary`）はすべて空で、それぞれが接続先の辞書ソースファイルを隠しフィールドに記録しています：`SimpDictPool` → `Dicts/pinyin_simp.dict.yaml.txt`、`TradDictPool` → `Dicts/luna_pinyin.dict.yaml.txt`、`JapaneseDictionary` → `Dicts/japanese_mozc_common.dict.tsv.txt`、`KoreanDictionary` → `Dicts/korean_nikl_common.dict.tsv.txt`。この 4 つのコンポーネントとソースファイルは既にプレハブに存在するため、新規作成や再接続は不要です。

各言語は手動で、1 言語ずつ設定します。その言語のコンポーネントの `PinyinDict` インスペクターで 2 つの手順を順に実行します：**「加载并应用字典 / Load and apply dictionary（辞書を読み込んで適用）」** は接続されたソースファイルを解析し、語句と重みを独立したバイナリ辞書アセット `Assets/HXIME_DictionaryData/<コンポーネントのオブジェクト名>.asset`（例：`SimpDictPool.asset`）に書き込みます。この段階では索引を構築せず、プレハブもシーンも変更しません。**「重建查询索引 / Rebuild lookup index（索引を再構築）」** は同じアセットに整列済み索引を構築します。語句と索引はこのバイナリ アセットだけに存在し、アセットはユーザー自身のプロジェクトで生成されるため、パッケージには含まれません。

エディターのメニューも言語ごとに分かれています：`Tools → HXIME → Simplified Chinese → Load and Apply Dictionary` / `Rebuild Lookup Index`、`Tools → HXIME → Traditional Chinese → Load and Apply Dictionary` / `Rebuild Lookup Index`、`Tools → HXIME → Japanese → Load and Apply Dictionary` / `Rebuild Lookup Index` / `Enable Language Button`、`Tools → HXIME → Korean → Load and Apply Dictionary` / `Rebuild Lookup Index` / `Enable Language Button`、および `Tools → HXIME → Validate All Dictionaries`、`Tools → HXIME → Load and Apply All Dictionaries`、`Tools → HXIME → Rebuild All Lookup Indexes`。旧メニューの `Tools → HXIME → Rebuild All Dictionary Indexes` と `Tools → HXIME → Configure Japanese and Korean Dictionaries` は存在しません。

言語が未読み込み、または索引が未構築の場合、対応する `PinyinDict` インスペクターに英語の ERROR ヘルプボックスが表示されます（例：`ERROR: HXIME Simplified Chinese dictionary data is not loaded. Click "Load and apply dictionary" in the PinyinDict inspector (source: Dicts/pinyin_simp.dict.yaml.txt).`、`ERROR: HXIME Simplified Chinese lookup index is not built. Click "Rebuild lookup index" in the PinyinDict inspector.`）。Play モードへの進入はキャンセルされ（不足している言語がダイアログに一覧表示されます）、ワールドビルドは即座に失敗します。

日本語と韓国語は必要に応じて有効にできます：`HXIMEUI` 最上部の「是否启用日文?」「是否启用韩文?」（日本語を有効化 / 韓国語を有効化）スイッチが、その言語を言語循環に含めるかを決めます。オフにすると言語ボタンはその言語を飛ばし、対応する辞書も検索しません。中国語と英語は常に有効です。辞書コンポーネント自体は削除されないため、再度有効にするにはスイッチを入れ直すだけです（`Tools → HXIME → Japanese → Enable Language Button` / `Tools → HXIME → Korean → Enable Language Button` でも切り替えられます）。

簡体字・繁体字のコンポーネントは引き続き `PinyinEngine` の `Dicts` 配列の先頭 2 つに、日本語・韓国語のコンポーネントは `Additional Dicts` 配列に接続されており、手作業での配線変更は不要です。さらに：

1. 同梱の TMP フォントは静的 `NotoSansMultilingualFallback` に接続済みで、現代ハングルの全音節と既定の日韓辞書の文字を覆います。出力先の入力欄がプロジェクト外のフォントを使う場合は、そのフォントの Fallback Font Assets にこの fallback を追加してください。辞書を追加したら、新しい文字がフォントに含まれるか確認してください。
2. UdonSharp をコンパイルし、Unity Play Mode / VRChat ClientSim で確認してからワールドをビルドします。言語ボタンは「中国語 → JP → Ko → 英語 → 中国語」を循環し、空の参照は飛ばされます。

辞書のソースファイルはパッケージに含めて配布され、語句と索引は Unity エディターで生成してワールドと一緒に配布されます。VRChat 内でプレイヤーのローカルファイルを読むことはありません。そのため同梱されるのは事前適用済みの辞書データではなく辞書のソーステキスト（数十 MB）で、ある言語の設定を完了した後の実行時検索性能は従来と同じです。読み込みは Undo に対応し、形式エラー時は以前の辞書を保持します。

`HXIMEUI.Target Inputfield` は独立した出力欄を指定してください。`InputBarHandle/InputBar/InputField`（ピンイン編集欄）は指定できません。誤って指定すると、確定した文字がピンイン候補の再計算を引き起こします。現在の版はそのような指定での確定を拒否します。メニュー **Tools → HXIME → Repair Output Field and Verify Chinese Selection** を使うと、現在のシーンの誤指定を修復できます。現在のシーンをバックアップし、候補バーの下に独立した `OutputField` を作成して再バインドし、シーンを保存します。すでに正しい出力欄は置き換えません。このメニューは続けて `HXIME_Pinyin.prefab` をメモリ上で開き、中国語の選詞セルフテスト（`ce s`、`ces`、`ce shi`、`c s`、余分な空白、残ったコード）を実行し、出力欄が編集欄に誤って接続されている場合は拒否されることを確認します。プレハブは変更されず、Play モードにも入りません。

メインキーボードと上部の言語ラベルは同期して更新され、現在の言語だけを表示します：中国語 `中`、日本語 `JP`、韓国語 `Ko`、英語 `En`。クリア、確定、ページ送り、設定、入力ヒントも言語に合わせて切り替わります。英字キーは辞書のローマ字コードを入力します。同梱スキンの作者説明は原文のままです。

フォントの出典とライセンスは `Fonts/MULTILINGUAL-FONT.md` にあります。メニュー **Tools → HXIME → Bake Japanese and Korean Font Fallback** は不足する静的フォントを生成して fallback を接続します。**Validate Multilingual Labels and Glyphs** は Play モードに入らずに、ラベル、TMP メッシュの字形、韓国語 `we` の候補を検証します。フォントは同梱済みのため、通常の取り込みで再ベイクは不要です。これらのツールはすべて編集モードで動作し、Play モードには入らず、結果はプロジェクトのエディター一時ディレクトリ（`Temp/`）に書き出されます。手動の場合は上記のメニューを使用してください。

## ファイル形式

UTF-8 テキストで、1 行につき実際のタブで区切ります：`語句<Tab>コード<Tab>重み`。重みは省略可能で既定は 0 です。非負整数・パーセント（`99.93%`）・小数（`1.5`）に対応し、後の 2 つは 1/100 精度で整数に拡大します（`99.93%` → `9993`、`1.5` → `150`）。これにより元の並び順を保持します。コードは大文字小文字を区別せず、前後の空白を除去し、内部の空白は保持します。空行、`#` で始まる行コメント、UTF-8 BOM、Windows の改行に対応します。

標準の RIME `---` / `...` ヘッダーに対応しますが、データ列の順序は text、code、weight でなければなりません。独自の `columns`、`import_tables` を使う辞書は、先に展開して上記の TSV に変換してください。他の入力方式のエンコード体系の自動変換や、外部サブ辞書の読み込みは行いません。

拡張言語の候補は「完全一致コードを優先し、次に重みの降順」で並び、同じ出力は重複排除されます。上限は既定で 50 件で、Inspector から設定できます。1 ページは 5 件です。前方一致の候補は語句全体を補完するもので、選択すると編集文字列全体を消去し、出力文字数でコードを切り詰めることはしません。複数語の語句も独立した項目として取り込んでください。

`HXIMEUI` の Inspector の `Candidate Count`（`candidateLimits`）で候補の総数上限を設定できます。既定値は 50、最小値は 1 です。1 ページは引き続き 5 件で、実際の件数は一致結果によります。100 を超えると Inspector に英語の性能警告が表示されますが、設定値は制限されません。上限を増やすと検索・重複排除・並べ替えの負荷が増える場合があります。特に短い入力や大きな辞書では、VRChat クライアントで入力遅延を確認してください。

すべての辞書は `Assets/HXIME_DictionaryData/` 内の独立したバイナリ `.asset`（例：`SimpDictPool.asset`）に保存され、そのアセットに語句と構築済みの索引の両方が入ります。プレハブとシーンは辞書データも索引配列も受け取らず、プレハブは辞書ソースファイルを接続するだけです。アセットはユーザー自身のプロジェクトで生成され、パッケージには含まれません。Play モード進入時とワールドビルド時に、エディターがそのアセットの語句と索引を Udon に書き込みます。読み込みまたは索引が不足している場合は Play モード進入がキャンセルされ、ワールドビルドは失敗します。大きな配列のプレハブオーバーライドは避けられ、ある言語の読み込みと索引構築を終えた後の実行時検索性能は従来と同じです。

スペースで現在ページの先頭候補、数字 1–5 で現在ページの候補を選択します。候補がないときはスペースでコードをそのまま確定、Enter でもコードを確定、Tab で編集文字列を消去、バックスペースでコードを 1 文字削除します。言語を切り替えると、未選択のコードは失わないように先に確定されます。中国語専用の簡繁・双拼ボタンは中国語モードでのみ表示されます。

## 確認チェックリスト

- 言語は「辞書を読み込んで適用」と「索引を再構築」の両方を実行すると使えるようになる。どちらか欠けると Play モード進入がキャンセルされ、ワールドビルドも失敗する。中国語の読み込みと索引構築が完了すれば、中国語・英語・簡繁・双拼は引き続き使える。
- 日本語で `NIHONGO` が `日本語`、`neko` が `猫` と `ねこ` になる。韓国語で `hangugeo` が `한국어` になる。
- 前方一致、一致なし、空文字までの削除、候補のページ送り、言語切り替えの後に、古い候補や空の候補が確定されない。
- 順序が乱れた語句、重複コード、空ファイル、負の重み、RIME の終端記号なしを取り込み、候補とエラーメッセージを確認する。
- 目標プラットフォームでフォント表示と大きな辞書の入力遅延を確認する。候補はソート済み索引の二分探索で一致範囲を特定し、1 件ずつの全件走査は行いません。短い入力では依然として広い範囲に一致することがあるため、非常に大きな辞書のフレーム時間は実クライアントで再確認が必要です。索引は常駐メモリの増加と引き換えに検索を高速化します。詳細は[候補検索の最適化](PERFORMANCE-JP.md)。

## 関連ドキュメント

- 辞書の出典・変換規則・ライセンス：[Dicts/SOURCES-JP.md](Dicts/SOURCES-JP.md)
- フォントの出典・ベイク設定・ライセンス：[Fonts/MULTILINGUAL-FONT.md](Fonts/MULTILINGUAL-FONT.md)
- 新しい入力欄で文字が□になる場合：[TMP字体方框解决方案.md](TMP字体方框解决方案.md)
- 索引と候補検索の実装説明：[PERFORMANCE-JP.md](PERFORMANCE-JP.md) ｜ [中文](PERFORMANCE.md) ｜ [English](PERFORMANCE-EN.md) ｜ [한국어](PERFORMANCE-KO.md)
- インストールとよくある質問：[README.md](README.md) ｜ [English](README-EN.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)
