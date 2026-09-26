# Multilingual Dictionary Input

Other languages: [中文](MULTILINGUAL.md) ｜ [日本語](MULTILINGUAL-JP.md) ｜ [한국어](MULTILINGUAL-KO.md)

Chinese full pinyin, initial-letter spelling, double pinyin and the simplified/traditional dictionaries are unchanged. Every additional language uses its own "code to word" dictionary, for example `nihongo` → `日本語` or `hangugeo` → `한국어`. More languages can be added the same way. The prefab itself no longer contains entries or an index, so each language you want has to be set up manually with the two steps below before it can be used.

This is dictionary-driven whole-word input: complete codes and prefix candidates are supported, but there is no arbitrary romaji-to-kana conversion, no Japanese sentence analysis and no Korean two-set keyboard composition. Codes must match the imported dictionary; you can also type kana or Hangul codes defined in the dictionary directly into the composition field.

## Setting up in Unity

`HXIME_Pinyin.prefab` no longer contains any dictionary entries and no lookup index at all: its four dictionary components (`SimpDictPool`, `TradDictPool`, `JapaneseDictionary`, `KoreanDictionary`) are empty, and each one uses a hidden field to name the dictionary source file it is mounted to: `SimpDictPool` → `Dicts/pinyin_simp.dict.yaml.txt`, `TradDictPool` → `Dicts/luna_pinyin.dict.yaml.txt`, `JapaneseDictionary` → `Dicts/japanese_mozc_common.dict.tsv.txt`, `KoreanDictionary` → `Dicts/korean_nikl_common.dict.tsv.txt`. Those four components and their sources already exist in the prefab, so no new component or rewiring is needed.

Every language must be set up manually, one language at a time, with two steps in the `PinyinDict` inspector of that language's component: **"Load and apply dictionary / 加载并应用字典"** parses the mounted source file and writes the entries and weights into a standalone binary dictionary asset `Assets/HXIME_DictionaryData/<component object name>.asset` (for example `SimpDictPool.asset`); this step does not build the lookup index and changes neither the prefab nor the scene. **"Rebuild lookup index / 重建查询索引"** builds the sorted lookup index into that same asset. The data and the index live only in that binary asset; the asset is created in your own project and is not part of the package.

The editor menus are also organised per language: `Tools → HXIME → Simplified Chinese → Load and Apply Dictionary` / `Rebuild Lookup Index`, `Tools → HXIME → Traditional Chinese → Load and Apply Dictionary` / `Rebuild Lookup Index`, `Tools → HXIME → Japanese → Load and Apply Dictionary` / `Rebuild Lookup Index` / `Enable Language Button`, `Tools → HXIME → Korean → Load and Apply Dictionary` / `Rebuild Lookup Index` / `Enable Language Button`, plus `Tools → HXIME → Validate All Dictionaries`, `Tools → HXIME → Load and Apply All Dictionaries` and `Tools → HXIME → Rebuild All Lookup Indexes`. The old menu items `Tools → HXIME → Rebuild All Dictionary Indexes` and `Tools → HXIME → Configure Japanese and Korean Dictionaries` no longer exist.

If a language has not been loaded, or its index has not been built, the corresponding `PinyinDict` inspector shows an English ERROR HelpBox (for example `ERROR: HXIME Simplified Chinese dictionary data is not loaded. Click "Load and apply dictionary" in the PinyinDict inspector (source: Dicts/pinyin_simp.dict.yaml.txt).` and `ERROR: HXIME Simplified Chinese lookup index is not built. Click "Rebuild lookup index" in the PinyinDict inspector.`), entering Play mode is cancelled (a dialog lists the missing languages), and building the world fails immediately.

Japanese and Korean are optional: the "Enable Japanese" / "Enable Korean" switches (是否启用日文? / 是否启用韩文?) at the top of `HXIMEUI` decide whether that language takes part in the language cycle; when switched off the language button skips it and its dictionary is never queried. Chinese and English are always enabled. The dictionary components themselves are not removed, so re-enabling only needs the switch turned back on, or the `Tools → HXIME → Japanese → Enable Language Button` / `Tools → HXIME → Korean → Enable Language Button` menus.

The simplified and traditional components are still connected to the first two entries of `PinyinEngine`'s `Dicts` array, and the Japanese and Korean components are still in the `Additional Dicts` array; none of that wiring needs to be changed by hand. In addition:

1. The bundled TMP fonts are connected to the static `NotoSansMultilingualFallback`, which covers every modern Hangul syllable plus the characters used by the default Japanese and Korean dictionaries. If your target input field uses a font outside this project, add that fallback to the target font's Fallback Font Assets. After loading extra dictionaries, check that the new characters are covered.
2. Compile UdonSharp, verify in Unity Play Mode / VRChat ClientSim, then build the world. The language button cycles Chinese → JP → Ko → English → Chinese, skipping empty references.

Dictionary source files ship with the package; the entries and the index are generated in the Unity editor and shipped with the world, and nothing is read from a player's local files inside VRChat. The package therefore ships the dictionary source text files (a few tens of MB) instead of pre-applied dictionary data, and once a language has been set up runtime lookup performance is unchanged. Loading supports Undo and keeps the previous dictionary if the file is rejected.

`HXIMEUI.Target Inputfield` must point to a dedicated output field, never to `InputBarHandle/InputBar/InputField` (the pinyin composition field). A wrong binding makes committed text trigger a pinyin candidate refresh; the current version refuses to commit through such a binding. The menu **Tools → HXIME → Repair Output Field and Verify Chinese Selection** repairs this in the current scene: it backs up the current scene, creates a dedicated `OutputField` under the candidate bar, rebinds it and saves the scene. An already correct output field is left untouched. The menu then also opens `HXIME_Pinyin.prefab` in memory and runs a Chinese selection self-test (`ce s`, `ces`, `ce shi`, `c s`, extra spaces, leftover codes), and checks that an output field bound to the composition field is rejected. The prefab is not modified and Play mode is not entered.

The main keyboard and the top language label update together and only show the current language: Chinese `中`, Japanese `JP`, Korean `Ko`, English `En`. Clear, enter, paging, settings and the input hint all follow the language. Letter keys still type the romanized dictionary codes. Built-in skin author descriptions stay as they are.

Font sources and licensing are in `Fonts/MULTILINGUAL-FONT.md`. The menu **Tools → HXIME → Bake Japanese and Korean Font Fallback** generates missing static fonts and connects the fallback; **Validate Multilingual Labels and Glyphs** validates labels, TMP mesh glyphs and Korean `we` candidates in edit mode without entering Play mode. Fonts are pre-generated in the package, so a normal import needs no rebake. All of these tools run in edit mode, never enter Play mode; the result is written to the project's editor temp directory (`Temp/`). Manual work should use the menus above.

## File format

UTF-8 text, one entry per line, separated by real tabs: `word<TAB>code<TAB>weight`. The weight may be omitted and defaults to 0; non-negative integers, percentages (`99.93%`) and decimals (`1.5`) are accepted, and the last two are scaled by 100 into integers (`99.93%` → `9993`, `1.5` → `150`) so the original ordering is preserved. Codes are case insensitive, trimmed, and keep inner spaces. Blank lines, whole-line comments starting with `#`, a UTF-8 BOM and Windows line endings are accepted.

Standard RIME `---` / `...` headers are supported, and the data columns must be text, code, weight in that order. Dictionaries with custom `columns` or `import_tables` must be expanded and converted into the TSV above. Other input methods' encoding schemes are not converted automatically and external sub-dictionaries are not loaded.

Candidates in extra languages are ordered by "exact code first, then descending weight", with identical outputs de-duplicated. By default, up to 50 candidates are produced, shown 5 per page; the limit is configurable in the Inspector. A prefix candidate completes a whole entry; selecting it clears the entire composition string and never truncates the code by the number of output characters. Multi-word phrases must be imported as their own entries.

In the `HXIMEUI` Inspector, set `Candidate Count` (`candidateLimits`) to customize the total candidate limit (default 50, minimum 1). Each page still shows 5 candidates; the actual count depends on matches. Values above 100 display an English performance warning in the Inspector but are still allowed. Higher limits can increase lookup, deduplication and sorting costs, especially with short inputs or large dictionaries; test input latency in the VRChat client.

Every dictionary is stored as a standalone binary `.asset` under `Assets/HXIME_DictionaryData/` (for example `SimpDictPool.asset`), and that asset holds both the entries and the built lookup index; neither the prefab nor the scene ever receives dictionary data or index arrays, and the prefab only mounts the dictionary source file. The asset is created in your own project and is not part of the package. On entering Play mode or building the world, the editor bakes the entries and the index of that asset into Udon; a missing load or index cancels Play mode and fails the world build. This avoids large prefab overrides, and after a language has been loaded and indexed its runtime lookup performance is unchanged.

Space selects the first candidate on the page; number keys 1–5 select a candidate on the page; with no candidates, Space commits the raw code; Enter commits the raw code; Tab clears the composition; Backspace deletes one code character. Switching language commits the unselected raw code so nothing is lost. The Chinese-only simplified/traditional and double-pinyin buttons are shown in Chinese mode only.

## Verification checklist

- A language is usable once both "Load and apply dictionary" and "Rebuild lookup index" have been run for it; missing either step cancels entering Play mode and fails the world build. Once Chinese is loaded and indexed, Chinese, English, simplified/traditional and double pinyin still work.
- Japanese `NIHONGO` yields `日本語`, and `neko` yields `猫` and `ねこ`; Korean `hangugeo` yields `한국어`.
- After typing a prefix, an unmatched code, deleting to an empty string, paging candidates and switching language, no stale or empty candidate is ever committed.
- Import out-of-order entries, duplicate codes, an empty file, negative weights and a RIME header without its terminator, then check the candidates and error messages.
- Verify font rendering and large-dictionary input latency on the target platform. Candidates are located by binary search over the sorted index instead of scanning every entry; a short input can still match a large range, so frame time for very large dictionaries needs retesting in the real client. The index trades extra resident memory for faster lookups, see [Candidate Lookup Optimization](PERFORMANCE-EN.md).

## Related documents

- Dictionary sources, conversion rules and licenses: [Dicts/SOURCES-EN.md](Dicts/SOURCES-EN.md)
- Font sources, bake settings and licensing: [Fonts/MULTILINGUAL-FONT.md](Fonts/MULTILINGUAL-FONT.md)
- Troubleshooting box glyphs in a new input field: [TMP字体方框解决方案.md](TMP字体方框解决方案.md)
- How the indexes and candidate lookup are implemented: [PERFORMANCE-EN.md](PERFORMANCE-EN.md) ｜ [中文](PERFORMANCE.md) ｜ [日本語](PERFORMANCE-JP.md) ｜ [한국어](PERFORMANCE-KO.md)
- Installation and FAQ: [README.md](README.md) ｜ [English](README-EN.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)
