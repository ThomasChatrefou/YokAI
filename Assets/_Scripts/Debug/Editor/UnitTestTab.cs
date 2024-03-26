using UnityEditor;
using UnityEngine;

namespace YokAI.Debugging
{
    public class UnitTestTab : DebuggerTab
    {
        public override void Open()
        {
            GUILayout.Label(TabNames.UNIT_TESTS, EditorStyles.whiteLargeLabel);
        }

        public override void OnRefresh(bool hasSettings)
        {

        }
    }
}