
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Text;
#endif

namespace HX2xianglong90.HXIME
{
    public class PinyinDict : UdonSharpBehaviour
    {
        public string languageLabel = "IME";
        // 该语言挂载的字典源文件（Dicts/ 目录下的文件名）。编辑器用它执行“加载并应用字典”；
        // 运行时用不到，但必须是普通字段：只在编辑器存在的字段会让 Udon 代理布局与程序不一致。
        [HideInInspector] public string sourceDictionaryName;
        public string[] entries;
        public string[] pinyins;
        public int[] weights;
        public int[] indices;
        // Built in the editor. Orders contain source row IDs, preserving tie order.
        [HideInInspector] public string[] lookupCodes;
        [HideInInspector] public int[] codeOrder;
        [HideInInspector] public string[] lookupInitials;
        [HideInInspector] public int[] initialsOrder;
        [HideInInspector] public int[] wordIds;
        [HideInInspector] public int lookupVersion;
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    // 词库数据的唯一存放点是独立二进制资产（Assets/HXIME_DictionaryData/<对象名>.asset）：
    // 预制件与场景都不保存词条，更不保存索引。两步手动流程（每个语言各做一次）：
    //   1. 加载并应用字典 —— 解析挂载的源文件，把词条写进该资产（lookupVersion = 0，尚无索引）
    //   2. 重建查询索引 —— 把排序索引写进同一份资产
    // 两步都不完成就进 Play 或构建世界会直接失败，详见 Validate()。
    public static class PinyinLookupBuilder
    {
        public const string DataAssetFolder = "Assets/HXIME_DictionaryData";
        private const string SourceFolder = "Dicts";

        // 源文件名 → 英文语言名（用于 Inspector 与构建错误的英文提示）
        private static readonly Dictionary<string, string> LanguageNames = new Dictionary<string, string>
        {
            { "pinyin_simp.dict.yaml.txt", "Simplified Chinese" },
            { "luna_pinyin.dict.yaml.txt", "Traditional Chinese" },
            { "japanese_mozc_common.dict.tsv.txt", "Japanese" },
            { "japanese_mozc.dict.tsv.txt", "Japanese" },
            { "korean_nikl_common.dict.tsv.txt", "Korean" },
            { "korean_nikl.dict.tsv.txt", "Korean" },
        };

        private static string packageRoot;

        public static string PackageRoot(PinyinDict dictionary)
        {
            if (!string.IsNullOrEmpty(packageRoot)) return packageRoot;
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(dictionary));
            packageRoot = Path.GetDirectoryName(Path.GetDirectoryName(scriptPath)).Replace('\\', '/');
            return packageRoot;
        }

        // 挂载源文件名：优先用组件上显式配置的字段；为空时按对象名/语言标签推断，
        // 这样复制、改名出来的对象也能自动挂上对应的源文件。
        public static string SourceFileName(PinyinDict dictionary)
        {
            if (!string.IsNullOrEmpty(dictionary.sourceDictionaryName)) return dictionary.sourceDictionaryName;
            string name = dictionary.name;
            if (name.IndexOf("Simp", StringComparison.OrdinalIgnoreCase) >= 0) return "pinyin_simp.dict.yaml.txt";
            if (name.IndexOf("Trad", StringComparison.OrdinalIgnoreCase) >= 0) return "luna_pinyin.dict.yaml.txt";
            if (string.Equals(dictionary.languageLabel, "Ja", StringComparison.OrdinalIgnoreCase)) return "japanese_mozc_common.dict.tsv.txt";
            if (string.Equals(dictionary.languageLabel, "Ko", StringComparison.OrdinalIgnoreCase)) return "korean_nikl_common.dict.tsv.txt";
            if (name.IndexOf("Japan", StringComparison.OrdinalIgnoreCase) >= 0) return "japanese_mozc_common.dict.tsv.txt";
            if (name.IndexOf("Korea", StringComparison.OrdinalIgnoreCase) >= 0) return "korean_nikl_common.dict.tsv.txt";
            return null;
        }

        public static string SourcePath(PinyinDict dictionary)
        {
            string fileName = SourceFileName(dictionary);
            return string.IsNullOrEmpty(fileName) ? null : PackageRoot(dictionary) + "/" + SourceFolder + "/" + fileName;
        }

        public static string DataAssetPath(PinyinDict dictionary) => DataAssetFolder + "/" + dictionary.name + ".asset";

        public static string LanguageName(PinyinDict dictionary)
        {
            string fileName = SourceFileName(dictionary);
            if (!string.IsNullOrEmpty(fileName) && LanguageNames.TryGetValue(fileName, out string language)) return language;
            return string.IsNullOrEmpty(dictionary.languageLabel) ? dictionary.name : dictionary.languageLabel;
        }

        public static PinyinDictionaryData ResolveData(PinyinDict dictionary)
            => AssetDatabase.LoadAssetAtPath<PinyinDictionaryData>(DataAssetPath(dictionary));

        public static int Count(PinyinDict dictionary)
        {
            PinyinDictionaryData data = ResolveData(dictionary);
            return data != null && data.entries != null ? data.entries.Length : 0;
        }

        // 返回英文错误（Inspector 显示、构建与 Play 拦截都用它）；一切就绪时返回 null。
        public static string Validate(PinyinDict dictionary)
        {
            string language = LanguageName(dictionary);
            PinyinDictionaryData data = ResolveData(dictionary);
            if (data == null)
            {
                string source = SourceFileName(dictionary);
                return "ERROR: HXIME " + language + " dictionary data is not loaded. Click \"Load and apply dictionary\" "
                    + "in the " + dictionary.name + " PinyinDict inspector"
                    + (string.IsNullOrEmpty(source) ? "." : " (source: " + SourceFolder + "/" + source + ").");
            }
            int count = data.entries == null ? 0 : data.entries.Length;
            if (count == 0 || data.pinyins == null || data.weights == null || data.indices == null
                || data.pinyins.Length != count || data.weights.Length != count || data.indices.Length != count)
                return "ERROR: HXIME " + language + " dictionary asset is invalid ("
                    + AssetDatabase.GetAssetPath(data) + "). Click \"Load and apply dictionary\" again.";
            if (data.lookupVersion <= 0 || data.lookupCodes == null || data.codeOrder == null
                || data.lookupInitials == null || data.initialsOrder == null || data.wordIds == null
                || data.lookupCodes.Length != count || data.codeOrder.Length != count
                || data.lookupInitials.Length != count || data.initialsOrder.Length != count
                || data.wordIds.Length != count)
                return "ERROR: HXIME " + language + " lookup index is not built ("
                    + AssetDatabase.GetAssetPath(data) + "). Click \"Rebuild lookup index\" in the "
                    + dictionary.name + " PinyinDict inspector.";
            return null;
        }

        // 第 1 步：解析挂载的源文件，把词条写进独立资产。不建立索引，不改预制件/场景。
        public static void LoadAndApply(PinyinDict dictionary)
        {
            string language = LanguageName(dictionary);
            string sourcePath = SourcePath(dictionary);
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
            {
                Debug.LogError("ERROR: HXIME " + language + " dictionary source file not found: "
                    + (sourcePath ?? "<not mounted>") + ". Set 源文件 / sourceDictionaryName on " + dictionary.name + ".");
                return;
            }
            var total = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                EditorUtility.DisplayProgressBar("HXIME — " + language, "读取源文件…", 0.05f);
                string[] allLines = File.ReadAllLines(sourcePath, Encoding.UTF8);
                EditorUtility.DisplayProgressBar("HXIME — " + language, "解析与校验…", 0.3f);
                var entriesList = new List<string>();
                var pinyinsList = new List<string>();
                var weightsList = new List<int>();
                var indicesList = new List<int>();
                int counter = 0;
                bool inHeader = false;
                for (int lineIndex = 0; lineIndex < allLines.Length; lineIndex++)
                {
                    if (lineIndex % 8192 == 0)
                        EditorUtility.DisplayProgressBar("HXIME — " + language,
                            $"解析与校验: {lineIndex:N0}/{allLines.Length:N0} 行", 0.3f + 0.4f * lineIndex / Math.Max(1, allLines.Length));
                    string line = allLines[lineIndex].TrimStart('\uFEFF');
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed.StartsWith("#")) continue;
                    if (trimmed == "---") { inHeader = true; continue; }
                    if (inHeader)
                    {
                        // Only the standard text/code/weight column order is supported.
                        if (trimmed.StartsWith("columns:") || trimmed.StartsWith("import_tables:"))
                            throw new FormatException("请先将自定义 columns / import_tables 展开为标准 TSV（词条、编码、权重）。");
                        if (trimmed == "...") inHeader = false;
                        continue;
                    }
                    string[] parts = line.Split('\t');
                    if (parts.Length < 2 || parts.Length > 3)
                        throw new FormatException($"第 {lineIndex + 1} 行必须包含 2 或 3 个 Tab 分隔的字段。");
                    string word = parts[0].Trim();
                    string pinyin = parts[1].Trim().ToLowerInvariant();
                    if (word.Length == 0 || pinyin.Length == 0)
                        throw new FormatException($"第 {lineIndex + 1} 行的词条或编码为空。");
                    int weight = 0;
                    if (parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2])
                        && !TryParseWeight(parts[2], out weight))
                        throw new FormatException($"第 {lineIndex + 1} 行权重必须为非负整数、百分比（99.93%）或小数（1.5）。");
                    entriesList.Add(word);
                    pinyinsList.Add(pinyin);
                    weightsList.Add(weight);
                    indicesList.Add(counter);
                    counter++;
                }
                if (inHeader) throw new FormatException("RIME 文件头缺少 ... 结束标记。");
                if (counter == 0) throw new FormatException("字典没有有效词条。");
                EditorUtility.DisplayProgressBar("HXIME — " + language, "写入独立词库资产…", 0.8f);
                WriteDataAsset(dictionary, entriesList.ToArray(), pinyinsList.ToArray(),
                    weightsList.ToArray(), indicesList.ToArray());
                Debug.Log("[HXIME Dictionary] " + language + " 词条已写入 " + DataAssetPath(dictionary)
                    + "（" + counter.ToString("N0") + " 条，" + total.Elapsed.TotalSeconds.ToString("F1") + " 秒，源文件 "
                    + SourceFileName(dictionary) + "）。索引尚未建立，请点“重建查询索引 / Rebuild lookup index”。");
            }
            catch (Exception error)
            {
                Debug.LogError("ERROR: HXIME " + language + " dictionary load failed: " + error.Message);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // 第 2 步：把排序索引写进已经加载好的独立资产。没有词条数据时拒绝执行。
        public static void Rebuild(PinyinDict dictionary)
        {
            string language = LanguageName(dictionary);
            PinyinDictionaryData data = ResolveData(dictionary);
            if (data == null || data.entries == null || data.entries.Length == 0)
            {
                Debug.LogError("ERROR: HXIME " + language + " dictionary data is not loaded. Click "
                    + "\"Load and apply dictionary\" first (expected asset: " + DataAssetPath(dictionary) + ").");
                return;
            }
            BuildIndex(data.entries, data.pinyins, data.weights,
                out data.lookupCodes, out data.lookupInitials, out data.codeOrder,
                out data.initialsOrder, out data.wordIds);
            data.lookupVersion = Math.Max(1, data.lookupVersion + 1);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssetIfDirty(data);
            Debug.Log("[HXIME Dictionary] " + language + " 查询索引已写入 " + DataAssetPath(dictionary)
                + "（lookupVersion " + data.lookupVersion + "）。");
        }

        // 进入 Play 模式或构建世界时把资产里的词条与索引写进 Udon。任何语言未就绪都会直接抛错。
        public static void BakeRuntime(PinyinDict dictionary)
        {
            string error = Validate(dictionary);
            if (error != null) throw new InvalidOperationException(error);
            ResolveData(dictionary).CopyTo(dictionary);
        }

        // 创建或复用同名独立资产，写入词条数组并清空索引（lookupVersion = 0）。
        private static void WriteDataAsset(PinyinDict dictionary, string[] entries, string[] pinyins,
            int[] weights, int[] indices)
        {
            PinyinDictionaryData data = ResolveData(dictionary);
            if (data == null)
            {
                if (!AssetDatabase.IsValidFolder(DataAssetFolder))
                    AssetDatabase.CreateFolder("Assets", "HXIME_DictionaryData");
                data = ScriptableObject.CreateInstance<PinyinDictionaryData>();
                AssetDatabase.CreateAsset(data, DataAssetPath(dictionary));
            }
            data.entries = entries;
            data.pinyins = pinyins;
            data.weights = weights;
            data.indices = indices;
            data.lookupCodes = new string[0];
            data.lookupInitials = new string[0];
            data.codeOrder = new int[0];
            data.initialsOrder = new int[0];
            data.wordIds = new int[0];
            data.lookupVersion = 0;
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssetIfDirty(data);
        }

        // 权重列支持三种写法：非负整数（原样使用）、百分比（99.93%）、小数（1.5）。
        // 百分比与小数按 1/100 精度放大成整数（99.93% -> 9993、0.07% -> 7），保持原有排序关系。
        private static bool TryParseWeight(string text, out int weight)
        {
            weight = 0;
            string value = text.Trim();
            bool percent = value.EndsWith("%", StringComparison.Ordinal);
            if (percent) value = value.Substring(0, value.Length - 1).Trim();
            if (value.Length == 0) return false;
            if (!percent && value.IndexOf('.') < 0)
                return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out weight) && weight >= 0;
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed)
                || double.IsNaN(parsed) || double.IsInfinity(parsed) || parsed < 0) return false;
            double scaled = Math.Round(parsed * 100d, MidpointRounding.AwayFromZero);
            weight = scaled >= int.MaxValue ? int.MaxValue : (int)scaled;
            return true;
        }

        private static void BuildIndex(string[] entries, string[] pinyins, int[] weights,
            out string[] codes, out string[] initials, out int[] order, out int[] initialOrder, out int[] wordIds)
        {
            int count = pinyins == null ? 0 : pinyins.Length;
            if (entries == null || weights == null
                || entries.Length != count || weights.Length != count)
                throw new InvalidOperationException("Dictionary arrays must have equal lengths.");
            codes = new string[count];
            initials = new string[count];
            order = new int[count];
            initialOrder = new int[count];
            wordIds = new int[count];
            var words = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < count; i++)
            {
                codes[i] = (pinyins[i] ?? "").Trim().ToLowerInvariant();
                initials[i] = string.Concat(codes[i].Split(' ').Where(s => s.Length > 0).Select(s => s.Substring(0, 1)));
                order[i] = initialOrder[i] = i;
                string word = entries[i] ?? "";
                if (!words.TryGetValue(word, out int id)) { id = words.Count; words.Add(word, id); }
                wordIds[i] = id;
            }
            var sortCodes = codes;
            var sortInitials = initials;
            Array.Sort(order, (a, b) => { int c = string.CompareOrdinal(sortCodes[a], sortCodes[b]); return c != 0 ? c : a.CompareTo(b); });
            Array.Sort(initialOrder, (a, b) => { int c = string.CompareOrdinal(sortInitials[a], sortInitials[b]); return c != 0 ? c : a.CompareTo(b); });
        }
    }

    [CustomEditor(typeof(PinyinDict))]
    public class PinyinDictEditor : Editor
    {
        private PinyinDict targetScript;
        private EditorApplication.CallbackFunction pendingAction;

        private void OnEnable()
        {
            targetScript = (PinyinDict)target;
        }

        private void OnDisable()
        {
            if (pendingAction != null) EditorApplication.delayCall -= pendingAction;
            pendingAction = null;
        }

        private void QueueEditorAction(Action action)
        {
            if (pendingAction != null) return;
            // Progress bars and asset writes reset the IMGUI layout while UdonSharp still owns an
            // enclosing layout group, so run them after the inspector has unwound.
            pendingAction = () =>
            {
                try
                {
                    if (this != null && targetScript != null) action();
                }
                finally
                {
                    pendingAction = null;
                    if (this != null) Repaint();
                }
            };
            EditorApplication.delayCall += pendingAction;
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("字典状态 / Dictionary status", EditorStyles.boldLabel);
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("languageLabel"),
                new GUIContent("语言按钮名称 / Language label"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sourceDictionaryName"),
                new GUIContent("源文件 / Source file"));
            serializedObject.ApplyModifiedProperties();

            string sourcePath = PinyinLookupBuilder.SourcePath(targetScript);
            EditorGUILayout.LabelField("源文件 / Source",
                string.IsNullOrEmpty(sourcePath) ? "<not mounted>"
                : (File.Exists(sourcePath) ? PinyinLookupBuilder.SourceFileName(targetScript)
                                           : PinyinLookupBuilder.SourceFileName(targetScript) + "  (missing)"));
            EditorGUILayout.LabelField("词条数量 / Entries", PinyinLookupBuilder.Count(targetScript).ToString("N0"));
            PinyinDictionaryData data = PinyinLookupBuilder.ResolveData(targetScript);
            EditorGUILayout.LabelField("索引 / Lookup index",
                data != null && data.lookupVersion > 0 ? "built (v" + data.lookupVersion + ")" : "not built");
            EditorGUILayout.LabelField("独立资产 / Data asset", PinyinLookupBuilder.DataAssetPath(targetScript));

            string error = PinyinLookupBuilder.Validate(targetScript);
            if (error != null)
                EditorGUILayout.HelpBox(error + "\n\nThis language blocks Play mode and world builds until both steps are done.",
                    MessageType.Error);
            else
                EditorGUILayout.HelpBox("Ready. Entries and lookup index live in "
                    + PinyinLookupBuilder.DataAssetPath(targetScript) + " and are baked into Udon on Play / build.",
                    MessageType.Info);

            EditorGUILayout.Space();
            if (GUILayout.Button("加载并应用字典 / Load and apply dictionary"))
                QueueEditorAction(() => PinyinLookupBuilder.LoadAndApply(targetScript));
            if (GUILayout.Button("重建查询索引 / Rebuild lookup index"))
                QueueEditorAction(() => PinyinLookupBuilder.Rebuild(targetScript));
        }
    }
#endif
}
