# 日语、韩语词库

本目录新增词库与既存中文词库一样，均为 UTF-8 `.txt` 文本，格式是「词条、输入编码、非负整数权重」，以 Tab 分隔，可直接用 `PinyinDict` 的「加载并应用字典」导入。

| 文件 | 用途 |
| --- | --- |
| `japanese_mozc_common.dict.tsv.txt` | 日语较小版本，按转换权重选取前 30,000 个词条/编码对，建议先测试此版本 |
| `japanese_mozc.dict.tsv.txt` | 日语完整转换结果，覆盖更多词形与专名 |
| `korean_nikl_common.dict.tsv.txt` | 韩语已分级词条按权重选取前 10,000 个词条/编码对，建议先测试此版本 |
| `korean_nikl.dict.tsv.txt` | 韩语基础词典完整转换结果，包括未分级词条 |

“完整”指下述过滤规则后的完整结果，并不包含原始数据的所有内容。具体行数、字节数、SHA-256、上游提交号与每个源文件地址见 `dictionary-manifest.json`。较小版本不是人工审校的高频词表；当前引擎逐条扫描，仍需在目标 VRChat 平台验证延迟。

## 日语来源与处理

- 来源：Google Mozc，`src/data/dictionary_oss/dictionary00.txt` 至 `dictionary09.txt`。
- 固定版本：[`13c98988247aa711d99db9e348ec2a597d14b5cd`](https://github.com/google/mozc/tree/13c98988247aa711d99db9e348ec2a597d14b5cd/src/data/dictionary_oss)。
- 从原始行提取读音、输出词条和词成本；使用 pykakasi 2.3.0 的 Hepburn 输出将**原始假名读音**转换成编码，不从汉字猜测读音。
- 仅处理由完整平假名、片假名及长音符组成的读音；过滤带数字、符号或无法转换成拉丁字母编码的读音。接受编码中的 `'` 和 `-`。
- 权重为 `max(1, 40000 - 原始成本)`，原始成本越低，转换权重越高。它是排序近似值，不是词频，也不包含 Mozc 的词性连接成本或句子分析能力。
- 按「输出词条、编码」去重，保留最大权重；补入项目原有日语示例，权重 50,000。再选出权重最高的 30,000 对作为较小版本。
- 编码保留转换器的长音、拨音等规则，不保证覆盖所有输入法别名；例如 `nihongo` → `日本語`、`toukyou` → `東京`。自动转换结果未逐条人工校对。

版权归 Google LLC、NAIST 及上游注明的其他权利人所有。随附上游原文：

- [Mozc LICENSE](Licenses/mozc-LICENSE.txt)：BSD 3-Clause。
- [词库专项声明](Licenses/mozc-dictionary-NOTICE.txt)：包括 IPAdic / NAIST / ICOT 条款与 Okinawa 数据声明。词库不可仅以 Mozc 代码许可证概括；再分发时一并保留这些声明。

## 韩语来源与处理

- 原作者：韩国国立国语院（국립국어원 / National Institute of Korean Language）。数据集：[한국어기초사전 / Korean Basic Dictionary](https://krdict.korean.go.kr/)。
- 下载来源：社区维护的 [`spellcheck-ko/korean-dict-nikl`](https://github.com/spellcheck-ko/korean-dict-nikl/tree/42c0d01889f34536e9cf94fe57f62bd2055b1bde/krdict)，固定提交 `42c0d01889f34536e9cf94fe57f62bd2055b1bde`，`krdict/001.xml` 至 `011.xml`。该镜像不是国立国语院官方维护。
- 仅提取韩文词头、文本发音和词汇等级，不分发原文例句、释义、音频或其他媒体。
- 解析前移除原始 XML 中不被 XML 1.0 接受的控制字符，数量记录在清单中，原始下载文件不修改。
- 排除以连字符开头/结尾的词缀、独立字母及非纯韩文词头；移除词头内部的连字符分隔符，将 `^` 转为输出空格。
- 用 korean-romanizer 0.28.0 从词头生成罗马字编码；有可处理的文本发音时，另生成发音编码别名。编码去除空格、转小写，输出词条仍保留词间空格。自动转换结果未逐条人工校对，不代表支持所有罗马字拼写习惯。
- 权重：初级 300、中级 200、高级 100、未分级 10。这是学习等级排序，不是真实词频。较小版本从已分级词条与示例中按权重取前 10,000 对；同权重按编码、词条排序以确保结果可复现。重复「词条、编码」保留最大权重。
- 两个版本都补入项目既存韩语示例（权重 1,000），包括 `hangugeo` → `한국어`、`annyeonghaseyo` → `안녕하세요` 等常用输入。

上游与本项目转换后的韩语词库以 **CC BY-SA 2.0 KR** 发布。本项目所做修改为筛选词头、增加罗马字编码、生成排序权重、去重与添加示例。请保留作者、数据来源、本修改说明和[许可证链接](https://creativecommons.org/licenses/by-sa/2.0/kr/)，派生词库保持相同许可。

随附[镜像原始说明](Licenses/nikl-README.md)；[国立国语院版权政策](https://krdict.korean.go.kr/kor/kboardPolicy/copyRightTermsInfo)。这些词库的许可独立于项目代码许可。

## 溯源与校验

本目录只随包发布转换结果与许可证，构建脚本与自动化测试已从仓库移除，因此仓库内无法再重现转换过程。上文的来源、固定提交、过滤规则与权重换算即为可追溯的全部依据；源数据未被再分发，需按上文链接自行获取。

已发布文件可用 `dictionary-manifest.json` 核对：`outputs` 给出每个词库的条目数与 SHA-256，`bytes` 给出字节数，`sources` 与 `source_revisions` 记录上游文件地址与提交号，`conversion_packages` 记录当时使用的转换库版本，`statistics` 记录过滤统计。哈希使用 SHA-256 对文件整体计算（含 `#` 注释行）。

转换只使用了项目少量示例补充，其校验值记录在 `local_supplements` 中。许可证与署名要求不因构建脚本移除而改变，仍按上文各节执行。

导入配置步骤见 [MULTILINGUAL.md](../MULTILINGUAL.md)。选择同一语言的一个版本即可，不需同时加载完整与较小版本。
