# 日语、韩语字形支持

源字体：Noto Sans CJK KR Regular，Noto CJK 项目。

- 固定版本：`f8d157532fbfaeda587e826d4cd5b21a49186f7c`
- 来源：https://github.com/notofonts/noto-cjk/blob/f8d157532fbfaeda587e826d4cd5b21a49186f7c/Sans/OTF/Korean/NotoSansCJKkr-Regular.otf
- 授权：SIL Open Font License 1.1，全文见 `NotoSansCJK-LICENSE.txt`。

`NotoSansMultilingualFallback.asset` 在 Unity 编辑器用 TextMesh Pro 生成，40 点、4 像素 padding、SDFAA、4096×4096 多图集，最终设为 Static。包含全部 11,172 个现代韩文音节、韩文字母，以及默认日、韩精简词库和本地化界面中的字符。运行时无需构建这些字形。

项目内的 TMP 字体通过 fallback 引用该资源，保留原有主字体和材质。外部目标输入框若使用其他字体，也需添加此 fallback。替换大词库或输入罕见字时需自行补充字形。
