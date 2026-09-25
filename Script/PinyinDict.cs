
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
#endif

namespace HX2xianglong90.HXIME
{
    public class PinyinDict : UdonSharpBehaviour
    {
        public string languageLabel = "IME";
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
    public static class PinyinLookupBuilder
    {
        public static void Build(PinyinDict dictionary)
        {
            int count = dictionary.pinyins == null ? 0 : dictionary.pinyins.Length;
            if (dictionary.entries == null || dictionary.weights == null
                || dictionary.entries.Length != count || dictionary.weights.Length != count)
                throw new InvalidOperationException("Dictionary arrays must have equal lengths.");
            var codes = new string[count];
            var initials = new string[count];
            var order = new int[count];
            var initialOrder = new int[count];
            var wordIds = new int[count];
            var words = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < count; i++)
            {
                codes[i] = (dictionary.pinyins[i] ?? "").Trim().ToLowerInvariant();
                initials[i] = string.Concat(codes[i].Split(' ').Where(s => s.Length > 0).Select(s => s.Substring(0, 1)));
                order[i] = initialOrder[i] = i;
                string word = dictionary.entries[i] ?? "";
                if (!words.TryGetValue(word, out int id)) { id = words.Count; words.Add(word, id); }
                wordIds[i] = id;
            }
            Array.Sort(order, (a, b) => { int c = string.CompareOrdinal(codes[a], codes[b]); return c != 0 ? c : a.CompareTo(b); });
            Array.Sort(initialOrder, (a, b) => { int c = string.CompareOrdinal(initials[a], initials[b]); return c != 0 ? c : a.CompareTo(b); });
            dictionary.lookupCodes = codes;
            dictionary.codeOrder = order;
            dictionary.lookupInitials = initials;
            dictionary.initialsOrder = initialOrder;
            dictionary.wordIds = wordIds;
            dictionary.lookupVersion = Math.Max(1, dictionary.lookupVersion + 1);
        }
    }

    [CustomEditor(typeof(PinyinDict))]
    public class PinyinDictEditor : Editor
    {
        private PinyinDict targetScript;
        private static string filePath = "";

        private void OnEnable()
        {
            targetScript = (PinyinDict)target;
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            // 显示当前数据统计
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("当前字典数据", EditorStyles.boldLabel);
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("languageLabel"), new GUIContent("语言按钮名称"));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.LabelField($"词条数量: {targetScript.entries?.Length ?? 0}");
            EditorGUILayout.LabelField($"编码数量: {targetScript.pinyins?.Length ?? 0}");
            EditorGUILayout.LabelField($"权重数量: {targetScript.weights?.Length ?? 0}");
            if (GUILayout.Button("重建查询索引 / Rebuild lookup index"))
            {
                Undo.RecordObject(targetScript, "Rebuild HXIME lookup index");
                PinyinLookupBuilder.Build(targetScript);
                UdonSharpEditorUtility.CopyProxyToUdon(targetScript);
                EditorUtility.SetDirty(targetScript);
                PrefabUtility.RecordPrefabInstancePropertyModifications(targetScript);
            }

            // 文件加载区域
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("从文件加载字典", EditorStyles.boldLabel);

            // 文件路径输入
            filePath = EditorGUILayout.TextField("字典文件路径", filePath);
            
            // 浏览文件按钮
            if (GUILayout.Button("浏览文件..."))
            {
                string newPath = EditorUtility.OpenFilePanel("选择 UTF-8 TSV / RIME 字典", "", "");
                if (!string.IsNullOrEmpty(newPath))
                {
                    filePath = newPath;
                }
            }

            // 加载并应用按钮
            if (GUILayout.Button("加载并应用字典"))
            {
                if (File.Exists(filePath))
                {
                    LoadAndApplyDictionary(filePath);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "文件不存在！", "确定");
                }
            }
        }

        private void LoadAndApplyDictionary(string path)
        {
            try
            {
                string[] allLines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
                var entriesList = new List<string>();
                var pinyinsList = new List<string>();
                var weightsList = new List<int>();
                var indicesList = new List<int>();
                int counter =0;

                bool inHeader = false;
                for (int lineIndex = 0; lineIndex < allLines.Length; lineIndex++)
                {
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
                    
                    // 处理权重 - 如果没有权重列，默认为0
                    int weight = 0;
                    if (parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2]))
                    {
                        if (!int.TryParse(parts[2].Trim(), out weight) || weight < 0)
                            throw new FormatException($"第 {lineIndex + 1} 行权重必须为非负整数。");
                    }

                    entriesList.Add(word);
                    pinyinsList.Add(pinyin);
                    weightsList.Add(weight);
                    indicesList.Add(counter);
                    counter++;
                }
                if (inHeader) throw new FormatException("RIME 文件头缺少 ... 结束标记。");
                if (counter == 0) throw new FormatException("字典没有有效词条，原词库保持不变。");

                // 直接应用到目标组件
                Undo.RecordObject(targetScript, "Import HXIME dictionary");
                targetScript.entries = entriesList.ToArray();
                targetScript.weights = weightsList.ToArray();
                targetScript.pinyins = pinyinsList.ToArray();
                targetScript.indices = indicesList.ToArray();

                PinyinLookupBuilder.Build(targetScript);
                UdonSharpEditorUtility.CopyProxyToUdon(targetScript);

                EditorUtility.SetDirty(targetScript);
                PrefabUtility.RecordPrefabInstancePropertyModifications(targetScript);
                EditorUtility.DisplayDialog("成功", 
                    $"字典加载并应用成功！\n词条数量: {entriesList.Count}", 
                    "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"加载字典失败: {e.Message}", "确定");
            }
        }
    }
#endif
}
