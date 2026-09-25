# Multilingual Dictionary Input

Other languages: [中文](MULTILINGUAL.md) ｜ [日本語](MULTILINGUAL-JP.md) ｜ [한국어](MULTILINGUAL-KO.md)

Chinese full pinyin, initial-letter spelling, double pinyin and the simplified/traditional dictionaries are unchanged. Every additional language uses its own "code to word" dictionary, for example `nihongo` → `日本語` or `hangugeo` → `한국어`. More languages can be added the same way. With no extra dictionary configured, the language button still only toggles Chinese and English.

This is dictionary-driven whole-word input: complete codes and prefix candidates are supported, but there is no arbitrary romaji-to-kana conversion, no Japanese sentence analysis and no Korean two-set keyboard composition. Codes must match the imported dictionary; you can also type kana or Hangul codes defined in the dictionary directly into the composition field.

## Setting up in Unity

The current `HXIME_Pinyin.prefab` is wired to a 30,000-entry compact Japanese dictionary and a 10,000-entry compact Korean dictionary, so using that prefab cycles Chinese → JP → Ko → En out of the box.

Japanese and Korean are optional: the "Enable Japanese" / "Enable Korean" switches (是否启用日文? / 是否启用韩文?) at the top of `HXIMEUI` decide whether that language takes part in the language cycle; when switched off the language button skips it and its dictionary is never queried. Chinese and English are always enabled. The dictionary components themselves are not removed, so re-enabling only needs the switch turned back on (or `Tools → HXIME → Configure Japanese and Korean Dictionaries` to reconnect them).

If you imported an older package, or a scene instance overrides the dictionary array, exit Play mode and run **Tools → HXIME → Configure Japanese and Korean Dictionaries** to complete the wiring on the prefab and on the currently loaded scenes. The action keeps already connected valid dictionaries of the same language and never adds duplicates; the prefab is saved, and you should save the scene after it runs. It does not enter Play mode.

To configure or replace dictionaries manually:

1. Create two child objects under the HXIME instance in your scene and add a `PinyinDict` component to each.
2. In the inspector, set "language button label" to `Ja` and `Ko`. Use "Browse..." and "Load and apply dictionary" to import `Dicts/japanese_mozc_common.dict.tsv.txt` and `Dicts/korean_nikl_common.dict.tsv.txt`. These are the compact versions; see [dictionary notes](Dicts/SOURCES.md) for the full versions, upstream sources and conversion rules. The existing `*_sample.tsv.txt` files only contain a few test entries.
3. Put both components into the `Additional Dicts` array of the `PinyinEngine`. The first two entries of `Dicts` remain simplified and traditional Chinese; do not change their meaning.
4. The bundled TMP fonts are connected to the static `NotoSansMultilingualFallback`, which covers every modern Hangul syllable plus the characters used by the default Japanese and Korean dictionaries. If your target input field uses a font outside this project, add that fallback to the target font's Fallback Font Assets. After importing extra dictionaries, check that the new characters are covered.
5. Compile UdonSharp, verify in Unity Play Mode / VRChat ClientSim, then build the world. The language button cycles Chinese → JP → Ko → English → Chinese, skipping empty references.

Dictionaries are imported in the Unity editor and shipped with the world; nothing is read from a player's local files inside VRChat. Imports support Undo and keep the previous dictionary if the file is rejected.

`HXIMEUI.Target Inputfield` must point to a dedicated output field, never to `InputBarHandle/InputBar/InputField` (the pinyin composition field). A wrong binding makes committed text trigger a pinyin candidate refresh; the current version refuses to commit through such a binding. The menu **Tools → HXIME → Repair Output Field and Verify Chinese Selection** repairs this in the current scene: it backs up the scene into `Temp`, creates a dedicated `OutputField` under the candidate bar, rebinds it and saves the scene. An already correct output field is left untouched. The menu then also opens `HXIME_Pinyin.prefab` in memory and runs a Chinese selection self-test (`ce s`, `ces`, `ce shi`, `c s`, extra spaces, leftover codes), and checks that an output field bound to the composition field is rejected; the result goes to `Temp/HXIME-input-repair.txt`. The prefab is not modified and Play mode is not entered.

The main keyboard and the top language label update together and only show the current language: Chinese `中`, Japanese `JP`, Korean `Ko`, English `En`. Clear, enter, paging, settings and the input hint all follow the language. Letter keys still type the romanized dictionary codes. Built-in skin author descriptions stay as they are.

Font sources and licensing are in `Fonts/MULTILINGUAL-FONT.md`. The menu **Tools → HXIME → Bake Japanese and Korean Font Fallback** generates missing static fonts and connects the fallback; **Validate Multilingual Labels and Glyphs** validates labels, TMP mesh glyphs and Korean `we` candidates in edit mode without entering Play mode. Fonts are pre-generated in the package, so a normal import needs no rebake. All of these tools run in edit mode, never enter Play mode, and write their result into `Temp/`: font baking `Temp/HXIME-font-setup.txt`, dictionary setup `Temp/HXIME-language-setup.json`, output-field repair and Chinese selection self-test `Temp/HXIME-input-repair.txt`, label and glyph validation `Temp/HXIME-ui-validation.txt`. For batch or automated runs you can drop an empty `HXIME-font-setup.request`, `HXIME-language-setup.request`, `HXIME-input-repair.request` or `HXIME-ui-validation.request` into `Temp/`; the editor then runs the matching tool once compilation and importing have finished. Manual work should use the menus above.

## File format

UTF-8 text, one entry per line, separated by real tabs: `word<TAB>code<TAB>non-negative integer weight`. The weight may be omitted and defaults to 0; codes are case insensitive, trimmed, and keep inner spaces. Blank lines, whole-line comments starting with `#`, a UTF-8 BOM and Windows line endings are accepted.

Standard RIME `---` / `...` headers are supported, and the data columns must be text, code, weight in that order. Dictionaries with custom `columns` or `import_tables` must be expanded and converted into the TSV above; percentage or fractional weights must be converted to non-negative integers first. Other input methods' encoding schemes are not converted automatically and external sub-dictionaries are not loaded.

Candidates in extra languages are ordered by "exact code first, then descending weight", with identical outputs de-duplicated. Up to 30 candidates are produced, shown 5 per page. A prefix candidate completes a whole entry; selecting it clears the entire composition string and never truncates the code by the number of output characters. Multi-word phrases must be imported as their own entries.

Space selects the first candidate on the page; number keys 1–5 select a candidate on the page; with no candidates, Space commits the raw code; Enter commits the raw code; Tab clears the composition; Backspace deletes one code character. Switching language commits the unselected raw code so nothing is lost. The Chinese-only simplified/traditional and double-pinyin buttons are shown in Chinese mode only.

## Verification checklist

- With no extra dictionary configured, Chinese, English, simplified/traditional and double pinyin still work.
- Japanese `NIHONGO` yields `日本語`, and `neko` yields `猫` and `ねこ`; Korean `hangugeo` yields `한국어`.
- After typing a prefix, an unmatched code, deleting to an empty string, paging candidates and switching language, no stale or empty candidate is ever committed.
- Import out-of-order entries, duplicate codes, an empty file, negative weights and a RIME header without its terminator, then check the candidates and error messages.
- Verify font rendering and large-dictionary input latency on the target platform. Candidates are located by binary search over the sorted index instead of scanning every entry; a short input can still match a large range, so frame time for very large dictionaries needs retesting in the real client. The index trades extra resident memory for faster lookups, see [Candidate Lookup Optimization](PERFORMANCE-EN.md).

## Related documents

- Dictionary sources, conversion rules and licenses: [Dicts/SOURCES.md](Dicts/SOURCES.md)
- Font sources, bake settings and licensing: [Fonts/MULTILINGUAL-FONT.md](Fonts/MULTILINGUAL-FONT.md)
- Troubleshooting box glyphs in a new input field: [TMP字体方框解决方案.md](TMP字体方框解决方案.md)
- How the indexes and candidate lookup are implemented: [PERFORMANCE-EN.md](PERFORMANCE-EN.md) ｜ [中文](PERFORMANCE.md) ｜ [日本語](PERFORMANCE-JP.md) ｜ [한국어](PERFORMANCE-KO.md)
- Installation and FAQ: [README.md](README.md) ｜ [English](README-EN.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)
