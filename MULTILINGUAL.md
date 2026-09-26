# 多语言词库输入

其他语言：[English](MULTILINGUAL-EN.md) ｜ [日本語](MULTILINGUAL-JP.md) ｜ [한국어](MULTILINGUAL-KO.md)

保留中文全拼、简拼、双拼与简繁词库。新增的语言使用独立的「编码 → 词条」词库：例如 `nihongo` → `日本語`、`hangugeo` → `한국어`。可用同样方式增加其他语言。预制件本身不再包含词条与索引，使用前需要为每个想用的语言手动执行「加载并应用字典」与「重建查询索引」两步。

这是词库驱动的整词输入：支持完整编码和前缀候选，不提供任意罗马字转假名、日语句子分析或韩语二式键盘组字。编码必须与导入的词库一致；也可在预编辑框直接输入词库中定义的假名或韩文编码。

## 在 Unity 配置

`HXIME_Pinyin.prefab` 不再包含任何词条与查询索引：预制件里的四个词库组件（`SimpDictPool`、`TradDictPool`、`JapaneseDictionary`、`KoreanDictionary`）都是空的，各自用一个隐藏字段记录自己挂接的词库源文件：`SimpDictPool` → `Dicts/pinyin_simp.dict.yaml.txt`，`TradDictPool` → `Dicts/luna_pinyin.dict.yaml.txt`，`JapaneseDictionary` → `Dicts/japanese_mozc_common.dict.tsv.txt`，`KoreanDictionary` → `Dicts/korean_nikl_common.dict.tsv.txt`。这四个组件及其源文件已经存在于预制件中，不需要新建组件或重新接线。

每个语言都要手动配置，一次处理一个语言，在该语言组件的 `PinyinDict` 检查器里依次执行两步：**加载并应用字典 / Load and apply dictionary** 会解析挂接的源文件，把词条与权重写入独立的二进制词库资产 `Assets/HXIME_DictionaryData/<组件对象名>.asset`（例如 `SimpDictPool.asset`），这一步不构建查询索引，也不会改动预制件或场景；**重建查询索引 / Rebuild lookup index** 会在同一资产内构建排序索引。词条与索引只存在于这个二进制资产中，资产在用户自己的工程里生成，不属于随包发布的预制件。

也可以使用按语言分组的菜单：`Tools → HXIME → Simplified Chinese → Load and Apply Dictionary` / `Rebuild Lookup Index`、`Tools → HXIME → Traditional Chinese → Load and Apply Dictionary` / `Rebuild Lookup Index`、`Tools → HXIME → Japanese → Load and Apply Dictionary` / `Rebuild Lookup Index` / `Enable Language Button`、`Tools → HXIME → Korean → Load and Apply Dictionary` / `Rebuild Lookup Index` / `Enable Language Button`，以及 `Tools → HXIME → Validate All Dictionaries`、`Tools → HXIME → Load and Apply All Dictionaries`、`Tools → HXIME → Rebuild All Lookup Indexes`。旧菜单 `Tools → HXIME → Rebuild All Dictionary Indexes` 与 `Tools → HXIME → Configure Japanese and Korean Dictionaries` 已不存在。

如果某个语言尚未加载，或尚未重建索引，对应 `PinyinDict` 检查器会显示英文 ERROR 提示（例如 `ERROR: HXIME Simplified Chinese dictionary data is not loaded. Click "Load and apply dictionary" in the PinyinDict inspector (source: Dicts/pinyin_simp.dict.yaml.txt).`、`ERROR: HXIME Simplified Chinese lookup index is not built. Click "Rebuild lookup index" in the PinyinDict inspector.`），进入 Play 模式会被取消（对话框会列出缺失的语言），构建世界也会立即失败。

日语和韩语可以按需启用：`HXIMEUI` 最上面的「是否启用日文?」「是否启用韩文?」开关决定该语言是否进入语言循环；关闭后语言按钮会跳过它，也不会查询对应的词库。中文与英文始终启用。词库组件本身不会被删除，重新启用只需把开关打开，也可以用 `Tools → HXIME → Japanese → Enable Language Button` / `Tools → HXIME → Korean → Enable Language Button`。

预制件中的简体、繁体组件仍连接在 `PinyinEngine` 的 `Dicts` 前两项，日语、韩语组件仍在 `Additional Dicts` 数组中，这些都无需手动改动。此外：

1. 附带的 TMP 字体已连接静态 `NotoSansMultilingualFallback`，覆盖全部现代韩文音节及默认日、韩词库字符。若目标输入框使用项目之外的字体，请将该 fallback 添加到目标字体的 Fallback Font Assets。加载额外词库后应检查新字符是否被字体覆盖。
2. 编译 UdonSharp，在 Unity Play Mode / VRChat ClientSim 验证，再构建世界。现有语言按钮会循环「中文 → JP → Ko → 英文 → 中文」，空引用会被跳过。

词库源文件随包发布，词条与索引在 Unity 编辑器中生成并随世界发布，不是在 VRChat 内读取玩家的本地文件。随包发布的是词库源文本文件（数十 MB），而不是预先应用好的词库数据；完成某个语言的加载与索引后，运行时的查询性能与之前一致。加载操作支持 Undo，格式错误时保留旧词库。

`HXIMEUI.Target Inputfield` 必须指向独立的输出框，不能指向 `InputBarHandle/InputBar/InputField`（拼音预编辑框）。误绑会使上屏文字触发拼音候选刷新；新版会阻止这种绑定提交。菜单 **Tools → HXIME → Repair Output Field and Verify Chinese Selection** 可修复当前场景中的此类误绑：会先备份当前场景，再在候选栏下方创建独立的 `OutputField`、重新绑定并保存场景。已有正确输出框不会被替换。该菜单随后还会在内存中打开 `HXIME_Pinyin.prefab` 跑一轮中文选词自测（`ce s`、`ces`、`ce shi`、`c s`、前后多空格、带残余编码），并确认把输出框误绑到预编辑框时会被拒绝；不修改预制件、也不进入 Play 模式。

主键盘和顶部语言标签同步更新，仅显示当前语言：中文 `中`、日语 `JP`、韩语 `Ko`、英语 `En`；清空、回车、翻页、设置及输入提示随语言切换。字母键仍输入词库的罗马字编码。内置皮肤的作者描述保持原文。

字体来源与授权见 `Fonts/MULTILINGUAL-FONT.md`。菜单 **Tools → HXIME → Bake Japanese and Korean Font Fallback** 可生成缺失的静态字体并连接 fallback；**Validate Multilingual Labels and Glyphs** 在编辑模式验证标签、TMP 网格字形和韩文 `we` 候选，不进入 Play 模式。字体已随包预生成，普通导入无需再次烘焙。这些工具都在编辑模式运行、不会进入 Play 模式，结果写入工程的编辑器临时目录（`Temp/`）。手动操作直接用上面的菜单即可。

## 文件格式

UTF-8 文本，每行以真正的 Tab 分隔：`词条<Tab>编码<Tab>权重`。权重可省略，默认为 0；接受非负整数、百分比（`99.93%`）和小数（`1.5`）三种写法，后两种按 1/100 精度放大成整数（`99.93%` → `9993`、`1.5` → `150`）以保持原有排序。编码忽略大小写、去除首尾空白，内部空格保留。支持空行、以 `#` 开头的整行注释、UTF-8 BOM 和 Windows 换行。

支持标准 RIME `---` / `...` 文件头，数据列顺序必须为 text、code、weight。带自定义 `columns`、`import_tables` 的词库需先展开、转换成上述 TSV。不会自动转换其他输入法的编码方案或加载外部子词库。

扩展语言候选按「完整编码优先，再按权重降序」排序，相同输出去重；候选上限默认 50 项，可在 Inspector 配置，每页 5 项。前缀候选表示补全整条词条，选中后清空整个预编辑串，不按输出字符数截断编码。完整词组也应作为独立词条导入。

在 `HXIMEUI` 的 Inspector 中，通过 `Candidate Count`（`candidateLimits`）设置候选词总数上限，默认 50，最小 1；每页仍显示 5 个，实际数量取决于匹配结果。超过 100 时 Inspector 会显示英文性能警告，但不会限制该值。较高上限可能增加查询、去重和排序开销，尤其是短输入或大词库，请在 VRChat 客户端实测输入延迟。

所有词库都保存为 `Assets/HXIME_DictionaryData/` 下独立的二进制 `.asset`（例如 `SimpDictPool.asset`），资产内同时保存词条与预建索引；预制件与场景都不再记录词库数据或索引数组，预制件只挂接词库源文件。资产在用户自己的工程里生成，不属于随包发布的预制件。进入 Play 模式或构建世界时，编辑器会把该资产中的词条和索引烘焙进 Udon；缺失的加载或索引会阻止进入 Play 模式与构建世界。此改动避免大数组的预制件覆盖开销；某个语言完成加载与索引后，运行时的查询性能与之前一致。

空格选择当前页首候选；数字 1–5 选择当前页候选；无候选时空格提交原始编码；回车提交原始编码；Tab 清空预编辑；退格删除一个编码字符。切换语言会提交未选中的原始编码，避免丢失输入。中文专用简繁、双拼按钮仅在中文模式显示。

## 验证清单

- 为某个语言执行「加载并应用字典」与「重建查询索引」后该语言可用；缺少任一步会取消进入 Play 模式并让构建世界失败。完成中文的加载与索引后，中英文与简繁、双拼仍可使用。
- 日语输入 `NIHONGO` 得到 `日本語`，输入 `neko` 得到 `猫`、`ねこ`；韩语输入 `hangugeo` 得到 `한국어`。
- 输入前缀、无匹配编码、删除到空串、候选翻页、切换语言后，不出现旧候选或空候选上屏。
- 导入乱序、重复编码、空文件、负数权重与缺失 RIME 结束标记，检查候选和错误信息。
- 在目标平台验证字体显示与大词库输入延迟。候选按排序索引二分定位匹配区间，不再逐条全量扫描；短输入仍可能命中较大区间，超大词库的帧耗时需在实际客户端复测。索引以额外常驻内存换取查询效率，详见[候选查询优化说明](PERFORMANCE.md)。

## 相关文档

- 词库来源、处理规则与许可证：[Dicts/SOURCES.md](Dicts/SOURCES.md)
- 字体来源、烘焙参数与授权：[Fonts/MULTILINGUAL-FONT.md](Fonts/MULTILINGUAL-FONT.md)
- 新建输入框显示方框的排查：[TMP字体方框解决方案.md](TMP字体方框解决方案.md)
- 索引与候选查询的实现说明：[PERFORMANCE.md](PERFORMANCE.md) ｜ [English](PERFORMANCE-EN.md) ｜ [日本語](PERFORMANCE-JP.md) ｜ [한국어](PERFORMANCE-KO.md)
- 安装与常见问题：[README.md](README.md) ｜ [English](README-EN.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)
