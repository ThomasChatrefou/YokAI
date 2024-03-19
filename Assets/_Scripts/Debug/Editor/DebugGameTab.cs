using UnityEngine;
using UnityEditor;
using YokAI.Notation;

namespace YokAI.Debugging
{
    public class DebugGameTab : DebuggerTab
    {
        public const float SPACE_BETWEEN_PARTS = 10f;

        public override void Open()
        {
            //GUILayout.Label(TabNames.GAME, EditorStyles.whiteLargeLabel);
            GUILayout.Space(SPACE_BETWEEN_PARTS);

            PrintGameInformation();
            GUILayout.Space(SPACE_BETWEEN_PARTS);

            HandleMoveMaking();
            GUILayout.Space(SPACE_BETWEEN_PARTS);

            HandlePositionLoading();

            GUILayout.FlexibleSpace();
            HandleFooter();
        }

        public void PrintGameInformation()
        {
            GUILayout.Label(Labels.GAME_INFOS, EditorStyles.whiteLargeLabel);

            float width = 120f;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.PLAYING_COLOR, GUILayout.Width(width));
            EditorGUILayout.LabelField(YokAIDebugger.GetPlayingColor());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.MOVE_NUMBER, GUILayout.Width(width));
            EditorGUILayout.LabelField(YokAIDebugger.GetMoveNumber());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.LAST_MOVE, GUILayout.Width(width));
            EditorGUILayout.LabelField(YokAIDebugger.GetLastMove());
            GUILayout.EndHorizontal();

            YokAIDebugger.GetAvailableMoves(ref _availableMoves);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.AVAILABLE_MOVES, GUILayout.Width(width));
            EditorGUILayout.SelectableLabel(_availableMoves[0], GUILayout.Height(GUI.skin.textField.lineHeight));
            GUILayout.EndHorizontal();
            if (_availableMoves.Length > 1)
            {
                for (int i = 1; i < _availableMoves.Length; ++i)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Empty, GUILayout.Width(width));
                    EditorGUILayout.SelectableLabel(_availableMoves[i], GUILayout.Height(GUI.skin.textField.lineHeight));
                    GUILayout.EndHorizontal();
                }
            }
        }

        public void HandleMoveMaking()
        {
            GUILayout.Label(Labels.NEXT_MOVE, EditorStyles.whiteLargeLabel);

            GUILayout.BeginHorizontal();
            float width = EditorUtility.GetUniformWidthFromWindow(_window.position.width, 3);
            _userInputNotation = EditorGUILayout.TextArea(_userInputNotation, GUILayout.Width(width));
            bool doValidate = GUILayout.Button(Buttons.VALIDATE, GUILayout.Width(width));
            bool doPass = GUILayout.Button(Buttons.PASS_TURN, GUILayout.Width(width));
            GUILayout.EndHorizontal();

            if (doValidate)
            {
                YokAIDebugger.SendUserInputMove(_userInputNotation);
            }
            if (doPass)
            {
                YokAIDebugger.PassTurn();
            }
        }

        public void HandlePositionLoading()
        {
            float width = EditorUtility.GetUniformWidthFromWindow(WinWidth, 3);

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
                YokAIDebugger.LoadPosition(_userInputSFEN);
            }
            if (doSave)
            {
                YokAIDebugger.SavePosition();
            }
            if (doEmpty)
            {
                YokAIDebugger.EmptyPosition();
            }
            if (doStarting)
            {
                YokAIDebugger.RestartPosition();
            }
        }

        public void HandleFooter()
        {
        }

        private string[] _availableMoves = new string[1] { Symbol.INVALID.ToString() };
        private string _userInputNotation;
        private string _userInputSFEN;
    }
}
