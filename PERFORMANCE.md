# 中日韩候选查询优化说明

其他语言：[English](PERFORMANCE-EN.md) ｜ [日本語](PERFORMANCE-JP.md) ｜ [한국어](PERFORMANCE-KO.md)

## 优化目标

减少每次按键查询词库时的全量扫描和临时分配，降低输入卡顿。候选上限仍为 30 个，每页显示 5 个。

## 主要改动

- **统一索引查询**：中文、日语和韩语共用排序索引，通过二分查找定位精确及前缀匹配区间。
- **保留语言规则**：中文保留双拼转换、反向前缀和简拼匹配；日语、韩语保留罗马字前缀匹配及输出词去重。
- **减少分配**：中文改为流式 Top-30，移除词库大小的临时候选数组；复用查询缓冲区，简拼在编辑器预计算。
- **避免重复计算**：缓存最近一次查询，区分词库、输入、候选上限及匹配模式，并移除匹配热路径日志。
- **提前构建索引**：导入词库、进入 Play 模式和构建世界时，在编辑器生成索引并写入 Udon 数据。

## 使用方式

正常导入、运行和构建即可自动应用优化。旧场景或手动修改的词库，也可通过 `Tools → HXIME → Rebuild All Dictionary Indexes` 重建索引，然后保存场景。单个词库也可以在 `PinyinDict` 检视面板点 **重建查询索引 / Rebuild lookup index** 单独重建：结果与菜单相同，但只处理该组件。

若自定义脚本在运行时修改权重，需调用 `PinyinEngine.InvalidateMatchCache()`；编码和词条结构应在编辑器修改后重建索引。未生成索引的词库保留兼容查询路径。

## 验证与限制

- Unity 2022.3.22f1 / UdonSharp 编译通过。
- 开发阶段通过 1,197 组候选检查及缓存检查，覆盖简繁中文、日语、韩语、同分排序、重复词条和双拼。
- 验证四套词库的索引可写入 Udon 实际使用的数据。
- 上述为编译与正确性验证，未测量 VRChat 客户端的帧耗时改善；短输入仍可能命中较大区间。索引以额外常驻内存换取查询效率。

开发阶段临时使用的测试脚本已在验证完成后清理；随包提供的 `Rebuild All Dictionary Indexes` 与 `Validate Multilingual Labels and Glyphs` 菜单不受影响，仍然可用。

## 相关文档

- 多语言词库配置与文件格式：[MULTILINGUAL.md](MULTILINGUAL.md) ｜ [English](MULTILINGUAL-EN.md) ｜ [日本語](MULTILINGUAL-JP.md) ｜ [한국어](MULTILINGUAL-KO.md)
- 词库来源、处理规则与许可证：[Dicts/SOURCES.md](Dicts/SOURCES.md)
- 安装与常见问题：[README.md](README.md) ｜ [English](README-EN.md) ｜ [日本語](README-JP.md) ｜ [한국어](README-KO.md)
