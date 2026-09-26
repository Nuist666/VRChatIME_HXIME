# Candidate Lookup Optimization for Chinese, Japanese and Korean

Other languages: [中文](PERFORMANCE.md) ｜ [日本語](PERFORMANCE-JP.md) ｜ [한국어](PERFORMANCE-KO.md)

## Goal

Reduce the full-dictionary scans and temporary allocations performed on every key press, so typing stutters less. The candidate limit defaults to 50, is configurable in the Inspector, and is shown 5 per page.

In the `HXIMEUI` Inspector, set `Candidate Count` (`candidateLimits`) to customize the total candidate limit (default 50, minimum 1). Each page still shows 5 candidates; the actual count depends on matches. Values above 100 display an English performance warning in the Inspector but are still allowed. Higher limits can increase lookup, deduplication and sorting costs, especially with short inputs or large dictionaries; test input latency in the VRChat client.

## Main changes

- **One indexed lookup path**: Chinese, Japanese and Korean share sorted indexes, using binary search to locate the exact and prefix match ranges.
- **Language rules preserved**: Chinese keeps double-pinyin conversion, reverse prefix and initials matching; Japanese and Korean keep romanized prefix matching and output de-duplication.
- **Fewer allocations**: Chinese now streams a Top-K result and no longer allocates candidate arrays sized to the dictionary; the query workspace is reused and initials are precomputed in the editor.
- **No repeated work**: the most recent query is cached and keyed by dictionary, input, candidate limit and match mode; the logging call in the match hot path is gone.
- **Indexes built up front**: running "Rebuild lookup index" from the `PinyinDict` inspector or the `Tools → HXIME` menus generates the sorted index in the editor and writes it into the standalone binary dictionary asset; when entering Play mode or building the world the editor then bakes that asset's entries and index into the Udon data.

## Usage

The optimization applies once "Load and apply dictionary" and "Rebuild lookup index" have been run: click **重建查询索引 / Rebuild lookup index** in the `PinyinDict` inspector, or use `Tools → HXIME → Simplified Chinese / Traditional Chinese / Japanese / Korean → Rebuild Lookup Index` or `Tools → HXIME → Rebuild All Lookup Indexes` (the old `Tools → HXIME → Rebuild All Dictionary Indexes` menu no longer exists). A rebuild writes only the standalone binary dictionary asset (`Assets/HXIME_DictionaryData/`); it changes neither the prefab nor the scene, so the editor cannot get stuck. Rebuilding a single dictionary from the `PinyinDict` inspector gives the same result as the menu, but only that component is touched.

If a custom script changes weights at runtime, call `PinyinEngine.InvalidateMatchCache()`; codes and entry structure must be changed in the editor and followed by running "Load and apply dictionary" and "Rebuild lookup index" again. A language whose data has not been loaded or whose index has not been built cancels Play mode and fails the world build, and its `PinyinDict` inspector shows an English ERROR HelpBox.

## Verification and limits

- Compiles with Unity 2022.3.22f1 / UdonSharp.
- During development, 1,197 candidate checks and cache checks passed, covering simplified and traditional Chinese, Japanese, Korean, tied scores, duplicate entries and double pinyin.
- Verified that the indexes of all four dictionaries can be written into the data Udon actually uses.
- These are compile and correctness verifications; the frame-time improvement in the VRChat client was not measured, and a short input can still match a large range. The indexes trade extra resident memory for faster lookups.

The temporary test scripts used during development were removed after verification; the bundled `Tools → HXIME → Rebuild All Lookup Indexes`, the per-language `Rebuild Lookup Index` and `Validate Multilingual Labels and Glyphs` menus are unaffected and still available.

## Related documents

- Multilingual dictionary setup and file format: [MULTILINGUAL-EN.md](MULTILINGUAL-EN.md) ｜ [中文](MULTILINGUAL.md) ｜ [日本語](MULTILINGUAL-JP.md) ｜ [한국어](MULTILINGUAL-KO.md)
- Dictionary sources, conversion rules and licenses: [Dicts/SOURCES-EN.md](Dicts/SOURCES-EN.md)
- Installation and FAQ: [README-EN.md](README-EN.md) ｜ [中文](README.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)
