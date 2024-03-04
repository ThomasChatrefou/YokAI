using UnityEngine;
using UnityEditor;
using YokAI;

public class YokAIDebugger : EditorWindow
{
    public int TabId = 2;
    private readonly string[] _tabNames = new string[] { "Move Generator", "Template Tab" };

    private string _availableMoves;
    private string _userInputNotation;
    private string _userInputFeedback;

    [MenuItem("Window/YokAIDebugger")]
    public static void ShowWindow()
    {
        YokAIDebugger debugger = (YokAIDebugger)GetWindow(typeof(YokAIDebugger));
        debugger.minSize = new Vector2(300, 200);
    }

    private void OnGUI()
    {
        TabId = GUILayout.Toolbar(TabId, _tabNames);

        switch (TabId)
        {
            case 0:
                OpenMoveGeneratorTab();
                break;
            case 1:
                OpenTemplateTab();
                break;
        }
    }

    private void OpenMoveGeneratorTab()
    {
        GUILayout.Label("Move Generator", EditorStyles.whiteLargeLabel);

        EditorGUILayout.LabelField($"Playing Color : " + BanDebugger.GetPlayingColor());
        EditorGUILayout.LabelField($"Move Number : " + BanDebugger.GetMoveNumber());
        EditorGUILayout.LabelField($"Last Move : " + BanDebugger.GetLastMove());
        EditorGUILayout.SelectableLabel($"Available Moves : " + _availableMoves);

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel($"Input move : ");
        _userInputNotation = EditorGUILayout.TextArea(_userInputNotation);
        if (GUILayout.Button("Validate"))
        {
            ValidateInputMove();
        }
        if (GUILayout.Button("Validate&Draw"))
        {
            ValidateInputMove();
            BanDebugger.DrawBanInConsole();
        }

        GUILayout.Space(10);
        EditorGUILayout.PrefixLabel(_userInputFeedback);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Setup starting position"))
        {
            BanDebugger.SetupStartingPosition();
            BanDebugger.GetAvailableMoves(ref _availableMoves);
            _userInputFeedback = " ";
        }
        if (GUILayout.Button("Draw board in console"))
        {
            BanDebugger.DrawBanInConsole();
        }
    }

    private void ValidateInputMove()
    {
        if (BanDebugger.SendUserInputMove(_userInputNotation))
        {
            _userInputFeedback = "Done";
            BanDebugger.GetAvailableMoves(ref _availableMoves);
        }
        else
        {
            _userInputFeedback = "Invalid";
        }
    }

    private void OpenTemplateTab()
    {
        GUILayout.Label("Template Tab");
        GUILayout.FlexibleSpace();
    }
}
