#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HX2xianglong90.HXIME.EditorTools
{
    public static class HXIMELookupSetup
    {
        public const string SimplifiedSource = "pinyin_simp.dict.yaml.txt";
        public const string TraditionalSource = "luna_pinyin.dict.yaml.txt";
        public const string JapaneseSource = "japanese_mozc_common.dict.tsv.txt";
        public const string KoreanSource = "korean_nikl_common.dict.tsv.txt";
        public static readonly string[] AllSources =
            { SimplifiedSource, TraditionalSource, JapaneseSource, KoreanSource };

        // 进入 Play 模式或构建世界时把独立资产里的词条与索引写进 Udon。
        // 任何语言未加载或未建索引都会抛错：构建直接失败，Play 由下面的拦截取消。
        [UnityEditor.Callbacks.PostProcessScene(-100)]
        public static void BakeScene()
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (PinyinDict dictionary in root.GetComponentsInChildren<PinyinDict>(true))
                {
                    PinyinLookupBuilder.BakeRuntime(dictionary);
                    UdonSharpEditorUtility.CopyProxyToUdon(dictionary);
                }
        }

        static HXIMELookupSetup() { EditorApplication.playModeStateChanged += OnPlayModeStateChanged; }

        // Play 模式硬阻断：校验不过就取消这次进入 Play，并列出缺哪些语言。
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            string[] errors = CollectErrors();
            if (errors.Length == 0) return;
            EditorApplication.isPlaying = false;
            string message = "HXIME: cannot enter Play mode.\n\n" + string.Join("\n\n", errors);
            Debug.LogError(message);
            EditorUtility.DisplayDialog("HXIME dictionaries are not ready", message, "OK");
        }

        // ---------------------------------------------------------------- 菜单：每种语言单独设置
        [MenuItem("Tools/HXIME/Simplified Chinese/Load and Apply Dictionary")]
        private static void LoadSimplified() => LoadSource(SimplifiedSource);
        [MenuItem("Tools/HXIME/Simplified Chinese/Rebuild Lookup Index")]
        private static void RebuildSimplified() => RebuildSource(SimplifiedSource);

        [MenuItem("Tools/HXIME/Traditional Chinese/Load and Apply Dictionary")]
        private static void LoadTraditional() => LoadSource(TraditionalSource);
        [MenuItem("Tools/HXIME/Traditional Chinese/Rebuild Lookup Index")]
        private static void RebuildTraditional() => RebuildSource(TraditionalSource);

        [MenuItem("Tools/HXIME/Japanese/Load and Apply Dictionary")]
        private static void LoadJapanese() => LoadSource(JapaneseSource);
        [MenuItem("Tools/HXIME/Japanese/Rebuild Lookup Index")]
        private static void RebuildJapanese() => RebuildSource(JapaneseSource);
        [MenuItem("Tools/HXIME/Japanese/Enable Language Button")]
        private static void EnableJapanese() => HXIMELanguageSetup.SetLanguageSwitch("Japanese", true);

        [MenuItem("Tools/HXIME/Korean/Load and Apply Dictionary")]
        private static void LoadKorean() => LoadSource(KoreanSource);
        [MenuItem("Tools/HXIME/Korean/Rebuild Lookup Index")]
        private static void RebuildKorean() => RebuildSource(KoreanSource);
        [MenuItem("Tools/HXIME/Korean/Enable Language Button")]
        private static void EnableKorean() => HXIMELanguageSetup.SetLanguageSwitch("Korean", true);

        [MenuItem("Tools/HXIME/Load and Apply All Dictionaries")]
        private static void LoadAll() { foreach (string source in AllSources) LoadSource(source); }
        [MenuItem("Tools/HXIME/Rebuild All Lookup Indexes")]
        private static void RebuildAll() { foreach (string source in AllSources) RebuildSource(source); }

        [MenuItem("Tools/HXIME/Validate All Dictionaries")]
        public static void ValidateMenu()
        {
            string[] errors = CollectErrors();
            if (errors.Length == 0)
            {
                Debug.Log("HXIME: every dictionary in the open scenes is loaded and indexed.");
                return;
            }
            Debug.LogError("HXIME: " + errors.Length + " dictionary problem(s):\n" + string.Join("\n", errors));
        }

        // ---------------------------------------------------------------- 工具实现
        // 同一语言的多个组件（预制件里的与场景实例里的）共用同一份 <对象名>.asset，
        // 按资产路径去重，避免同一个大文件被解析多次。
        public static void LoadSource(string sourceFileName)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("HXIME: exit Play mode before loading dictionaries.");
                return;
            }
            foreach (var group in FindDictionaries(sourceFileName)
                .GroupBy(d => PinyinLookupBuilder.DataAssetPath(d)))
                PinyinLookupBuilder.LoadAndApply(group.First());
        }

        public static void RebuildSource(string sourceFileName)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("HXIME: exit Play mode before rebuilding lookup indexes.");
                return;
            }
            foreach (var group in FindDictionaries(sourceFileName)
                .GroupBy(d => PinyinLookupBuilder.DataAssetPath(d)))
                PinyinLookupBuilder.Rebuild(group.First());
        }

        // 英文错误清单：场景里每个未加载或未建索引的语言各一条（去重）。
        public static string[] CollectErrors()
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var errors = new List<string>();
            foreach (PinyinDict dictionary in SceneDictionaries())
            {
                string error = PinyinLookupBuilder.Validate(dictionary);
                if (error != null && seen.Add(error)) errors.Add(error);
            }
            return errors.ToArray();
        }

        public static IEnumerable<PinyinDict> SceneDictionaries()
        {
            foreach (PinyinDict dictionary in UnityEngine.Object.FindObjectsOfType<PinyinDict>(true))
            {
                Scene scene = dictionary.gameObject.scene;
                if (EditorUtility.IsPersistent(dictionary) || !scene.IsValid() || !scene.isLoaded
                    || EditorSceneManager.IsPreviewScene(scene)) continue;
                yield return dictionary;
            }
        }

        // 该语言在预制件资产与打开场景里的全部字典组件（用于按语言加载/建索引）。
        private static List<PinyinDict> FindDictionaries(string sourceFileName)
        {
            var found = new List<PinyinDict>();
            foreach (string guid in AssetDatabase.FindAssets("HXIME_Pinyin t:Prefab"))
            {
                GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (root == null) continue;
                foreach (PinyinDict dictionary in root.GetComponentsInChildren<PinyinDict>(true))
                    if (Matches(dictionary, sourceFileName) && !found.Contains(dictionary)) found.Add(dictionary);
            }
            foreach (PinyinDict dictionary in SceneDictionaries())
                if (Matches(dictionary, sourceFileName) && !found.Contains(dictionary)) found.Add(dictionary);
            return found;
        }

        private static bool Matches(PinyinDict dictionary, string sourceFileName)
            => string.Equals(PinyinLookupBuilder.SourceFileName(dictionary), sourceFileName, StringComparison.OrdinalIgnoreCase);
    }

    // 构建世界前的硬校验：任何语言未加载或未建索引都会让构建直接失败。
    public class HXIMEBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            string[] errors = HXIMELookupSetup.CollectErrors();
            if (errors.Length == 0) return;
            throw new BuildFailedException("HXIME: " + errors.Length + " dictionary problem(s):\n"
                + string.Join("\n", errors));
        }
    }
}
#endif
