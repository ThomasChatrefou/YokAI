using UnityEngine;
using UnityEditor;

namespace YokAI.Debugging
{
    public class DebugWindow : EditorWindow
    {
        public const int TabsCount = 3;
        public DebuggerTab[] Tabs { get { return _tabs; } }
        public DebugWindowSettings Settings { get { return _settings; } }

        [MenuItem("Window/YokAIDebugger")]
        public static void ShowWindow()
        {
            DebugWindow debugger = (DebugWindow)GetWindow(typeof(DebugWindow));
            debugger.minSize = new Vector2(300, 200);
        }

        private void OnEnable()
        {
            _tabs = new DebuggerTab[TabsCount]
            {
                DebuggerTab.Create<DebugGameTab>(this),
                DebuggerTab.Create<DebugAITab>(this),
                DebuggerTab.Create<DebugAITab>(this),
            };
            Refresh();
        }

        private void OnGUI()
        {
            _tabId = GUILayout.Toolbar(_tabId, _tabNames);
            Tabs[_tabId].Open();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Buttons.REFRESH))
            {
                Refresh();
            }
            Repaint();
        }

        private void Refresh()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(DebugWindowSettings)}");
            bool hasSettings = guids.Length > 0;
            if (hasSettings)
            {
                _settings = AssetDatabase.LoadAssetAtPath<DebugWindowSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
                _settings.OnChanged += () => RefreshTabs(true);
            }
            else
            {
                Debug.Log($"[{nameof(DebugWindow)}] Settings not found...");
            }
            RefreshTabs(hasSettings);
        }

        private void RefreshTabs(bool hasSettings)
        {
            foreach (DebuggerTab tab in _tabs)
            {
                tab.OnRefresh(hasSettings);
            }
        }

        private static DebuggerTab[] _tabs;
        private static readonly string[] _tabNames = new string[]
        { 
            TabNames.GAME,
            TabNames.AI,
            TabNames.UNIT_TESTS
        };
        private int _tabId = 0;
        private DebugWindowSettings _settings = null;
    }
}
