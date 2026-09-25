### HXIME v0.9.5 Multilingual Input Keyboard for VRChat

Other languages: [中文](README.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)

An input method keyboard built for VRChat worlds. It started as a Chinese pinyin IME and is now a **configurable multilingual input**: type romanized codes to produce whole words in Japanese, Korean and other languages.

- **Chinese**: full pinyin, initial-letter and mixed spelling, simplified and traditional dictionaries
- **Japanese**: romanized codes such as `nihongo` → `日本語`, `neko` → `猫` / `ねこ`
- **Korean**: romanized codes such as `hangugeo` → `한국어`, `annyeonghaseyo` → `안녕하세요`
- **English**: types directly, switchable with the dictionary modes at any time
- Japanese and Korean can be enabled or disabled individually at the top of `HXIMEUI`; Chinese and English are always enabled
- Dictionaries are imported in the Unity editor and shipped with the world; no local files are read at runtime
- Easy to install, custom skins supported; the language button cycles Chinese → JP → Ko → En

`Dicts` ships dictionary files ready to import: a Mozc Japanese dictionary and the National Institute of Korean Language basic Korean dictionary, each in a compact and a full converted version. See [dictionary notes](Dicts/SOURCES.md) for file choices, sources and licenses.

The current `HXIME_Pinyin.prefab` is wired to the compact Japanese and Korean dictionaries, so language switching works out of the box. For older scenes, run `Tools → HXIME → Configure Japanese and Korean Dictionaries`. Full setup, dictionary file format and feature scope are documented in [Multilingual Input](MULTILINGUAL-EN.md), also available in [中文](MULTILINGUAL.md) ｜ [日本語](MULTILINGUAL-JP.md) ｜ [한국어](MULTILINGUAL-KO.md).

Chinese, Japanese and Korean share sorted indexes and a Top-30 candidate lookup, while Chinese keeps reverse prefix and initials matching. Indexes are built in the editor when a dictionary is imported, when entering Play mode and when the world is built, so a key press only queries the relevant ranges. For old scenes or manually edited dictionaries, rebuild them with `Tools → HXIME → Rebuild All Dictionary Indexes` and save the scene. If a custom script changes dictionary weights at runtime, call the engine's `InvalidateMatchCache()`; codes and entry structure must be changed in the editor and followed by an index rebuild. See [Candidate Lookup Optimization](PERFORMANCE-EN.md) for the reasoning, index structure and verification scope, also available in [中文](PERFORMANCE.md) ｜ [日本語](PERFORMANCE-JP.md) ｜ [한국어](PERFORMANCE-KO.md).

The original author is still learning git, so many thanks to everyone who sends pull requests.

“Let those characters transcend dimensions and let civilization thrive”

Project Link: https://github.com/xianglong90II/VRChatChineseIME_HXIME

# Installation

- Drag the `HXIME_Pinyin` prefab into the map.
- You can adjust `SwitchBarHandle`, `InputBarHandle` and `KeyboardHandle` to your favorite position.
- The first skin is used by default. You can reorder the skins to change the default one.
- (Note that each skin description must match its image one to one. Skins you do not like can simply be removed.)
- In `HXIMEUI` on `HXIME_Pinyin`, select your target input field. Bind a **dedicated output field**, never the IME's own pinyin composition field.
- For multilingual input, configure the extra dictionaries as described in [Multilingual Input](MULTILINGUAL-EN.md); the current prefab already has them set up.
- Done!

# Q&A

- Q: I want the IME to stay in the world and not be grabbable. What should I do?
- A: Delete the VRC PickUp component from `SwitchBarHandle`, `InputBarHandle` and `KeyboardHandle`.
- Q: I don't want users to touch the keyboard toggle or handle toggle buttons. What should I do?
- A: Select the button you want to hide under `Buttons` in `SwitchBar` or `SettingsPanel` and clear the checkbox in the top left of the inspector.
- Q: Which languages are supported? Can I type Japanese kana or Korean?
- A: Languages are dictionary driven. Chinese, Japanese and Korean ship with the package, and English types directly. Japanese and Korean are **whole-word code input**: you type a romanized code and pick a candidate, for example `nihongo` → `日本語`. There is no arbitrary romaji-to-kana conversion, no Japanese sentence analysis and no Korean two-set keyboard composition. You can also type kana or Hangul codes directly in the composition field if the imported dictionary defines them. Import your own dictionary to add more languages.
- Q: Why do I see empty boxes (missing glyphs)?
- A: See [Fixing box glyphs in a new TMP input field](TMP字体方框解决方案.md) and `Fonts/MULTILINGUAL-FONT.md`.

# Advanced

- Q: Can I make my own skin?
- A: Of course! Write a skin description and prepare an image as the skin background. The format is: skin title;skin description;button color rgba;main color 1 rgba;main color 2 rgba
- For example:
- HXIME white;author: HX2 xianglong90;(255,255,255,128);(52,161,255,255);(52,255,209,255)
- To draw skins precisely, use `ThemePSDInstruction.psd` and the png files in the `Themes` folder.
- HXIME uses Texture2D instead of UISprite for skin backgrounds.
- So if you are willing to tinker, you can even load them over the network or use a RenderTexture (that is, capture with an in-game camera).
- Q: How many skins can I add?
- A: Theoretically unlimited. Players currently have slots for the first 9 skins only; call the public method `SetSkin(index)` on the object holding the `HXIMEUI` script to use the 10th skin and beyond.
- Q: How do I import my own dictionary?
- A: Chinese dictionaries live in the simplified and traditional slots of `PinyinEngine`; other languages live in the `Additional Dicts` array, where each entry is a child object with a `PinyinDict` component, and its "language button label" decides the button text.
- Use "Browse..." and "Load and apply dictionary" to import a UTF-8 text dictionary in the format `word<TAB>code<TAB>weight` (weight optional, default 0). Codes for extra languages are romanized and case insensitive.
- As you may have noticed, this is the RIME dictionary format. Standard RIME dictionaries with a `---` / `...` header can be imported directly; dictionaries with custom `columns` / `import_tables` must be expanded into the three columns above. See [Multilingual Input](MULTILINGUAL-EN.md).
- Q: What are the key bindings in extra languages?
- A: Space picks the first candidate on the page; the number keys 1–5 pick a candidate on the page; with no candidate, Space or Enter commits the raw code; Tab clears the composition; Backspace deletes one code character; switching language commits the unselected raw code first.

# Licence: LGPL v3
https://github.com/xianglong90II/VRChatChineseIME_HXIME

# Credit
- Icons: Google Material UI & Fonts https://fonts.google.com/
- Font: Noto Sans / Noto Sans CJK https://fonts.google.com/noto
- Simplified Chinese Dictionary: RIME pinyin-simp https://github.com/rime/rime-pinyin-simp
- Traditional Chinese Dictionary: RIME luna-pinyin https://github.com/rime/rime-luna-pinyin
- Japanese Dictionary: Google Mozc https://github.com/google/mozc (BSD 3-Clause; the dictionary carries additional notices)
- Korean Dictionary: National Institute of Korean Language, Korean Basic Dictionary https://krdict.korean.go.kr/ (CC BY-SA 2.0 KR)
- Sources, conversion rules and full license texts: [Dicts/SOURCES.md](Dicts/SOURCES.md)
