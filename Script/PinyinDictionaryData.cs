#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using UnityEngine;

namespace HX2xianglong90.HXIME
{
    // Binary storage avoids huge YAML assets. This editor source is baked into
    // the existing runtime arrays, so Udon needs no custom asset type support.
    [PreferBinarySerialization]
    public sealed class PinyinDictionaryData : ScriptableObject
    {
        [HideInInspector] public string[] entries;
        [HideInInspector] public string[] pinyins;
        [HideInInspector] public int[] weights;
        [HideInInspector] public int[] indices;
        [HideInInspector] public string[] lookupCodes;
        [HideInInspector] public string[] lookupInitials;
        [HideInInspector] public int[] codeOrder;
        [HideInInspector] public int[] initialsOrder;
        [HideInInspector] public int[] wordIds;
        [HideInInspector] public int lookupVersion;

        public void CopyTo(PinyinDict dictionary)
        {
            int count = entries == null ? 0 : entries.Length;
            if (count == 0 || pinyins?.Length != count || weights?.Length != count
                || indices?.Length != count || lookupCodes?.Length != count
                || lookupInitials?.Length != count || codeOrder?.Length != count
                || initialsOrder?.Length != count || wordIds?.Length != count || lookupVersion <= 0)
                throw new InvalidOperationException("Invalid HXIME dictionary asset: " + name);
            dictionary.entries = (string[])entries.Clone();
            dictionary.pinyins = (string[])pinyins.Clone();
            dictionary.weights = (int[])weights.Clone();
            dictionary.indices = (int[])indices.Clone();
            dictionary.lookupCodes = (string[])lookupCodes.Clone();
            dictionary.lookupInitials = (string[])lookupInitials.Clone();
            dictionary.codeOrder = (int[])codeOrder.Clone();
            dictionary.initialsOrder = (int[])initialsOrder.Clone();
            dictionary.wordIds = (int[])wordIds.Clone();
            dictionary.lookupVersion = lookupVersion;
        }
    }
}
#endif
