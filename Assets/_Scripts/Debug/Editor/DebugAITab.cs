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

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.NB_POSITION_REACHED, GUILayout.Width(_staticLabelWidth));
            EditorGUILayout.LabelField(YokAIDebugger.GiveMeAName());
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
        }

        public override void OnRefresh(bool hasSettings)
        {
            _staticLabelWidth = hasSettings ? _window.Settings.AiTabLabelWidth : DefaultDebugWindowSettings.LABEL_WIDTH;
        }

        private float _staticLabelWidth = DefaultDebugWindowSettings.LABEL_WIDTH;
    }
}
