# Japanese and Korean Dictionary Files

Other languages: [中文](SOURCES.md) ｜ [日本語](SOURCES-JP.md) ｜ [한국어](SOURCES-KO.md)

The dictionaries added in this folder, like the pre-existing Chinese dictionaries, are UTF-8 `.txt` text in the form `word, input code, weight` separated by real tabs, and are applied with `PinyinDict`'s "Load and apply dictionary", followed by "Rebuild lookup index"; the `JapaneseDictionary` and `KoreanDictionary` components in the prefab are already mounted to `japanese_mozc_common.dict.tsv.txt` and `korean_nikl_common.dict.tsv.txt` from the table below. The weight may be omitted (defaults to 0); non-negative integers, the percentages common in RIME (such as `99.93%` in `luna_pinyin`) and decimals are accepted, and percentages and decimals are scaled by 1/100 into integers on import.

| File | Purpose |
| --- | --- |
| `japanese_mozc_common.dict.tsv.txt` | Smaller Japanese version: the top 30,000 entry/code pairs by conversion weight. Test this one first |
| `japanese_mozc.dict.tsv.txt` | Full Japanese conversion result, covering more word forms and proper nouns. **Warning: too many entries; not recommended for direct use.** |
| `korean_nikl_common.dict.tsv.txt` | Korean graded entries: the top 10,000 entry/code pairs by weight. Test this one first |
| `korean_nikl.dict.tsv.txt` | Full conversion result of the Korean basic dictionary, including ungraded entries. **Warning: too many entries; not recommended for direct use.** |

For every dictionary, "Load and apply dictionary" parses the mounted source file and writes the entries and weights into the standalone dictionary asset, but it does not build the lookup index; "Rebuild lookup index" then writes the sorted index into that same asset. Both steps can also be run from the menus: `Tools → HXIME → Simplified Chinese / Traditional Chinese / Japanese / Korean → Load and Apply Dictionary` and the matching `Rebuild Lookup Index`, or `Tools → HXIME → Load and Apply All Dictionaries` and `Tools → HXIME → Rebuild All Lookup Indexes`. The parsing stage shows the number of processed lines; the Console logs stage timings under `[HXIME Dictionary Import]`. These hints do not make loading a full dictionary any faster.

Every dictionary is stored as a standalone binary `.asset` under `Assets/HXIME_DictionaryData/` (for example `SimpDictPool.asset`): "Load and apply dictionary" writes the entries first, then "Rebuild lookup index" writes the sorted index into the same asset. Neither the prefab nor the scene receives dictionary data or index arrays; the prefab only mounts the dictionary source file. The asset is created in your own project and is not part of the package, which ships the source text files in this folder instead. Re-loading creates a new asset, so other users of the old asset are unaffected. On entering Play mode or building the world, the editor bakes the entries and indexes from the asset into Udon; a missing load or index cancels Play mode and fails the world build. This avoids large prefab overrides, and once a language has been loaded and indexed its runtime lookup performance is unchanged.

"Full" means the complete result after the filtering rules below, not every part of the original data. See `dictionary-manifest.json` for the exact row counts, byte sizes, SHA-256 values, upstream commit IDs and the address of every source file. The smaller versions are not manually reviewed high-frequency word lists; the current engine queries a sorted index, and latency still has to be verified on the target VRChat platform.

## Japanese sources and processing

- Source: Google Mozc, `src/data/dictionary_oss/dictionary00.txt` through `dictionary09.txt`.
- Pinned revision: [`13c98988247aa711d99db9e348ec2a597d14b5cd`](https://github.com/google/mozc/tree/13c98988247aa711d99db9e348ec2a597d14b5cd/src/data/dictionary_oss).
- The reading, the output entry and the word cost are extracted from the original rows; codes are generated from the **original kana reading** with the Hepburn output of pykakasi 2.3.0, and are never guessed from the kanji.
- Only readings made of full hiragana, katakana and the long vowel mark are processed; readings containing digits or symbols, or that cannot be converted into Latin codes, are filtered out. `'` and `-` are accepted inside codes.
- The weight is `max(1, 40000 - original cost)`; the lower the original cost, the higher the conversion weight. It is a ranking approximation, not a word frequency, and does not include Mozc's part-of-speech connection costs or sentence analysis.
- Entries are de-duplicated by "output entry, code", keeping the highest weight; the project's existing Japanese samples are added with weight 50,000. The 30,000 highest-weight pairs are then selected as the smaller version.
- Codes keep the converter's rules for long vowels, the moraic nasal and so on, and are not guaranteed to cover every input method alias, for example `nihongo` → `日本語`, `toukyou` → `東京`. The automatically converted result was not proofread entry by entry.

Copyright belongs to Google LLC, NAIST and the other rights holders noted upstream. The upstream notices are included:

- [Mozc LICENSE](Licenses/mozc-LICENSE.txt): BSD 3-Clause.
- [Dictionary-specific notice](Licenses/mozc-dictionary-NOTICE.txt): includes the IPAdic / NAIST / ICOT terms and the Okinawa data notice. The dictionaries cannot be covered by the Mozc code license alone; keep these notices when redistributing.

## Korean sources and processing

- Original author: National Institute of Korean Language (국립국어원). Dataset: [한국어기초사전 / Korean Basic Dictionary](https://krdict.korean.go.kr/).
- Download source: the community-maintained [`spellcheck-ko/korean-dict-nikl`](https://github.com/spellcheck-ko/korean-dict-nikl/tree/42c0d01889f34536e9cf94fe57f62bd2055b1bde/krdict), pinned commit `42c0d01889f34536e9cf94fe57f62bd2055b1bde`, `krdict/001.xml` through `011.xml`. That mirror is not maintained by the National Institute of Korean Language.
- Only the Korean headword, the textual pronunciation and the vocabulary grade are extracted; example sentences, definitions, audio and other media from the original are not redistributed.
- Control characters that XML 1.0 does not accept are removed from the original XML before parsing and counted in the manifest; the downloaded files themselves are not modified.
- Affixes that start or end with a hyphen, single letters and non-pure-Korean headwords are excluded; hyphen separators inside a headword are removed, and `^` becomes an output space.
- Romanized codes are generated from the headword with korean-romanizer 0.28.0; when a usable textual pronunciation exists, an additional pronunciation code alias is generated. Codes drop spaces and are lower-cased, while output entries keep spaces between words. The automatically converted result was not proofread entry by entry and does not claim to support every romanization habit.
- Weights: beginner 300, intermediate 200, advanced 100, ungraded 10. This is a learning-level ranking, not a real word frequency. The smaller version takes the top 10,000 pairs by weight from the graded entries and samples; equal weights are sorted by code and then by entry so the result is reproducible. Duplicate "entry, code" pairs keep the highest weight.
- Both versions add the project's existing Korean samples (weight 1,000), including common inputs such as `hangugeo` → `한국어` and `annyeonghaseyo` → `안녕하세요`.

The upstream and project-converted Korean dictionaries are released under **CC BY-SA 2.0 KR**. The modifications made by this project are selecting headwords, adding romanized codes, generating ranking weights, de-duplicating and adding samples. Please keep the author, the data source, this modification notice and the [license link](https://creativecommons.org/licenses/by-sa/2.0/kr/), and keep derivative dictionaries under the same license.

The [original mirror README](Licenses/nikl-README.md) and the [National Institute of Korean Language copyright policy](https://krdict.korean.go.kr/kor/kboardPolicy/copyRightTermsInfo) are included. The licenses of these dictionaries are independent of the project's code license.

## Provenance and verification

This folder distributes only the conversion results and the licenses with the package. The build script lives in `tools/build_dictionaries.py`: it downloads the pinned source revisions from the addresses recorded in `dictionary-manifest.json`, verifies their byte sizes and SHA-256 values, regenerates the four dictionaries with the rules above and compares them against the SHA-256 values in the manifest, so the conversion is reproducible inside the repository. The source data is still not redistributed and must be obtained from the links above. Run `python Dicts/tools/build_dictionaries.py` to rebuild and verify; `--check` regenerates all dictionaries from the pinned upstream data, byte-compares them against the SHA-256 values in `dictionary-manifest.json` and leaves this folder untouched. The conversion libraries are pinned to `pykakasi==2.3.0` and `korean-romanizer==0.28.0`; downloaded upstream inputs are cached to speed up repeated runs — the cache location is the `DEFAULT_CACHE` setting listed below and can be deleted at any time.

### Settings in the build script

The weight and size settings live at the top of `build_dictionaries.py`; changing them changes the size and the ranking bias of the dictionaries:

| Constant | Current value | Effect |
| --- | --- | --- |
| `JAPANESE_COST_BASE` | `40000` | Japanese weight = this value − Mozc cost, so a lower cost ranks higher; raising it lifts every Japanese weight |
| `JAPANESE_SAMPLE_WEIGHT` | `50000` | Weight given to every project Japanese sample, overriding the 100/80/50 written in `japanese_sample.tsv.txt` |
| `JAPANESE_COMPACT_SIZE` | `30000` | Rows kept in `japanese_mozc_common`: weight descending, ties by code then word, take the top N |
| `JAPANESE_ALLOWED` | hiragana `U+3041–U+3096`, katakana `U+30A1–U+30FA`, `ー` `U+30FC` | Characters allowed in a reading; anything else is filtered as a non-kana reading |
| `JAPANESE_CODE` | `^[a-z'-]+$` | Characters allowed in a romanised code; readings that do not match are dropped |
| `KOREAN_LEVEL_WEIGHT` | 초급 `300`, 중급 `200`, 고급 `100`, ungraded (`없음` or missing) `10` | Weight of each vocabulary grade |
| `KOREAN_SAMPLE_WEIGHT` | `1000` | Weight given to every project Korean sample, overriding the 100 written in `korean_sample.tsv.txt` |
| `KOREAN_GRADED_MIN_WEIGHT` | `100` | Lower bound for `korean_nikl_common`: only graded pairs at or above this weight compete for the top N |
| `KOREAN_COMPACT_SIZE` | `10000` | Rows kept in `korean_nikl_common` |
| `KOREAN_WORD` | `^[가-힣]+( [가-힣]+)*$` | Headwords must be Hangul syllables separated by single spaces; hyphen affixes, standalone jamo, non-Korean headwords and double spaces are filtered |

Other settings: `DEFAULT_CACHE` (input cache directory, `Temp/dict-cache` by default), `JAPANESE_SAMPLE` / `KOREAN_SAMPLE` (sample file paths) and `HEADERS` (the three `#` comment lines of each output, which are part of its SHA-256). Changing any constant changes the output, so the script has to be re-run and the `outputs` and `statistics` sections of `dictionary-manifest.json` updated.

The released files can be checked against `dictionary-manifest.json`: `generator` records the build script (`Dicts/tools/build_dictionaries.py`), `outputs` gives the entry count and SHA-256 of each dictionary, `bytes` gives the byte size, `sources` and `source_revisions` record the upstream file addresses and commit IDs, `conversion_packages` records the conversion library versions used at the time, and `statistics` records the filtering statistics. Hashes are computed with SHA-256 over the whole file, including `#` comment lines.

Only a few project samples were used as supplements during conversion, and their checksums are recorded in `local_supplements`. The license and attribution requirements do not depend on how the conversion is run or where the script lives, and still apply as described above.

Import and setup steps: see [MULTILINGUAL-EN.md](../MULTILINGUAL-EN.md). Pick one version per language; there is no need to load both the full and the smaller version.
