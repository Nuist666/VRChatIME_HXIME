# HXIME：新建 TextMeshPro Input Field 显示方框的解决方案

适用环境：Unity 2022.3、TextMesh Pro 3.0.x、HXIME 多语言导入包。

## 1. 先确认问题所在

**绑定 `HXIMEUI → Target Inputfield` 只指定文字输出位置，不会复制字体或材质。** 新建的 TMP 输入框可能仍使用默认字体，其字形不覆盖中文、日文或韩文。HXIME 的候选栏字体与新建输出框的字体是独立配置的。

这是本次现象的优先排查方向；未检查你新建输入框的实际 Font Asset 和 Console 日志前，不能把所有方框都归为同一种原因。

| 现象 | 优先检查 |
| --- | --- |
| 英文、数字正常，中文或日韩文是方框 | 主字体缺字，或没有连接覆盖该语言的后备字体 |
| 候选栏正常，只有新建输出框是方框 | 新输出框的 Font Asset、Text 子对象和 Placeholder |
| 只有占位提示是方框，输入内容正常 | Placeholder 的字体单独配置错误 |
| 连 `ABC123` 都是方框或实心矩形 | Font Asset/Atlas 引用丢失、字体与材质不配套；检查 Console |
| 编辑器正常，重新打开或进入 VRChat 后缺字 | 字形可能只在编辑器动态生成；检查保存的图集和构建后的显示 |

## 2. 推荐修复：直接使用 HXIME 的字体

先退出 Play 模式，再修改场景，避免退出 Play 后配置丢失。

1. 在 Hierarchy 中选中**新建的目标输入框根对象**，找到 `TMP Input Field` 组件。
2. 将该组件的 **Font Asset** 设置为：

   `Assets/HX2xianglong90/HXIME/Fonts/NotoSansNormal.asset`

   请拖入 TMP 字体 `.asset`，不是 `.ttf`、`.otf` 或材质。TMP 3.0.6 的这个字段会同时设置输入文字和占位提示的字体。

3. 展开输入框，检查以下子对象的 `TextMeshPro - Text (UI)` 组件：

   ```text
   新建的 TMP Input Field
   └─ Text Area（实际名称可能不同）
      ├─ Text          ← 真正显示输入内容
      └─ Placeholder   ← 空输入框的提示文字
   ```

   确认 **Text 和 Placeholder 的 Font Asset 都已更新**。若根组件没有显示字体字段，可以分别修改这两个子对象。

4. 检查子对象的 **Material Preset**，使用所选字体对应的默认材质。不要继续套用另一套字体（例如原默认字体）的材质。先确认普通文字能正常显示，再恢复自定义透明、描边等效果。
5. 选中 `NotoSansNormal.asset`，展开 **Fallback Font Assets / Fallback Font Asset Table**，确认包含：

   `Assets/HX2xianglong90/HXIME/Fonts/NotoSansMultilingualFallback.asset`

6. 将新输入框根对象上的 **TMP_InputField 组件**绑定到 `HXIMEUI → Target Inputfield`。不要绑定 Text 子对象，也不要绑定输入法自己的 `InputBarHandle/InputBar/InputField`，后者是拼音预编辑框。
7. 保存场景；若修改的是预制体实例且需要复用，再按项目需要保存相应预制体覆盖。

**不要只改 Project Settings 中的默认字体。** 现有输入框上的字体引用应直接检查和修改。

## 3. 保留新输入框原字体：添加后备字体

如果希望保留原来的英文字体外观，可以使用后备字体链：

```text
新输入框的原主字体
└─ NotoSansNormal
   └─ NotoSansMultilingualFallback
```

1. 选中新输入框正在使用的 TMP Font Asset。
2. 在其 Fallback Font Asset Table 中加入 `NotoSansNormal.asset`。
3. 确认 `NotoSansNormal` 的后备字体列表中已有 `NotoSansMultilingualFallback.asset`。
4. Placeholder 若使用另一套字体，也要单独配置或与 Text 统一。

修改字体资源会影响所有引用该字体的文字。如果只想改变当前输入框，可以先在 `Assets` 下复制一份主字体资源，并让当前输入框使用这份副本。不要创建互相引用的后备字体循环。

## 4. 重导入后仍然是方框

### 检查字体是否完整导入

在 Project 中确认以下资源存在且没有 Missing 引用：

- `Fonts/NotoSansNormal.asset`
- `Fonts/NotoSansMultilingualFallback.asset`
- `Fonts/NotoSansSC-VariableFont_wght.ttf`
- `Fonts/NotoSansCJKkr-Regular.otf`

`NotoSansMultilingualFallback` 是预生成的静态 TMP 字体，当前包含 **13,878 个字符和两张 4096×4096 图集**。图集和材质作为该 `.asset` 的子资源保存，不一定有独立 PNG 文件。展开资源，检查 Atlas 和 Material 引用；不要把“没有外部 PNG”误判为缺文件。

若曾部分导入，只选择了脚本或词库，请重新导入包中的完整 Fonts 内容及相关依赖。不要删除或重新生成现有 `.meta` 来尝试修复字体，否则可能破坏场景和字体间的 GUID 引用。

### 检查项目字体的后备连接

菜单：

`Tools → HXIME → Bake Japanese and Korean Font Fallback`

该工具在字体不存在时生成静态图集，并为 **HXIME/Fonts 内的 TMP 字体**连接后备字体。已有图集会被检查和复用，普通导入无需重新烘焙。运行结果（PASS/FAIL 与字符数、图集数）写入工程的编辑器临时目录（`Temp/`）。

**它不会自动修改你在其他目录创建的字体，也不会替你设置新输入框的 Font Asset。** 新输入框仍需完成第 2 或第 3 节。若报告现有静态字体缺字，不要反复运行期待它自动扩容，应按第 5 节补充字形。

### 阅读 Console 的具体提示

例如：

```text
The character with Unicode value ... was not found in the [...] font asset
or any potential fallbacks. It was replaced by Unicode character ...
```

其中的字体名称和文字对象名称，可以定位**实际缺字的组件**。如果日志仍指出默认字体，说明仍有 Text 或 Placeholder 没有换好字体。

如果连英文字母也显示异常，检查对应 TMP Font Asset 的 Atlas Texture 是否为空、Material Preset 是否配套，以及材质的字体图集纹理是否指向该字体的 Atlas。不要仅靠增加字号或更换文字颜色解决缺字问题。

## 5. 仍有少数字是方框：补充静态字形

当前多语言后备字体覆盖全部 **11,172 个现代韩文音节**、韩文字母，以及默认日、韩精简词库和本地化界面的字符。**它不是完整的中日韩 Unicode 字库**，不能保证所有中文生僻字、繁体字、罕见日文汉字或 Emoji 都存在。原有中文主字体也不应被视为全部中文字形的静态全集。

如果只缺特定字符：

1. 记录 Console 报告的缺字，准备 UTF-8 文本文件，包含实际需要显示的文字；词库中的编码和词条并不能代替字体字形。
2. 打开 `Window → TextMeshPro → Font Asset Creator`（不同版本菜单名称可能略有区别）。
3. 选择确实包含这些字符的源字体。包内的 Noto Sans SC 或 Noto Sans CJK KR 可作为起点，但仍要检查生成结果中的 Missing Characters。
4. 在字符集选项中使用 **Characters from File**，选择准备好的文本；也可对少量缺字使用 Custom Characters。
5. 生成 SDF 图集并保存为新的 TMP Font Asset，确认所需字符没有缺失。若字符过多、图集无法容纳，应拆分字体或使用支持多图集的生成流程，不要忽略未打包字符。
6. 将新字体作为额外 fallback 加到实际使用的主字体上，并确认所需字形已保存。面向 VRChat 发布时，优先使用已烘焙且可在目标平台验证的静态图集。

仅把 Atlas Population Mode 改成 Dynamic 并不保证源字体包含缺字，也不能代替构建后的验证；不要将“编辑器里偶然显示过”当成字形已经随世界正确发布。

## 6. 验收步骤

1. 在新输出框中直接输入以下文字，先排除词库和输入逻辑的影响：

   ```text
   ABC123 测试 日本語 こんにちは 한국어 외국어
   ```

2. 分别检查 Text 的输入内容和 Placeholder 的提示文字，不再出现方框。
3. 再通过 HXIME 选词测试：中文 `ce s` → `测试`，日语 `nihongo` → `日本語`，韩语 `hangugeo` → `한국어`。
4. 切换英语 `En`，整个 `InputBarHandle` 会隐藏。若需要英文模式下仍显示输出框，应将目标输入框放在该层级之外，再验证英文输入。
5. 保存后重新打开场景，再进行目标平台的 VRChat Build & Test，确认图集和字体引用在构建后仍正常。

可额外执行 `Tools → HXIME → Validate Multilingual Labels and Glyphs`。该菜单检查 HXIME 自带字体与测试文字，**通过不代表新建外部输入框已配置正确**，仍需完成上面的直接输入验证。它同样只在编辑模式运行、不进入 Play 模式，也不修改预制件。

## 配置依据

本说明依据项目内 `Fonts/MULTILINGUAL-FONT.md`、静态字体资源、`Editor/HXIMEFontSetup.cs`，以及当前 Unity 项目安装的 TMP 3.0.6 源码核对。TMP 的 `TMP_InputField.SetGlobalFontAsset` 同时更新 Text 和 Placeholder；HXIME 的目标输入框绑定本身不设置字体。
