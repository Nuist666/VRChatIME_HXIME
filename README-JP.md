### HXIME v0.9.6 VRChatワールド用 多言語入力キーボード

他の言語：[中文](README.md) ｜ [English](README-EN.md) ｜ [한국어](README-KO.md)

VRChat ワールド向けの入力キーボードです。もともとは中国語ピンイン入力用でしたが、現在は**設定可能な多言語入力**に拡張され、ローマ字コードを入力すると日本語や韓国語などの語句を直接出力できます。

- **中国語**：全拼（フルピンイン）、簡拼、混拼、簡体字／繁体字辞書の切り替え
- **日本語**：ローマ字コード入力。例：`nihongo` → `日本語`、`neko` → `猫`／`ねこ`
- **韓国語**：ローマ字コード入力。例：`hangugeo` → `한국어`、`annyeonghaseyo` → `안녕하세요`
- **英語**：そのまま入力。辞書入力とはいつでも切り替え可能
- 日本語と韓国語は `HXIMEUI` 最上部で個別に有効/無効を切り替えられます。中国語と英語は常に有効です
- 辞書は Unity エディターで取り込み、ワールドと一緒に配布されます。プレイヤーのローカルファイルは読み込みません
- 導入は簡単、スキンのカスタマイズにも対応。言語ボタンは「中 → JP → Ko → En」を循環します

`Dicts` にはそのまま取り込める Google Mozc の日本語辞書と、韓国国立国語院の韓国語基礎辞書を用意しています（コンパクト版と完全変換版）。ファイルの選び方、出典、ライセンスは[辞書について](Dicts/SOURCES-JP.md)を参照してください。

現在の `HXIME_Pinyin.prefab` には語句も索引も入っておらず、4 つの辞書コンポーネントはそれぞれ辞書ソースファイルを接続しているだけです。使用前に[多言語辞書入力](MULTILINGUAL-JP.md)に従い、使いたい言語ごとに「辞書を読み込んで適用」と「索引を再構築」を順に実行してください。どちらか欠けるとコンポーネントに英語の ERROR が表示され、Play モード進入はキャンセルされ、ワールドビルドも失敗します。詳しい設定手順、辞書のファイル形式、機能範囲は[多言語辞書入力](MULTILINGUAL-JP.md)にまとめています（[中文](MULTILINGUAL.md) ｜ [English](MULTILINGUAL-EN.md) ｜ [한국어](MULTILINGUAL-KO.md)）。

中国語・日本語・韓国語は同一のソート済み索引と Top-K の候補検索を共有し、中国語は逆方向前方一致と簡拼も保持します。索引はエディターで「索引を再構築」を実行したときに生成され、`Assets/HXIME_DictionaryData/` 内の独立バイナリ辞書アセットへ書き込まれます。Play モード進入時とワールドビルド時に、エディターがそのアセットの語句と索引を Udon へ書き込むため、キー入力時は関連する範囲だけを検索します。古いシーンや手動で変更した辞書は `Tools → HXIME → Rebuild All Lookup Indexes` または言語ごとの `Rebuild Lookup Index` で再構築してください（旧 `Tools → HXIME → Rebuild All Dictionary Indexes` メニューは存在しません）。カスタムスクリプトが実行時に重みを変更する場合はエンジンの `InvalidateMatchCache()` を呼び出します。コードや語句の構成はエディターで変更し、その後に読み込みと索引の再構築を行ってください。経緯・索引構造・検証範囲は[候補検索の最適化](PERFORMANCE-JP.md)にまとめています（[中文](PERFORMANCE.md) ｜ [English](PERFORMANCE-EN.md) ｜ [한국어](PERFORMANCE-KO.md)）。

`HXIMEUI` の Inspector の `Candidate Count`（`candidateLimits`）で候補の総数上限を設定できます。既定値は 50、最小値は 1 です。1 ページは引き続き 5 件で、実際の件数は一致結果によります。100 を超えると Inspector に英語の性能警告が表示されますが、設定値は制限されません。上限を増やすと検索・重複排除・並べ替えの負荷が増える場合があります。特に短い入力や大きな辞書では、VRChat クライアントで入力遅延を確認してください。

作者はまだ git が使えないため、PR を送ってくださる皆さんに大変感謝しています。

「漢字が次元を超え、文明が栄えるように」

Project Link: https://github.com/xianglong90II/VRChatChineseIME_HXIME

# インストール

- `HXIME_Pinyin` プレハブをワールドにドラッグ＆ドロップします。
- `SwitchBarHandle`、`InputBarHandle`、`KeyboardHandle` を好きな位置に調整できます。
- 既定では最初のスキンが使われます。スキンの順序を変えると既定のスキンを差し替えられます。
- （スキンの説明と画像は 1 対 1 で対応させる必要があります。不要なスキンは削除して構いません）
- `HXIME_Pinyin` の `HXIMEUI` で、Target Inputfield に出力先の入力欄を指定します。**独立した出力欄**を指定し、入力法自身のピンイン編集欄は指定しないでください。
- 使用前に[多言語辞書入力](MULTILINGUAL-JP.md)に従い、使いたい言語ごとに「辞書を読み込んで適用」と「索引を再構築」を実行してください。プレハブ自体には語句も索引も含まれていません。
- 完了です！

# Q&A

- Q: 入力キーボードをワールドに固定し、プレイヤーに掴ませたくない場合は？
- A: `SwitchBarHandle`、`InputBarHandle`、`KeyboardHandle` の VRC PickUp を削除します。
- Q: キーボード切り替えやハンドル切り替えなどのボタンに触れさせたくない場合は？
- A: `SwitchBar` または `SettingsPanel` の `Buttons` から隠したいボタンを選び、インスペクター左上のチェックを外します。
- Q: どの言語に対応していますか。日本語のかなや韓国語を入力できますか？
- A: 言語は辞書で決まります。中国語・日本語・韓国語を同梱し、英語はそのまま入力できます。日本語と韓国語は**語句単位のコード入力**です。ローマ字コードを入力して候補を選びます（例：`nihongo` → `日本語`）。任意のローマ字をかなに変換する機能、日本語の文解析、韓国語の 2 式キーボード組み立てには対応していません。取り込んだ辞書に定義があれば、編集欄にかなやハングルを直接入力することもできます。辞書を追加すれば他の言語も入力できます。
- Q: 文字が豆腐（□）になる場合は？
- A: [新しい TMP 入力欄で文字が□になる場合の対処](TMP字体方框解决方案.md) と `Fonts/MULTILINGUAL-FONT.md` を参照してください。

# 上級者向け

- Q: 独自のスキンを作成できますか?
- A: もちろんです！スキンの説明を書き、背景画像を用意するだけです。説明の形式は「スキン名;説明;ボタンの色 rgba;メインカラー1 rgba;メインカラー2 rgba」です。
- 例：
- HXIME白;作者: HX2 xianglong90;(255,255,255,128);(52,161,255,255);(52,255,209,255)
- 正確に描きたい場合は `Themes` フォルダーの `ThemePSDInstruction.psd` と png ファイルを参照してください。
- 背景には UISprite ではなく Texture2D を使用しています。
- 凝るなら、オンライン読み込みや RenderTexture（ゲーム内カメラでの撮影）も試せます。
- Q: スキンは何セットまで置けますか?
- A: 理論上はほぼ無制限です。ただしプレイヤーが選べるスロットは最初の 9 つだけです。10 セット目以降は `HXIMEUI` が付いたオブジェクトから公開メソッド `SetSkin(index)` を呼び出して設定します。
- Q: 自分の辞書を取り込みたい場合は？
- A: 中国語辞書は `PinyinEngine` の簡体字・繁体字スロット、その他の言語は `Additional Dicts` 配列にあります。配列の各要素は `PinyinDict` コンポーネントを持つ子オブジェクトで、「言語ボタン名」がボタンの表示文字になります。
- その言語の `PinyinDict` インスペクターで「辞書を読み込んで適用」を押して UTF-8 テキスト辞書を適用し、続けて「索引を再構築」を押します。語句と索引は `Assets/HXIME_DictionaryData/` 内の独立バイナリ アセットにだけ書き込まれ、プレハブもシーンも変更されません。形式は「語句 tab コード tab 重み（省略可、既定 0）」です。拡張言語のコードはローマ字で、大文字小文字は区別しません。
- お気づきのとおり、これは RIME 辞書の形式です。`---` / `...` ヘッダー付きの標準 RIME 辞書もそのまま取り込めます。独自の `columns` / `import_tables` を使う辞書は、先に上記 3 列へ展開してください。詳細は[多言語辞書入力](MULTILINGUAL-JP.md)。
- Q: 拡張言語のキー操作は？
- A: スペースで現在ページの先頭候補、数字 1–5 でページ内の候補を選択。候補がないときはスペースまたは Enter でコードをそのまま確定、Tab で編集文字列を消去、バックスペースでコードを 1 文字削除します。言語を切り替えると、未選択のコードは先に確定されます。

# Licence: LGPL v3
https://github.com/xianglong90II/VRChatChineseIME_HXIME

# Credit
- アイコン Icons: Google Material UI & Fonts https://fonts.google.com/
- フォント Font: Noto Sans / Noto Sans CJK https://fonts.google.com/noto
- 簡体字辞書: RIME pinyin-simp https://github.com/rime/rime-pinyin-simp
- 繁体字辞書: RIME luna-pinyin https://github.com/rime/rime-luna-pinyin
- 日本語辞書: Google Mozc https://github.com/google/mozc （BSD 3-Clause、辞書には別途の注意書きあり）
- 韓国語辞書: 韓国国立国語院 韓国語基礎辞典 https://krdict.korean.go.kr/ （CC BY-SA 2.0 KR）
- 出典・変換規則・ライセンス全文: [Dicts/SOURCES-JP.md](Dicts/SOURCES-JP.md)
