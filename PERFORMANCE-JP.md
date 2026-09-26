# 中日韓 候補検索の最適化について

他の言語：[中文](PERFORMANCE.md) ｜ [English](PERFORMANCE-EN.md) ｜ [한국어](PERFORMANCE-KO.md)

## 最適化の目標

キー入力のたびに発生していた辞書全体の走査と一時確保を減らし、入力の引っかかりを軽減します。候補の上限は既定で 50 件で、Inspector から設定できます。1 ページは 5 件です。

`HXIMEUI` の Inspector の `Candidate Count`（`candidateLimits`）で候補の総数上限を設定できます。既定値は 50、最小値は 1 です。1 ページは引き続き 5 件で、実際の件数は一致結果によります。100 を超えると Inspector に英語の性能警告が表示されますが、設定値は制限されません。上限を増やすと検索・重複排除・並べ替えの負荷が増える場合があります。特に短い入力や大きな辞書では、VRChat クライアントで入力遅延を確認してください。

## 主な変更

- **検索経路を索引に統一**：中国語・日本語・韓国語が同一のソート済み索引を共有し、二分探索で完全一致および前方一致の範囲を特定します。
- **言語ごとの規則は維持**：中国語は二重ピンイン変換・逆方向前方一致・簡拼を残し、日本語と韓国語はローマ字の前方一致と出力の重複排除を残します。
- **確保を削減**：中国語は Top-K をストリーム処理し、辞書サイズの候補配列を廃止しました。検索用ワークスペースは再利用し、簡拼はエディターで事前計算します。
- **再計算を回避**：直近の検索を辞書・入力・候補上限・一致モードをキーにキャッシュし、一致処理のホットパスからログ出力を削除しました。
- **索引を事前に構築**：`PinyinDict` インスペクターまたは `Tools → HXIME` メニューで「索引を再構築」を実行すると、エディターが整列済み索引を生成して独立バイナリ辞書アセットへ書き込みます。Play モード進入時とワールドビルド時に、エディターがそのアセットの語句と索引を Udon データへ書き込みます。

## 使い方

「辞書を読み込んで適用」と「索引を再構築」を実行すると最適化が有効になります。`PinyinDict` インスペクターの **重建查询索引 / Rebuild lookup index** を押すか、`Tools → HXIME → Simplified Chinese / Traditional Chinese / Japanese / Korean → Rebuild Lookup Index`、`Tools → HXIME → Rebuild All Lookup Indexes` を使用します（旧 `Tools → HXIME → Rebuild All Dictionary Indexes` メニューは存在しません）。再構築は独立バイナリ辞書アセット（`Assets/HXIME_DictionaryData/`）だけを書き換え、プレハブもシーンも変更しないため、エディターが固まることはありません。個別の辞書を `PinyinDict` インスペクターで再構築した結果はメニューと同じですが、そのコンポーネントだけを処理します。

カスタムスクリプトが実行時に重みを変更する場合は `PinyinEngine.InvalidateMatchCache()` を呼び出します。コードや語句の構成はエディターで変更し、その後に「辞書を読み込んで適用」と「索引を再構築」を再度実行してください。データを読み込んでいない、または索引を構築していない言語は Play モード進入をキャンセルし、ワールドビルドを失敗させます。対応する `PinyinDict` インスペクターには英語の ERROR ヘルプボックスが表示されます。

## 検証と制限

- Unity 2022.3.22f1 / UdonSharp でコンパイルが通ることを確認。
- 開発段階で 1,197 件の候補チェックとキャッシュチェックに合格し、簡体・繁体中国語、日本語、韓国語、同点順位、重複語句、二重ピンインを網羅。
- 4 つの辞書の索引が Udon の実際に使用するデータへ書き込めることを検証。
- 以上はコンパイルと正しさの検証であり、VRChat クライアントでのフレーム時間の改善は未計測です。短い入力では依然として広い範囲に一致することがあります。索引は常駐メモリの増加と引き換えに検索を高速化します。

開発段階の一時的なテストスクリプトは検証完了後に削除しました。同梱の `Tools → HXIME → Rebuild All Lookup Indexes`、各言語の `Rebuild Lookup Index` と `Validate Multilingual Labels and Glyphs` メニューは影響を受けず、そのまま利用できます。

## 関連ドキュメント

- 多言語辞書の設定とファイル形式：[MULTILINGUAL-JP.md](MULTILINGUAL-JP.md) ｜ [中文](MULTILINGUAL.md) ｜ [English](MULTILINGUAL-EN.md) ｜ [한국어](MULTILINGUAL-KO.md)
- 辞書の出典・変換規則・ライセンス：[Dicts/SOURCES-JP.md](Dicts/SOURCES-JP.md)
- インストールとよくある質問：[README-JP.md](README-JP.md) ｜ [中文](README.md) ｜ [English](README-EN.md) ｜ [한국어](README-KO.md)
