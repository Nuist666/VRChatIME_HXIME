#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UdonSharpEditor;
using UnityEditor;

namespace HX2xianglong90.HXIME.EditorTools
{
    [CustomEditor(typeof(HXIMEUI))]
    [CanEditMultipleObjects]
    public class HXIMEUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets)) return;

            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
                {
                    if (property.propertyPath == "candidateLimits")
                        EditorGUILayout.PropertyField(property,
                            new UnityEngine.GUIContent("Candidate Count", property.tooltip), true);
                    else
                        EditorGUILayout.PropertyField(property, true);
                }
            }
            serializedObject.ApplyModifiedProperties();

            foreach (UnityEngine.Object selected in targets)
            {
                var selectedObject = new SerializedObject(selected);
                if (selectedObject.FindProperty("candidateLimits").intValue <= 100) continue;

                EditorGUILayout.HelpBox(
                    "Candidate Count is above 100. Higher limits may increase input latency, especially with short inputs or large dictionaries. Test performance in the VRChat client.",
                    MessageType.Warning);
                break;
            }
        }
    }
}
#endif
