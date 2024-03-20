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

            float width = 120f;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.NB_POSITION_REACHED, GUILayout.Width(width));
            EditorGUILayout.LabelField(AIManager.Instance.playerOne.Evaluator.NbPositionReached.ToString());
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
        }
    }
}
