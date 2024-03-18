using UnityEngine;
using UnityEditor;
using YokAI.POC;

namespace YokAI.Debugging
{
    public class DebugGameTab : DebuggerTab
    {
        public override void Open()
        {
            BanDebugger.GetAvailableMoves(ref _availableMoves);

            GUILayout.Label(TabNames.GAME, EditorStyles.whiteLargeLabel);
            GUILayout.Space(10);

            PrintGameInformation();
            GUILayout.Space(10);

            HandleMoveMaking();
            GUILayout.Space(10);

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

        public void HandleMoveMaking()
        {
            GUILayout.Label(Labels.NEXT_MOVE, EditorStyles.whiteLargeLabel);

            GUILayout.BeginHorizontal();
            float width = EditorUtility.GetUniformWidthFromWindow(_window.position.width, 3);
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
                    UnityEngine.Debug.Log(Feedbacks.INVALID);
                }
            }
            if (doPass)
            {
                BanDebugger.PassTurn();
                BanDebugger.GetAvailableMoves(ref _availableMoves);
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
                BanDebugger.LoadSFEN(_userInputSFEN);
            }
            if (doSave)
            {
                UnityEngine.Debug.Log("Not implemented yet");
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

        public void HandleFooter()
        {
            if (GUILayout.Button(Buttons.DRAW))
            {
                BanDebugger.DrawBanInConsole();
            }
        }

        private string _availableMoves;
        private string _userInputNotation;
        private string _userInputSFEN;
        private string _feedbackOnMove;
    }
}
