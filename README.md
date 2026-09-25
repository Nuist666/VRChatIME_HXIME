### HXIME v0.9 多语言输入法+键盘 VRChat 世界用多语言输入法 Multilingual Input Keyboard

其他语言：[English](README-EN.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)

专为 VRChat 世界打造的输入法键盘，最初用于中文拼音输入，现在已扩展为**可配置的多语言输入**：用罗马字编码直接打出日语、韩语等各种语言的词条。

- **中文**：全拼、简拼、混拼，简体／繁体词库切换
- **日语**：罗马字编码输入，如 `nihongo` → `日本語`、`neko` → `猫`／`ねこ`
- **韩语**：罗马字编码输入，如 `hangugeo` → `한국어`、`annyeonghaseyo` → `안녕하세요`
- **英语**：直接上屏，可随时与词库输入切换
- 词库在 Unity 编辑器导入并随世界发布，不读取玩家本地文件
- 安装简便，支持自定义皮肤；语言按钮循环「中 → Ja → Ko → En」

`Dicts` 中已提供可直接导入的 Mozc 日语词库与国立国语院基础韩语词库，包含精简版本和完整转换版本。文件选择、数据来源及许可证见[词库说明](Dicts/SOURCES.md)。

新版 `HXIME_Pinyin.prefab` 默认连接日韩精简词库，开箱即可切换语言；旧场景可用菜单 `Tools → HXIME → Configure Japanese and Korean Dictionaries` 补齐连接。完整配置方法、词库文件格式与功能范围见[多语言使用说明](MULTILINGUAL.md)，各语言版本见 [English](MULTILINGUAL-EN.md) ｜ [日本語](MULTILINGUAL-JP.md) ｜ [한국어](MULTILINGUAL-KO.md)。

作者还不会用git，所以非常感谢帮忙提交PR的大家。

“让文字跨越次元，让文明生生不息”

Project Link: https://github.com/xianglong90II/VRChatChineseIME_HXIME

# 安装方法

- 把 `HXIME_Pinyin` 预制件拖进地图里。
- 你可以把 `SwitchBarHandle`、`InputBarHandle`、`KeyboardHandle` 调整到你喜欢的位置上。
- 默认会使用第一个皮肤，你可以调整皮肤的顺序，实现更换默认皮肤。
- （注意，皮肤描述和图片必须一一对应哦。当然，觉得有些不太合适的皮肤可以移除）
- 在 `HXIME_Pinyin` 的 `HXIMEUI` 里面，目标输入框那地方选择你的目标输入框。注意要绑定**独立的输出框**，不要绑定输入法自身的拼音预编辑框。
- 需要多语言输入时，按[多语言使用说明](MULTILINGUAL.md)配置扩展词库；新版预制件已默认配置好。
- 完成！

# Q&A

- Q: 我想要把输入法固定在世界里，不想让玩家抓取怎么办？
- A: 把 `SwitchBarHandle`、`InputBarHandle`、`KeyboardHandle` 的 VRC PickUp 删掉。
- Q：我不希望用户能碰到开关键盘、开关握把之类的按钮怎么办？
- A: 在 `SwitchBar` 或 `SettingsPanel` 下的 `Buttons` 里面选中你想要隐藏的按钮，把检查器左上角的勾去掉就行。
- Q: 具体支持哪些语言？能不能打日语的假名或韩语？
- A: 语言由词库决定，目前随包提供中文、日语、韩语，英语直接上屏。日语、韩语都是**整词编码输入**：输入罗马字编码得到候选词条，例如 `nihongo` → `日本語`。它不提供任意罗马字转假名、日语句子分析或韩语二式键盘组字；也可以直接在预编辑框输入词库中已定义的假名或韩文编码。只要按说明导入词库，就能增加其他语言。
- Q: 打出方框（缺字）怎么办？
- A: 见[新建 TMP 输入框显示方框的解决方案](TMP字体方框解决方案.md)，以及 `Fonts/MULTILINGUAL-FONT.md`。

# 进阶

- Q: 可以制作自己的皮肤吗？
- A: 当然可以！您只需要写好皮肤描述和准备一张图片作为皮肤背景。皮肤描述的格式为: 皮肤标题;皮肤描述;按钮颜色的rgba;主色1的rgba;主色2的rgba
- 例如：
- HXIME白;作者: HX2 xianglong90;(255,255,255,128);(52,161,255,255);(52,255,209,255)
- 想要精确绘制皮肤，在 `Themes` 文件夹里可以找到 `ThemePSDInstruction.psd` 文件和 png 文件辅助您绘制！
- 对于皮肤背景，HXIME 使用了 Texture2D 而非 UISprite。
- 也就是说如果愿意折腾的话，您甚至可以尝试线上加载或是使用 RenderTexture（即用游戏内摄像机拍摄）
- Q: 可以放多少套皮肤？
- A：理论上基本无限。但是玩家目前只有前 9 个皮肤有槽位可以选。当然，可以从装有 `HXIMEUI` 这个脚本的物体上调用 `SetSkin(索引)` 这个公开方法来设置第 10 套及后面的皮肤。
- Q: 我想要导入自己的字词库怎么办？
- A: 中文词库在 `PinyinEngine` 的简体、繁体词库数组中；其他语言在 `Additional Dicts` 数组里，每一项是一个带 `PinyinDict` 组件的子物体，用「语言按钮名称」决定语言按钮显示的文字。
- 用「浏览文件」和「加载并应用字典」导入 UTF-8 文本字典即可，格式为「词条 tab 编码 tab 权重（可省略，默认为 0）」。扩展语言的编码是罗马字，编码忽略大小写。
- 您可能已经发现了，这与 RIME 字典的格式一致！带 `---` / `...` 文件头的标准 RIME 字典也可以直接导入；带自定义 `columns` / `import_tables` 的字典需要先展开成上述三列。详见[多语言使用说明](MULTILINGUAL.md)。
- Q: 扩展语言有没有快捷键说明？
- A: 空格选择当前页首候选；数字 1–5 选择当前页候选；没有候选时空格或回车提交原始编码；Tab 清空预编辑；退格删除一个编码字符；切换语言会先提交未选中的原始编码。

# Licence: LGPL v3
https://github.com/xianglong90II/VRChatChineseIME_HXIME

# Credit
- 图标 Icons: Google Material UI & Fonts https://fonts.google.com/
- 字体 Font: Noto Sans / Noto Sans CJK https://fonts.google.com/noto
- 简体词库 Simplified Chinese Dictionary：RIME 袖珍简化字拼音 https://github.com/rime/rime-pinyin-simp
- 繁体词库 Traditional Chinese Dictionary：RIME 朙月拼音 https://github.com/rime/rime-luna-pinyin
- 日语词库 Japanese Dictionary: Google Mozc https://github.com/google/mozc （BSD 3-Clause，词库另有专项声明）
- 韩语词库 Korean Dictionary: 韩国国立国语院 韩国语基础词典 https://krdict.korean.go.kr/ （CC BY-SA 2.0 KR）
- 词库来源、处理方式与许可证全文见 [Dicts/SOURCES.md](Dicts/SOURCES.md)
