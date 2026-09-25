#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HX2xianglong90.HXIME.EditorTools
{
    [InitializeOnLoad]
    public static class HXIMEMultilingualValidation
    {
        private const string Root = "Assets/HX2xianglong90/HXIME";
        static HXIMEMultilingualValidation() { EditorApplication.update += Tick; }
        private static void Tick()
        {
            const string request = "Temp/HXIME-ui-validation.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode || File.Exists("Temp/HXIME-font-setup.request")) return;
            File.Delete(request);
            Verify();
        }
        private static void Set(HXIMEUI ui, string name, object value)
        {
            typeof(HXIMEUI).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ui, value);
        }
        [MenuItem("Tools/HXIME/Validate Multilingual Labels and Glyphs")]
        public static void VerifyWithConfirmation()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before validating labels and glyphs.");
            if (!EditorUtility.DisplayDialog("HXIME multilingual validation",
                "This validation runs with Japanese and Korean enabled: it switches the language mode to "
                + "Japanese/Korean in memory and checks dictionary candidates, language button labels and TMP "
                + "glyphs. It does not write back to the prefab and does not change the \"Enable Japanese\" / "
                + "\"Enable Korean\" switches.\n\nContinue?",
                "Validate", "Cancel")) return;
            Verify();
        }

        // Also called by the Temp/HXIME-ui-validation.request step, so it never shows a dialog.
        public static void Verify()
        {
            GameObject prefab = null;
            GameObject probe = null;
            try
            {
                prefab = PrefabUtility.LoadPrefabContents(Root + "/HXIME_Pinyin.prefab");
                HXIMEUI ui = prefab.GetComponentInChildren<HXIMEUI>(true);
                PinyinEngine engine = prefab.GetComponentInChildren<PinyinEngine>(true);
                Transform keyboard = prefab.transform.Find("KeyboardHandle/Keyboard/SpecialButtons");
                Transform bar = prefab.transform.Find("SwitchBarHandle");
                Button[] buttons = keyboard.GetComponentsInChildren<Button>(true);
                Set(ui, "engine", engine);
                Set(ui, "specialButtons", buttons);
                Set(ui, "specialButtonTexts", buttons.Select(b => b.transform.GetChild(0).GetComponent<TMP_Text>()).ToArray());
                Set(ui, "switchBarLangSwitchButton", bar.Find("SwitchBar/LangState").GetComponent<TMP_Text>());
                Set(ui, "IMEInputFieldTexts", new[] { prefab.transform.Find("InputBarHandle/InputBar/InputField/TextArea/Placeholder").GetComponent<TMP_Text>() });
                Set(ui, "skinCenterLabel", bar.Find("SkinCenter/Title").GetComponent<TMP_Text>());
                Set(ui, "settingsPanel", bar.Find("SettingsPanel").gameObject);
                Set(ui, "settingPanelButtonsText", bar.Find("SettingsPanel/Buttons").Cast<Transform>().Select(t => t.Find("Text").GetComponent<TMP_Text>()).ToArray());
                var refresh = typeof(HXIMEUI).GetMethod("RefreshKeyboardLabels", BindingFlags.Instance | BindingFlags.NonPublic);
                string report = "";
                // Include switching back, so stale labels are caught.
                foreach (int mode in new[] { 1, 2, 3, 0, 3, 2, 1 })
                {
                    Set(ui, "langMode", mode);
                    refresh.Invoke(ui, null);
                    string expected = mode == 1 ? "中" : mode == 2 ? "JP" : mode == 3 ? "Ko" : "En";
                    string actual = keyboard.Find("LangChange").GetComponentInChildren<TMP_Text>().text;
                    if (actual != expected) throw new Exception("Language key: " + actual + " expected " + expected);
                    report += "PASS mode " + mode + ": " + actual + "; " + keyboard.Find("PrevPage").GetComponentInChildren<TMP_Text>().text + "\n";
                }
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/Fonts/NotoSansNormal.asset");
                probe = new GameObject("HXIME glyph validation", typeof(RectTransform), typeof(TextMeshPro));
                TextMeshPro text = probe.GetComponent<TextMeshPro>();
                text.font = font;
                text.rectTransform.sizeDelta = new Vector2(2000, 500);
                string sample = "한국어 외국어 웨 일본어 日本語 クリア 確定 이전 다음";
                text.text = sample;
                text.ForceMeshUpdate(true, true);
                for (int i = 0; i < text.textInfo.characterCount; i++)
                {
                    TMP_CharacterInfo c = text.textInfo.characterInfo[i];
                    if (c.textElement == null || c.textElement.unicode != sample[i]) throw new Exception("Missing rendered glyph: " + sample[i]);
                }
                if (text.textInfo.characterCount != sample.Length) throw new Exception("Incomplete text mesh: " + text.textInfo.characterCount + "/" + sample.Length);
                report += "PASS rendered TMP mesh: " + sample + "\n";
                string[] candidates = engine.MatchLanguage(3, "we", 30);
                if (candidates.Length == 0) throw new Exception("No Korean candidates for we");
                foreach (string word in candidates)
                    foreach (char c in word)
                        if (!font.HasCharacter(c, true, false)) throw new Exception("Missing candidate glyph: " + c);
                report += "PASS Korean we candidates: " + string.Join(", ", candidates) + "\n";
                File.WriteAllText("Temp/HXIME-ui-validation.txt", report);
                Debug.Log("HXIME multilingual validation: " + report);
            }
            catch (Exception e)
            {
                File.WriteAllText("Temp/HXIME-ui-validation.txt", "FAIL: " + e);
                Debug.LogException(e);
            }
            finally
            {
                if (probe != null) UnityEngine.Object.DestroyImmediate(probe);
                if (prefab != null) PrefabUtility.UnloadPrefabContents(prefab);
            }
        }
    }
}
#endif
