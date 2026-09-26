#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UdonSharpEditor;

namespace HX2xianglong90.HXIME.EditorTools
{
    // 语言相关的编辑器设置：Ja/Ko 语言按钮开关，以及维护/自动化用的“按语言加载 + 建索引 + 逐语言验证”。
    // 正常运行不需要它：用户是按 PinyinDict 检视面板的语言逐个手动完成的。
    [InitializeOnLoad]
    public static class HXIMELanguageSetup
    {
        private const string Root = "Assets/HX2xianglong90/HXIME";
        private const string Request = "Temp/HXIME-dictionary-setup.request";
        private const string ReportPath = "Temp/HXIME-dictionary-setup.txt";
        private static readonly double ReadyAt = EditorApplication.timeSinceStartup + 5;
        private static bool busy;

        static HXIMELanguageSetup() { EditorApplication.update += ProcessRequest; }

        private static void ProcessRequest()
        {
            if (busy || !File.Exists(Request) || EditorApplication.timeSinceStartup < ReadyAt
                || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling
                || EditorApplication.isUpdating) return;
            File.Delete(Request);
            busy = true;
            try { SetupAll(); }
            finally { busy = false; }
        }

        // 每个语言各自加载并建立索引，然后逐语言报告状态与一次查询冒烟测试。
        public static void SetupAll()
        {
            foreach (string source in HXIMELookupSetup.AllSources)
            {
                HXIMELookupSetup.LoadSource(source);
                HXIMELookupSetup.RebuildSource(source);
            }
            SetLanguageSwitch("Japanese", true);
            SetLanguageSwitch("Korean", true);
            File.WriteAllText(ReportPath, Verify());
            string[] sceneErrors = HXIMELookupSetup.CollectErrors();
            File.AppendAllText(ReportPath, sceneErrors.Length == 0
                ? "\nScene validation: PASS (Play mode and world builds are allowed)\n"
                : "\nScene validation: FAIL (Play mode is cancelled, world builds fail)\n"
                  + string.Join("\n", sceneErrors) + "\n");
            Debug.Log("HXIME dictionary setup finished:\n" + File.ReadAllText(ReportPath));
        }

        // 逐语言验证：词条与索引状态 + 该语言的一次真实查询。
        // 预制件内容只在内存里烘焙，不写回文件。
        public static string Verify()
        {
            var report = new StringBuilder();
            GameObject prefab = null;
            try
            {
                prefab = PrefabUtility.LoadPrefabContents(Root + "/HXIME_Pinyin.prefab");
                PinyinEngine engine = prefab.GetComponentInChildren<PinyinEngine>(true);
                foreach (PinyinDict dictionary in prefab.GetComponentsInChildren<PinyinDict>(true))
                {
                    string error = PinyinLookupBuilder.Validate(dictionary);
                    report.AppendLine((error == null ? "PASS " : "FAIL ")
                        + PinyinLookupBuilder.LanguageName(dictionary) + " (" + dictionary.name + "): entries="
                        + PinyinLookupBuilder.Count(dictionary).ToString("N0") + ", index="
                        + (error == null ? "built" : "missing") + (error == null ? "" : " — " + error));
                    if (error == null)
                    {
                        PinyinLookupBuilder.BakeRuntime(dictionary);
                        UdonSharpEditorUtility.CopyProxyToUdon(dictionary);
                    }
                }
                foreach (var probe in new[]
                {
                    new { Mode = 1, Input = "nihao", Language = "Chinese" },
                    new { Mode = 2, Input = "nihongo", Language = "Japanese" },
                    new { Mode = 3, Input = "hangugeo", Language = "Korean" },
                })
                {
                    string[] candidates;
                    try
                    {
                        // 中文走 Match()（MatchLanguage 只服务 additionalDicts 里的日/韩）。
                        candidates = probe.Mode == 1
                            ? engine.Match(probe.Input, 10)
                            : engine.MatchLanguage(probe.Mode, probe.Input, 10);
                    }
                    catch (Exception error)
                    {
                        report.AppendLine("FAIL " + probe.Language + " probe '" + probe.Input + "': " + error.Message);
                        continue;
                    }
                    report.AppendLine((candidates.Length > 0 ? "PASS " : "FAIL ") + probe.Language + " probe '"
                        + probe.Input + "': " + (candidates.Length > 0 ? string.Join(", ", candidates) : "<no candidates>"));
                }
            }
            catch (Exception error)
            {
                report.AppendLine("FAIL " + error);
            }
            finally
            {
                // 只卸载内存里的副本，绝不保存：预制件保持“空词库”的出厂状态。
                if (prefab != null) PrefabUtility.UnloadPrefabContents(prefab);
            }
            return report.ToString();
        }

        // 打开/关闭某个语言按钮（Ja/Ko）。预制件与场景实例一起设置。
        public static void SetLanguageSwitch(string language, bool value)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("HXIME: exit Play mode before changing language switches.");
                return;
            }
            string field = language == "Japanese" ? "enableJapanese" : language == "Korean" ? "enableKorean" : null;
            if (field == null) throw new ArgumentException("Unsupported language: " + language);
            string prefabPath = Root + "/HXIME_Pinyin.prefab";
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                foreach (HXIMEUI ui in prefab.GetComponentsInChildren<HXIMEUI>(true)) SetSwitch(ui, field, value);
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            }
            finally
            {
                // The SDK queues component setup callbacks with delayCall; unload after those.
                EditorApplication.delayCall += () =>
                {
                    if (prefab != null) PrefabUtility.UnloadPrefabContents(prefab);
                };
            }
            foreach (HXIMEUI ui in UnityEngine.Object.FindObjectsOfType<HXIMEUI>(true))
            {
                if (EditorUtility.IsPersistent(ui) || !ui.gameObject.scene.IsValid()
                    || !ui.gameObject.scene.isLoaded || EditorSceneManager.IsPreviewScene(ui.gameObject.scene)) continue;
                SetSwitch(ui, field, value);
                PrefabUtility.RecordPrefabInstancePropertyModifications(ui);
                EditorUtility.SetDirty(ui);
                EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
            }
            AssetDatabase.SaveAssetIfDirty(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
            Debug.Log("HXIME: " + language + " language button " + (value ? "enabled" : "disabled")
                + ". Save open scenes to keep instance overrides.");
        }

        private static void SetSwitch(HXIMEUI ui, string field, bool value)
        {
            typeof(HXIMEUI).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ui, value);
            UdonSharpEditorUtility.CopyProxyToUdon(ui);
            EditorUtility.SetDirty(ui);
        }
    }
}
#endif
