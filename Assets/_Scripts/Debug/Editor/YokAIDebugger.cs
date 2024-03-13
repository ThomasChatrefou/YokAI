using UnityEngine;
using UnityEditor;
using YokAI.POC;
using YokAI.AI;

public class YokAIDebugger : EditorWindow
{
    #region Keys

    private static class Tabs
    {
        public const string GAME = "Game";
        public const string AI = "AI";
    }

    private static class Labels
    {
        public const string GAME_INFOS = "Game Infos";
        public const string PLAYING_COLOR = "Playing Color : ";
        public const string MOVE_NUMBER = "Move Number : ";
        public const string LAST_MOVE = "Last Move : ";
        public const string AVAILABLE_MOVES = "Available Moves : ";
        public const string NEXT_MOVE = "Next Move";
        public const string LOAD_POSITION = "Load Position";
    }

    private static class Buttons
    {
        public const string VALIDATE = "Validate";
        public const string PASS_TURN = "Pass Turn"; //unused
        public const string LOAD_SFEN = "Load SFEN";
        public const string SAVE_SFEN = "Save SFEN";
        public const string EMPTY_POSITION = "Empty position";
        public const string START_POSITION = "Starting position";
        public const string DRAW = "Draw Ban in console";
    }

    private static class Feedbacks
    {
        public const string DONE = "Done";
        public const string INVALID = "Invalid";
    }

    #endregion Keys

    private int _tabId = 2;
    private readonly string[] _tabNames = new string[] { Tabs.GAME, Tabs.AI };

    private string _availableMoves;
    private string _userInputNotation;
    private string _userInputSFEN;
    private string _feedbackOnMove;

    private const float PADDING = 4f;


    [MenuItem("Window/YokAIDebugger")]
    public static void ShowWindow()
    {
        YokAIDebugger debugger = (YokAIDebugger)GetWindow(typeof(YokAIDebugger));
        debugger.minSize = new Vector2(300, 200);
    }

    private void OnFocus()
    {
        BanDebugger.GetAvailableMoves(ref _availableMoves);
    }

    private void OnGUI()
    {
        _tabId = GUILayout.Toolbar(_tabId, _tabNames);

        switch (_tabId)
        {
            case 0:
                OpenMoveGeneratorTab();
                break;
            case 1:
                OpenAITab();
                break;
        }
    }

    #region GameTab

    private void OpenMoveGeneratorTab()
    {
        PrintGameInformation();
        GUILayout.Space(10);

        HandleMoveMaking();
        GUILayout.Space(10);

        HandlePositionLoading();

        GUILayout.FlexibleSpace();
        HandleFooter();
    }

    private void PrintGameInformation()
    {
        GUILayout.Label(Labels.GAME_INFOS, EditorStyles.whiteLargeLabel);

        float width = 120f;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Labels.PLAYING_COLOR, GUILayout.Width(width));
        EditorGUILayout.LabelField(BanDebugger.GetPlayingColor());
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Labels.MOVE_NUMBER, GUILayout.Width(width));
        EditorGUILayout.LabelField(BanDebugger.GetMoveNumber());
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Labels.LAST_MOVE, GUILayout.Width(width));
        EditorGUILayout.LabelField(BanDebugger.GetLastMove());
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(Labels.AVAILABLE_MOVES, GUILayout.Width(width));
        EditorGUILayout.SelectableLabel(_availableMoves, GUILayout.Height(GUI.skin.textField.lineHeight));
        GUILayout.EndHorizontal();
    }

    private void HandleMoveMaking()
    {
        GUILayout.Label(Labels.NEXT_MOVE, EditorStyles.whiteLargeLabel);

        GUILayout.BeginHorizontal();
        float width = GetUniformWidthFromWindow(3);
        _userInputNotation = EditorGUILayout.TextArea(_userInputNotation, GUILayout.Width(width));
        bool doValidate = GUILayout.Button(Buttons.VALIDATE, GUILayout.Width(width));
        bool doPass = GUILayout.Button(Buttons.PASS_TURN, GUILayout.Width(width));
        EditorGUILayout.LabelField(_feedbackOnMove);
        GUILayout.EndHorizontal();

        if (doValidate)
        {
            if (BanDebugger.SendUserInputMove(_userInputNotation))
            {
                BanDebugger.GetAvailableMoves(ref _availableMoves);
                BanDebugger.DrawBanInConsole();
            }
            else
            {
                Debug.Log(Feedbacks.INVALID);
            }
        }
        if (doPass)
        {
            BanDebugger.PassTurn();
            BanDebugger.GetAvailableMoves(ref _availableMoves);
        }
    }

    private void HandlePositionLoading()
    {
        float width = GetUniformWidthFromWindow(3);

        GUILayout.BeginHorizontal();
        GUILayout.Label(Labels.LOAD_POSITION, EditorStyles.whiteLargeLabel);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        _userInputSFEN = EditorGUILayout.TextField(_userInputSFEN, GUILayout.Width(width));
        bool doLoad = GUILayout.Button(Buttons.LOAD_SFEN, GUILayout.Width(width));
        bool doSave = GUILayout.Button(Buttons.SAVE_SFEN, GUILayout.Width(width));
        GUILayout.EndHorizontal();

        bool doEmpty = GUILayout.Button(Buttons.EMPTY_POSITION);
        bool doStarting = GUILayout.Button(Buttons.START_POSITION);

        if (doLoad)
        {
            BanDebugger.LoadSFEN(_userInputSFEN);
        }
        if (doSave)
        {
            Debug.Log("Not implemented yet");
        }
        if (doEmpty)
        {
            BanDebugger.SetupEmptyPosition();
        }
        if (doStarting)
        {
            BanDebugger.SetupStartingPosition();
        }
        if (doLoad || doEmpty || doStarting)
        {
            _feedbackOnMove = " ";
            BanDebugger.GetAvailableMoves(ref _availableMoves);
            BanDebugger.DrawBanInConsole();
        }
    }

    private void HandleFooter()
    {
        if (GUILayout.Button(Buttons.DRAW))
        {
            BanDebugger.DrawBanInConsole();
        }
    }

    #endregion GameTab

    #region AITab

    private void OpenAITab()
    {
        GUILayout.Label(Tabs.AI);

        float width = 120f;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("nb moves reached", GUILayout.Width(width));
        EditorGUILayout.LabelField(Evaluator.NbPositionReached.ToString());
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
    }

    #endregion AITab


    #region Utility

    private float GetUniformWidthFromWindow(int nbElementsInLine)
    {
        return (position.width - nbElementsInLine * PADDING) / nbElementsInLine;
    }

    #endregion Utility
}
