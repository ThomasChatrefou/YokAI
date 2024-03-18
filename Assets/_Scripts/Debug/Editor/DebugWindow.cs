using UnityEngine;
using UnityEditor;

namespace YokAI.Debugging
{
    public class DebugWindow : EditorWindow
    {
        public const int TabsCount = 2;

        public DebuggerTab[] Tabs
        {
            get
            {
                _tabs ??= new DebuggerTab[TabsCount]
                {
                    DebuggerTab.Create<DebugGameTab>(this),
                    DebuggerTab.Create<DebugAITab>(this),
                };
                return _tabs;
            }
        }

        [MenuItem("Window/YokAIDebugger")]
        public static void ShowWindow()
        {
            DebugWindow debugger = (DebugWindow)GetWindow(typeof(DebugWindow));
            debugger.minSize = new Vector2(300, 200);
        }

        private void Update()
        {
            Repaint();
        }

        private void OnGUI()
        {
            _tabId = GUILayout.Toolbar(_tabId, _tabNames);
            Tabs[_tabId].Open();
        }

        private static DebuggerTab[] _tabs;
        private static readonly string[] _tabNames = new string[] { TabNames.GAME, TabNames.AI };
        private int _tabId = 0;
    }
}
