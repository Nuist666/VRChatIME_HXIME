# 多语言词库输入

其他语言：[English](MULTILINGUAL-EN.md) ｜ [日本語](MULTILINGUAL-JP.md) ｜ [한국어](MULTILINGUAL-KO.md)

保留中文全拼、简拼、双拼与简繁词库。新增的语言使用独立的「编码 → 词条」词库：例如 `nihongo` → `日本語`、`hangugeo` → `한국어`。可用同样方式增加其他语言。未配置扩展词库时，语言按钮仍只切换中英文。

这是词库驱动的整词输入：支持完整编码和前缀候选，不提供任意罗马字转假名、日语句子分析或韩语二式键盘组字。编码必须与导入的词库一致；也可在预编辑框直接输入词库中定义的假名或韩文编码。

## 在 Unity 配置

新版 `HXIME_Pinyin.prefab` 已挂接日语 30,000 条和韩语 10,000 条精简词库，直接使用该预制件即可循环切换「中 → Ja → Ko → En」。

已导入旧版或场景实例覆盖了词库数组时，在退出 Play 模式后执行 **Tools → HXIME → Configure Japanese and Korean Dictionaries**，即可补齐预制件与当前已加载场景中的词库连接。该操作保留已连接且有效的同名语言词库，不会重复添加；预制件会保存，场景配置后请保存场景。它不会自动进入 Play 模式。

手动配置或替换其他词库时：

1. 在场景的 HXIME 实例下新建两个子物体，分别添加 `PinyinDict` 组件。
2. 在组件检查器将「语言按钮名称」分别设为 `Ja`、`Ko`。通过「浏览文件」和「加载并应用字典」分别导入 `Dicts/japanese_mozc_common.dict.tsv.txt`、`Dicts/korean_nikl_common.dict.tsv.txt`。这两个是较小版本；完整版本、下载来源和转换规则见[词库说明](Dicts/SOURCES.md)。原有 `*_sample.tsv.txt` 仅为少量测试词条。
3. 在已有 `PinyinEngine` 的 `Additional Dicts` 数组中，依次放入两个组件。原有 `Dicts` 的前两项仍是简体、繁体中文，不要改变其含义。
4. 附带的 TMP 字体已连接静态 `NotoSansMultilingualFallback`，覆盖全部现代韩文音节及默认日、韩词库字符。若目标输入框使用项目之外的字体，请将该 fallback 添加到目标字体的 Fallback Font Assets。导入额外词库后应检查新字符是否被字体覆盖。
5. 编译 UdonSharp，在 Unity Play Mode / VRChat ClientSim 验证，再构建世界。现有语言按钮会循环「中文 → Ja → Ko → 英文 → 中文」，空引用会被跳过。

词库是在 Unity 编辑器导入并随世界发布的，不是在 VRChat 内读取玩家的本地文件。导入操作支持 Undo，格式错误时保留旧词库。

`HXIMEUI.Target Inputfield` 必须指向独立的输出框，不能指向 `InputBarHandle/InputBar/InputField`（拼音预编辑框）。误绑会使上屏文字触发拼音候选刷新；新版会阻止这种绑定提交。菜单 **Tools → HXIME → Repair Output Field and Verify Chinese Selection** 可修复当前场景中的此类误绑：先在 `Temp` 备份场景，再在候选栏下方创建独立的 `OutputField`、重新绑定并保存场景。已有正确输出框不会被替换。

主键盘和顶部语言标签同步更新，仅显示当前语言：中文 `中`、日语 `JP`、韩语 `Ko`、英语 `En`；清空、回车、翻页、设置及输入提示随语言切换。字母键仍输入词库的罗马字编码。内置皮肤的作者描述保持原文。

字体来源与授权见 `Fonts/MULTILINGUAL-FONT.md`。菜单 **Tools → HXIME → Bake Japanese and Korean Font Fallback** 可生成缺失的静态字体并连接 fallback；**Validate Multilingual Labels and Glyphs** 在编辑模式验证标签、TMP 网格字形和韩文 `we` 候选，不进入 Play 模式。字体已随包预生成，普通导入无需再次烘焙。

## 文件格式

UTF-8 文本，每行以真正的 Tab 分隔：`词条<Tab>编码<Tab>非负整数权重`。权重可省略，默认为 0；编码忽略大小写、去除首尾空白，内部空格保留。支持空行、以 `#` 开头的整行注释、UTF-8 BOM 和 Windows 换行。

支持标准 RIME `---` / `...` 文件头，数据列顺序必须为 text、code、weight。带自定义 `columns`、`import_tables` 的词库需先展开、转换成上述 TSV；百分比或小数权重也需先转为非负整数。不会自动转换其他输入法的编码方案或加载外部子词库。

扩展语言候选按「完整编码优先，再按权重降序」排序，相同输出去重；最多显示 30 项，每页 5 项。前缀候选表示补全整条词条，选中后清空整个预编辑串，不按输出字符数截断编码。完整词组也应作为独立词条导入。

空格选择当前页首候选；数字 1–5 选择当前页候选；无候选时空格提交原始编码；回车提交原始编码；Tab 清空预编辑；退格删除一个编码字符。切换语言会提交未选中的原始编码，避免丢失输入。中文专用简繁、双拼按钮仅在中文模式显示。

## 验证清单

- 未配置扩展词库时，中英文与简繁、双拼仍可使用。
- 日语输入 `NIHONGO` 得到 `日本語`，输入 `neko` 得到 `猫`、`ねこ`；韩语输入 `hangugeo` 得到 `한국어`。
- 输入前缀、无匹配编码、删除到空串、候选翻页、切换语言后，不出现旧候选或空候选上屏。
- 导入乱序、重复编码、空文件、负数权重与缺失 RIME 结束标记，检查候选和错误信息。
- 在目标平台验证字体显示与大词库输入延迟。扩展词库逐条扫描，候选缓存大小与候选上限相关；大型词库仍需实际性能测试。

## 相关文档

- 词库来源、处理规则与许可证：[Dicts/SOURCES.md](Dicts/SOURCES.md)
- 字体来源、烘焙参数与授权：[Fonts/MULTILINGUAL-FONT.md](Fonts/MULTILINGUAL-FONT.md)
- 新建输入框显示方框的排查：[TMP字体方框解决方案.md](TMP字体方框解决方案.md)
- 安装与常见问题：[README.md](README.md) ｜ [English](README-EN.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)
