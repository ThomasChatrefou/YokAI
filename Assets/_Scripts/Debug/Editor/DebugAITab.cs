using UnityEditor;
using UnityEngine;
using YokAI.AI;

namespace YokAI.Debugging
{
    public class DebugAITab : DebuggerTab
    {
        public override void Open()
        {
            GUILayout.Label(TabNames.AI, EditorStyles.whiteLargeLabel);

            float width = 150f;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.NB_POSITION_REACHED, GUILayout.Width(width));
            EditorGUILayout.LabelField(YokAIDebugger.GiveMeAName());
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
        }
    }
}
