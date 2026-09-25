#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HX2xianglong90.HXIME.EditorTools
{
    [InitializeOnLoad]
    public static class HXIMEInputSetup
    {
        static HXIMEInputSetup() { EditorApplication.update += Tick; }
        private static void Tick()
        {
            const string request = "Temp/HXIME-input-repair.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(request);
            RepairAndVerify();
        }

        [MenuItem("Tools/HXIME/Repair Output Field and Verify Chinese Selection")]
        public static void RepairAndVerify()
        {
            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Exit Play mode first.");
                string report = "";
                foreach (HXIMEUI ui in UnityEngine.Object.FindObjectsOfType<HXIMEUI>(true))
                {
                    var scene = ui.gameObject.scene;
                    if (EditorUtility.IsPersistent(ui) || !scene.IsValid() || !scene.isLoaded || EditorSceneManager.IsPreviewScene(scene)) continue;
                    Transform source = ui.transform.Find("InputBarHandle/InputBar/InputField");
                    if (source == null) continue;
                    var composition = source.GetComponent<TMP_InputField>();
                    if (ui.targetInputfield != composition)
                    {
                        // Normalize the placeholder on a field created by an older setup run.
                        if (ui.targetInputfield != null && ui.targetInputfield.name == "OutputField"
                            && ui.targetInputfield.placeholder is TMP_Text existing && existing.text == "输出结果 / Output")
                        {
                            existing.text = "Output";
                            RectTransform sourceRect = composition.GetComponent<RectTransform>();
                            RectTransform outputRect = ui.targetInputfield.GetComponent<RectTransform>();
                            outputRect.anchoredPosition = sourceRect.anchoredPosition - new Vector2(0, sourceRect.rect.height * 2 + 10);
                            EditorUtility.SetDirty(outputRect);
                            EditorUtility.SetDirty(existing);
                            EditorSceneManager.MarkSceneDirty(scene);
                            if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Unable to save output placeholder.");
                        }
                        continue;
                    }
                    string backup = "Temp/HXIME-before-output-repair-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity";
                    if (!EditorSceneManager.SaveScene(scene, backup, true)) throw new Exception("Unable to back up scene.");
                    Undo.RecordObject(ui, "Repair HXIME output binding");
                    TMP_InputField output = CreateOutput(composition);
                    Undo.RegisterCreatedObjectUndo(output.gameObject, "Create HXIME output field");
                    ui.targetInputfield = output;
                    UdonSharpEditorUtility.CopyProxyToUdon(ui);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(ui);
                    EditorUtility.SetDirty(ui);
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Unable to save repaired scene.");
                    report += "PASS repaired " + scene.path + "; backup: " + backup + "\n";
                }
                report += VerifySelection();
                File.WriteAllText("Temp/HXIME-input-repair.txt", report);
                Debug.Log("HXIME input: " + report);
            }
            catch (Exception e)
            {
                File.WriteAllText("Temp/HXIME-input-repair.txt", "FAIL: " + e);
                Debug.LogException(e);
            }
        }

        private static TMP_InputField CreateOutput(TMP_InputField composition)
        {
            GameObject go = UnityEngine.Object.Instantiate(composition.gameObject, composition.transform.parent);
            go.name = "OutputField";
            TMP_InputField output = go.GetComponent<TMP_InputField>();
            output.onValueChanged = new TMP_InputField.OnChangeEvent();
            output.SetTextWithoutNotify("");
            var placeholder = output.placeholder as TMP_Text;
            if (placeholder != null) placeholder.text = "Output";
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition -= new Vector2(0, rect.rect.height * 2 + 10);
            Image background = go.AddComponent<Image>();
            background.color = new Color(1, 1, 1, 0.9f);
            output.targetGraphic = background;
            return output;
        }

        private static void Set(HXIMEUI ui, string name, object value)
        {
            typeof(HXIMEUI).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ui, value);
        }
        private static string VerifySelection()
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents("Assets/HX2xianglong90/HXIME/HXIME_Pinyin.prefab");
            try
            {
                HXIMEUI ui = prefab.GetComponentInChildren<HXIMEUI>(true);
                PinyinEngine engine = prefab.GetComponentInChildren<PinyinEngine>(true);
                engine.SwitchSimp();
                TMP_InputField composition = prefab.transform.Find("InputBarHandle/InputBar/InputField").GetComponent<TMP_InputField>();
                TMP_InputField output = CreateOutput(composition);
                Button[] buttons = prefab.transform.Find("InputBarHandle/InputBar/Candidates").GetComponentsInChildren<Button>(true);
                TMP_Text[] labels = buttons.Select(b => b.transform.GetChild(0).GetComponent<TMP_Text>()).ToArray();
                Set(ui, "IMEInputField", composition);
                Set(ui, "wordCandidateButtons", buttons);
                Set(ui, "wordCandidateButtonTexts", labels);
                Set(ui, "langMode", 1);
                Set(ui, "ulpb", false);
                Set(ui, "engine", engine);
                ui.targetInputfield = output;
                composition.onValueChanged = new TMP_InputField.OnChangeEvent();
                composition.onValueChanged.AddListener(_ => ui.GetWordCandidates());
                // Simulate an output callback that refreshes candidate labels synchronously.
                output.onValueChanged.AddListener(_ => { labels[0].text = "侧"; ui.GetWordCandidates(); });
                string report = "";
                foreach (string input in new[] { "ce s", "ces", "ce shi", "c s", "  ce  s  ", "ce s hou" })
                {
                    output.SetTextWithoutNotify("");
                    composition.text = input;
                    if (input == "ce s" && !engine.Match(input, 30, false, false).Contains("测试")) throw new Exception("ces candidate missing");
                    // Exercise selection independently of ranking and page location.
                    Set(ui, "wordCandidates", new[] { "测试" });
                    Set(ui, "currentPageIndex", 0);
                    labels[0].text = "测试";
                    ui.WordCandidatePressedAgent0();
                    string remaining = input == "ce s hou" ? "hou" : "";
                    if (output.text != "测试" || composition.text != remaining)
                        throw new Exception(input + " produced output=" + output.text + ", composition=" + composition.text);
                    report += "PASS " + input + " -> 测试; remaining=" + remaining + "\n";
                }
                ui.targetInputfield = composition;
                composition.text = "ce s";
                Set(ui, "wordCandidates", new[] { "测试" });
                ui.WordCandidatePressedAgent0();
                if (composition.text != "ce s") throw new Exception("Self-bound field corrupted input");
                report += "PASS self-bound output rejected without changing composition\n";
                return report;
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
        }
    }
}
#endif
