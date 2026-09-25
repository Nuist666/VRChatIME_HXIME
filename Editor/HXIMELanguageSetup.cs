#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UdonSharpEditor;

namespace HX2xianglong90.HXIME.EditorTools
{
    // Editor-only setup: dictionaries are baked into Udon, never parsed at runtime.
    [InitializeOnLoad]
    public static class HXIMELanguageSetup
    {
        private const string Root = "Assets/HX2xianglong90/HXIME";
        private const string Request = "Temp/HXIME-language-setup.request";
        private const string ReportPath = "Temp/HXIME-language-setup.json";
        private static readonly double ReadyAt = EditorApplication.timeSinceStartup + 5;

        [Serializable] private class Report
        {
            public bool success;
            public string error;
            public List<string> configured = new List<string>();
            public List<string> verified = new List<string>();
        }

        static HXIMELanguageSetup() { EditorApplication.update += ProcessRequest; }

        // A one-shot local request is used by maintenance tooling. Normal imports do nothing.
        private static void ProcessRequest()
        {
            if (!File.Exists(Request) || EditorApplication.timeSinceStartup < ReadyAt
                || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling
                || EditorApplication.isUpdating) return;
            File.Delete(Request);
            Configure();
        }

        [MenuItem("Tools/HXIME/Configure Japanese and Korean Dictionaries")]
        public static void ConfigureWithConfirmation()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before configuring dictionaries.");
            if (!EditorUtility.DisplayDialog("HXIME Japanese / Korean dictionaries",
                "This enables the Japanese and Korean dictionaries: the prefab (and the scene instances loaded "
                + "in the editor) get their Japanese/Korean dictionaries created or reconnected, and the "
                + "\"Enable Japanese\" / \"Enable Korean\" switches on HXIMEUI are turned on.\n\nContinue?",
                "Enable", "Cancel")) return;
            Configure();
        }

        // Also called by the Temp/HXIME-language-setup.request step, so it never shows a dialog.
        public static void Configure()
        {
            var report = new Report();
            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    throw new InvalidOperationException("Exit Play mode before configuring dictionaries.");
                string prefabPath = Root + "/HXIME_Pinyin.prefab";
                // Validate the complete input before modifying any prefab or scene.
                string[][] ja = ReadRows(Root + "/Dicts/japanese_mozc_common.dict.tsv.txt");
                string[][] ko = ReadRows(Root + "/Dicts/korean_nikl_common.dict.tsv.txt");
                GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    foreach (PinyinEngine engine in prefab.GetComponentsInChildren<PinyinEngine>(true))
                        ConfigureEngine(engine, ja, ko, false, report);
                    foreach (HXIMEUI ui in prefab.GetComponentsInChildren<HXIMEUI>(true))
                        EnableLanguageToggles(ui);
                    PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                }
                finally
                {
                    // The SDK queues component setup callbacks using delayCall.
                    // Unload after those callbacks, so they never touch destroyed behaviours.
                    EditorApplication.delayCall += () =>
                    {
                        if (prefab != null) PrefabUtility.UnloadPrefabContents(prefab);
                    };
                }

                // Also repair instances with an explicit empty-array override, and unpacked instances.
                foreach (PinyinEngine engine in UnityEngine.Object.FindObjectsOfType<PinyinEngine>(true))
                {
                    if (EditorUtility.IsPersistent(engine) || !engine.gameObject.scene.IsValid()
                        || !engine.gameObject.scene.isLoaded || EditorSceneManager.IsPreviewScene(engine.gameObject.scene)) continue;
                    ConfigureEngine(engine, ja, ko, true, report);
                    EditorSceneManager.MarkSceneDirty(engine.gameObject.scene);
                }
                AssetDatabase.SaveAssetIfDirty(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));

                // The confirmation dialog promises to enable both languages, so open the switches
                // on scene instances as well; connected dictionaries alone would stay disabled.
                foreach (HXIMEUI ui in UnityEngine.Object.FindObjectsOfType<HXIMEUI>(true))
                {
                    if (EditorUtility.IsPersistent(ui) || !ui.gameObject.scene.IsValid()
                        || !ui.gameObject.scene.isLoaded || EditorSceneManager.IsPreviewScene(ui.gameObject.scene)) continue;
                    EnableLanguageToggles(ui);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(ui);
                    EditorUtility.SetDirty(ui);
                    EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
                }
                report.success = true;
                Debug.Log("HXIME: Japanese/Korean dictionaries configured. Save the scene to keep instance overrides.");
            }
            catch (Exception exception)
            {
                report.error = exception.ToString();
                Debug.LogException(exception);
            }
            Directory.CreateDirectory("Temp");
            File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
        }

        // The confirmation dialog promises to enable both languages, so open the switches as well:
        // a connected dictionary with the switch off would still be skipped at runtime.
        private static void EnableLanguageToggles(HXIMEUI ui)
        {
            typeof(HXIMEUI).GetField("enableJapanese", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ui, true);
            typeof(HXIMEUI).GetField("enableKorean", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ui, true);
            UdonSharpEditorUtility.CopyProxyToUdon(ui);
        }

        private static string[][] ReadRows(string path)
        {
            var result = new List<string[]>();
            foreach (string line in File.ReadAllLines(path, System.Text.Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                string[] columns = line.Split('\t');
                if (columns.Length != 3 || string.IsNullOrWhiteSpace(columns[0])
                    || string.IsNullOrWhiteSpace(columns[1]) || !int.TryParse(columns[2], out int weight) || weight < 0)
                    throw new FormatException("Invalid dictionary row in " + path);
                result.Add(columns);
            }
            if (result.Count == 0) throw new FormatException("Empty dictionary: " + path);
            return result.ToArray();
        }

        private static PinyinDict EnsureDictionary(PinyinEngine engine, string name, string label, string[][] rows, bool undo)
        {
            if (engine.additionalDicts != null)
                foreach (PinyinDict existing in engine.additionalDicts)
                    if (existing != null && existing.languageLabel == label && existing.entries != null
                        && existing.entries.Length > 0)
                    {
                        if (undo) Undo.RecordObject(existing, "Rebuild HXIME lookup index");
                        PinyinLookupBuilder.Build(existing);
                        UdonSharpEditorUtility.CopyProxyToUdon(existing);
                        EditorUtility.SetDirty(existing);
                        if (undo) PrefabUtility.RecordPrefabInstancePropertyModifications(existing);
                        return existing;
                    }
            Transform child = engine.transform.Find(name);
            if (child == null)
            {
                var obj = new GameObject(name);
                obj.transform.SetParent(engine.transform, false);
                if (undo) Undo.RegisterCreatedObjectUndo(obj, "Add HXIME language dictionary");
                child = obj.transform;
            }
            PinyinDict dictionary = child.GetComponent<PinyinDict>();
            if (dictionary == null) dictionary = child.gameObject.AddUdonSharpComponent<PinyinDict>();
            if (undo) Undo.RecordObject(dictionary, "Configure HXIME dictionary");
            dictionary.languageLabel = label;
            dictionary.entries = new string[rows.Length];
            dictionary.pinyins = new string[rows.Length];
            dictionary.weights = new int[rows.Length];
            dictionary.indices = new int[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                dictionary.entries[i] = rows[i][0];
                dictionary.pinyins[i] = rows[i][1];
                dictionary.weights[i] = int.Parse(rows[i][2]);
                dictionary.indices[i] = i;
            }
            PinyinLookupBuilder.Build(dictionary);
            UdonSharpEditorUtility.CopyProxyToUdon(dictionary);
            EditorUtility.SetDirty(dictionary);
            if (undo) PrefabUtility.RecordPrefabInstancePropertyModifications(dictionary);
            return dictionary;
        }

        private static void ConfigureEngine(PinyinEngine engine, string[][] ja, string[][] ko, bool undo, Report report)
        {
            PinyinDict japanese = EnsureDictionary(engine, "JapaneseDictionary", "Ja", ja, undo);
            PinyinDict korean = EnsureDictionary(engine, "KoreanDictionary", "Ko", ko, undo);
            var dictionaries = new List<PinyinDict>(engine.additionalDicts ?? new PinyinDict[0]);
            if (!dictionaries.Contains(japanese)) dictionaries.Add(japanese);
            if (!dictionaries.Contains(korean)) dictionaries.Add(korean);
            if (undo) Undo.RecordObject(engine, "Enable HXIME languages");
            engine.additionalDicts = dictionaries.ToArray();
            UdonSharpEditorUtility.CopyProxyToUdon(engine);
            EditorUtility.SetDirty(engine);
            if (undo) PrefabUtility.RecordPrefabInstancePropertyModifications(engine);
            int jaMode = dictionaries.IndexOf(japanese) + 2;
            int koMode = dictionaries.IndexOf(korean) + 2;
            if (Array.IndexOf(engine.MatchLanguage(jaMode, "nihongo", 30), "日本語") < 0
                || Array.IndexOf(engine.MatchLanguage(koMode, "hangugeo", 30), "한국어") < 0)
                throw new InvalidOperationException("Configured dictionary lookup failed.");
            var labels = new List<string>();
            int mode = 1;
            for (int i = 0; i < dictionaries.Count + 2; i++)
            {
                labels.Add(engine.LanguageLabel(mode));
                mode = engine.NextLanguage(mode);
            }
            report.configured.Add(engine.gameObject.scene.path + ": " + engine.name);
            report.verified.Add(string.Join(" -> ", labels) + "; Ja=" + japanese.entries.Length + "; Ko=" + korean.entries.Length);
        }
    }
}
#endif
