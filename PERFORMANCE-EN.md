# Candidate Lookup Optimization for Chinese, Japanese and Korean

Other languages: [中文](PERFORMANCE.md) ｜ [日本語](PERFORMANCE-JP.md) ｜ [한국어](PERFORMANCE-KO.md)

## Goal

Reduce the full-dictionary scans and temporary allocations performed on every key press, so typing stutters less. The candidate limit stays at 30, shown 5 per page.

## Main changes

- **One indexed lookup path**: Chinese, Japanese and Korean share sorted indexes, using binary search to locate the exact and prefix match ranges.
- **Language rules preserved**: Chinese keeps double-pinyin conversion, reverse prefix and initials matching; Japanese and Korean keep romanized prefix matching and output de-duplication.
- **Fewer allocations**: Chinese now streams a Top-30 result and no longer allocates candidate arrays sized to the dictionary; the query workspace is reused and initials are precomputed in the editor.
- **No repeated work**: the most recent query is cached and keyed by dictionary, input, candidate limit and match mode; the logging call in the match hot path is gone.
- **Indexes built up front**: indexes are generated in the editor and written into the Udon data when a dictionary is imported, when entering Play mode and when the world is built.

## Usage

Importing, running and building normally applies the optimization automatically. For old scenes or manually edited dictionaries, rebuild the indexes with `Tools → HXIME → Rebuild All Dictionary Indexes`, then save the scene. A single dictionary can also be rebuilt from the `PinyinDict` inspector with **重建查询索引 / Rebuild lookup index**: the result matches the menu, but only that component is touched.

If a custom script changes weights at runtime, call `PinyinEngine.InvalidateMatchCache()`; codes and entry structure must be changed in the editor and followed by an index rebuild. Dictionaries without a generated index keep a compatible lookup path.

## Verification and limits

- Compiles with Unity 2022.3.22f1 / UdonSharp.
- During development, 1,197 candidate checks and cache checks passed, covering simplified and traditional Chinese, Japanese, Korean, tied scores, duplicate entries and double pinyin.
- Verified that the indexes of all four dictionaries can be written into the data Udon actually uses.
- These are compile and correctness verifications; the frame-time improvement in the VRChat client was not measured, and a short input can still match a large range. The indexes trade extra resident memory for faster lookups.

The temporary test scripts used during development were removed after verification; the bundled `Rebuild All Dictionary Indexes` and `Validate Multilingual Labels and Glyphs` menus are unaffected and still available.

## Related documents

- Multilingual dictionary setup and file format: [MULTILINGUAL-EN.md](MULTILINGUAL-EN.md) ｜ [中文](MULTILINGUAL.md) ｜ [日本語](MULTILINGUAL-JP.md) ｜ [한국어](MULTILINGUAL-KO.md)
- Dictionary sources, conversion rules and licenses: [Dicts/SOURCES.md](Dicts/SOURCES.md)
- Installation and FAQ: [README-EN.md](README-EN.md) ｜ [中文](README.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)
