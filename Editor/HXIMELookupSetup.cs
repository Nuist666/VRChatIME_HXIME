#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HX2xianglong90.HXIME.EditorTools
{
    public static class HXIMELookupSetup
    {
        // Bake scene overrides before UdonSharp's order-0 scene processing copies
        // and removes proxies. This also covers old scenes when entering Play mode.
        [UnityEditor.Callbacks.PostProcessScene(-100)]
        public static void BakeScene()
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (PinyinDict dictionary in root.GetComponentsInChildren<PinyinDict>(true))
                {
                    PinyinLookupBuilder.Build(dictionary);
                    UdonSharpEditorUtility.CopyProxyToUdon(dictionary);
                }
        }

        [MenuItem("Tools/HXIME/Rebuild All Dictionary Indexes")]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before rebuilding dictionary indexes.");
            // Search rather than assume a particular installation folder.
            foreach (string guid in AssetDatabase.FindAssets("HXIME_Pinyin t:Prefab"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (PinyinDict dictionary in root.GetComponentsInChildren<PinyinDict>(true)) Bake(dictionary, false);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { EditorApplication.delayCall += () => { if (root != null) PrefabUtility.UnloadPrefabContents(root); }; }
            }
            foreach (PinyinDict dictionary in UnityEngine.Object.FindObjectsOfType<PinyinDict>(true))
            {
                var scene = dictionary.gameObject.scene;
                if (EditorUtility.IsPersistent(dictionary) || !scene.IsValid() || !scene.isLoaded
                    || EditorSceneManager.IsPreviewScene(scene)) continue;
                Bake(dictionary, true);
                EditorSceneManager.MarkSceneDirty(scene);
            }
            Debug.Log("HXIME dictionary indexes rebuilt. Save open scenes to retain instance overrides.");
        }

        private static void Bake(PinyinDict dictionary, bool undo)
        {
            if (undo) Undo.RecordObject(dictionary, "Rebuild HXIME dictionary index");
            PinyinLookupBuilder.Build(dictionary);
            UdonSharpEditorUtility.CopyProxyToUdon(dictionary);
            EditorUtility.SetDirty(dictionary);
            if (undo) PrefabUtility.RecordPrefabInstancePropertyModifications(dictionary);
        }
    }
}
#endif
