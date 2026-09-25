#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace HX2xianglong90.HXIME.EditorTools
{
    // Bake once in the editor. VRChat clients only use the saved static atlas.
    [InitializeOnLoad]
    public static class HXIMEFontSetup
    {
        private const string Root = "Assets/HX2xianglong90/HXIME";
        private const string Request = "Temp/HXIME-font-setup.request";
        static HXIMEFontSetup() { EditorApplication.update += ProcessRequest; }
        private static void ProcessRequest()
        {
            if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(Request);
            Configure();
        }

        [MenuItem("Tools/HXIME/Bake Japanese and Korean Font Fallback")]
        public static void Configure()
        {
            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    throw new InvalidOperationException("Exit Play mode before baking fonts.");
                var characters = new HashSet<uint>();
                for (uint c = 0xAC00; c <= 0xD7A3; c++) characters.Add(c);
                for (uint c = 0x1100; c <= 0x11FF; c++) characters.Add(c);
                for (uint c = 0x3131; c <= 0x318E; c++) characters.Add(c);
                // Include every character in the bundled default dictionaries and localized UI.
                foreach (string file in new[] { "japanese_mozc_common.dict.tsv.txt", "korean_nikl_common.dict.tsv.txt" })
                    foreach (string line in File.ReadLines(Root + "/Dicts/" + file))
                    {
                        if (line.StartsWith("#") || !line.Contains("\t")) continue;
                        string word = line.Split('\t')[0];
                        for (int i = 0; i < word.Length; i++)
                        {
                            uint c = (uint)char.ConvertToUtf32(word, i);
                            characters.Add(c);
                            if (char.IsHighSurrogate(word[i])) i++;
                        }
                    }
                foreach (char c in "クリア確定前へ次このキーボードで入力スン完全一致分離情報リセット지우기입력이전다음키보드로하세요스킨정확일치분리정보초기화") characters.Add(c);
                string path = Root + "/Fonts/NotoSansMultilingualFallback.asset";
                TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (fallback == null)
                {
                    Font source = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/NotoSansCJKkr-Regular.otf");
                    fallback = TMP_FontAsset.CreateFontAsset(source, 40, 4, GlyphRenderMode.SDFAA, 4096, 4096, AtlasPopulationMode.Dynamic, true);
                    fallback.name = "NotoSansMultilingualFallback";
                    fallback.TryAddCharacters(characters.OrderBy(c => c).ToArray(), out uint[] missing);
                    if (missing != null && missing.Length > 0)
                        throw new InvalidOperationException("Source font missing: " + string.Join(",", missing.Select(c => "U+" + c.ToString("X"))));
                    fallback.atlasPopulationMode = AtlasPopulationMode.Static;
                    AssetDatabase.CreateAsset(fallback, path);
                    AssetDatabase.AddObjectToAsset(fallback.material, fallback);
                    foreach (Texture2D atlas in fallback.atlasTextures)
                        if (atlas != null && !AssetDatabase.Contains(atlas)) AssetDatabase.AddObjectToAsset(atlas, fallback);
                }
                foreach (uint c in characters)
                    if (!fallback.characterLookupTable.ContainsKey(c)) throw new InvalidOperationException("Rebuild fallback: missing U+" + c.ToString("X"));
                var configured = new List<string>();
                foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { Root + "/Fonts" }))
                {
                    TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                    if (font == fallback) continue;
                    if (font.fallbackFontAssetTable == null) font.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    if (!font.fallbackFontAssetTable.Contains(fallback)) font.fallbackFontAssetTable.Add(fallback);
                    EditorUtility.SetDirty(font);
                    AssetDatabase.SaveAssetIfDirty(font);
                    configured.Add(font.name);
                }
                EditorUtility.SetDirty(fallback);
                AssetDatabase.SaveAssetIfDirty(fallback);
                string report = "PASS: " + characters.Count + " characters; " + fallback.atlasTextures.Length + " static atlases; fonts: " + string.Join(", ", configured);
                File.WriteAllText("Temp/HXIME-font-setup.txt", report);
                Debug.Log("HXIME fonts: " + report);
            }
            catch (Exception e)
            {
                File.WriteAllText("Temp/HXIME-font-setup.txt", "FAIL: " + e);
                Debug.LogException(e);
            }
        }
    }
}
#endif
