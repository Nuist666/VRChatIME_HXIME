# 中日韩候选查询优化说明

其他语言：[English](PERFORMANCE-EN.md) ｜ [日本語](PERFORMANCE-JP.md) ｜ [한국어](PERFORMANCE-KO.md)

## 优化目标

减少每次按键查询词库时的全量扫描和临时分配，降低输入卡顿。候选上限默认 50 个，可在 Inspector 配置，每页显示 5 个。

在 `HXIMEUI` 的 Inspector 中，通过 `Candidate Count`（`candidateLimits`）设置候选词总数上限，默认 50，最小 1；每页仍显示 5 个，实际数量取决于匹配结果。超过 100 时 Inspector 会显示英文性能警告，但不会限制该值。较高上限可能增加查询、去重和排序开销，尤其是短输入或大词库，请在 VRChat 客户端实测输入延迟。

## 主要改动

- **统一索引查询**：中文、日语和韩语共用排序索引，通过二分查找定位精确及前缀匹配区间。
- **保留语言规则**：中文保留双拼转换、反向前缀和简拼匹配；日语、韩语保留罗马字前缀匹配及输出词去重。
- **减少分配**：中文改为流式 Top-K，移除词库大小的临时候选数组；复用查询缓冲区，简拼在编辑器预计算。
- **避免重复计算**：缓存最近一次查询，区分词库、输入、候选上限及匹配模式，并移除匹配热路径日志。
- **提前构建索引**：在 `PinyinDict` 检查器或 `Tools → HXIME` 菜单执行「重建查询索引」时，在编辑器生成排序索引并写入独立二进制词库资产；进入 Play 模式和构建世界时，编辑器再把该资产中的词条与索引烘焙进 Udon 数据。

## 使用方式

正常执行「加载并应用字典」与「重建查询索引」后优化即生效：在 `PinyinDict` 检查器里点 **重建查询索引 / Rebuild lookup index**，或用 `Tools → HXIME → Simplified Chinese / Traditional Chinese / Japanese / Korean → Rebuild Lookup Index`、`Tools → HXIME → Rebuild All Lookup Indexes`（旧的 `Tools → HXIME → Rebuild All Dictionary Indexes` 菜单已不存在）。重建只写独立二进制词库资产（`Assets/HXIME_DictionaryData/`），既不改动预制件也不改动场景，所以不会卡住编辑器。单个词库在 `PinyinDict` 检视面板单独重建的结果与菜单相同，但只处理该组件。

若自定义脚本在运行时修改权重，需调用 `PinyinEngine.InvalidateMatchCache()`；编码和词条结构应在编辑器修改后，重新执行「加载并应用字典」与「重建查询索引」。未加载数据或未构建索引的语言会取消进入 Play 模式并让构建世界失败，对应 `PinyinDict` 检查器会显示英文 ERROR 提示。

## 验证与限制

- Unity 2022.3.22f1 / UdonSharp 编译通过。
- 开发阶段通过 1,197 组候选检查及缓存检查，覆盖简繁中文、日语、韩语、同分排序、重复词条和双拼。
- 验证四套词库的索引可写入 Udon 实际使用的数据。
- 上述为编译与正确性验证，未测量 VRChat 客户端的帧耗时改善；短输入仍可能命中较大区间。索引以额外常驻内存换取查询效率。

开发阶段临时使用的测试脚本已在验证完成后清理；随包提供的 `Tools → HXIME → Rebuild All Lookup Indexes`、各语言的 `Rebuild Lookup Index` 与 `Validate Multilingual Labels and Glyphs` 菜单不受影响，仍然可用。

## 相关文档

- 多语言词库配置与文件格式：[MULTILINGUAL.md](MULTILINGUAL.md) ｜ [English](MULTILINGUAL-EN.md) ｜ [日本語](MULTILINGUAL-JP.md) ｜ [한국어](MULTILINGUAL-KO.md)
- 词库来源、处理规则与许可证：[Dicts/SOURCES.md](Dicts/SOURCES.md)
- 安装与常见问题：[README.md](README.md) ｜ [English](README-EN.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)
