using UnityEngine;
using UnityEditor;
using YokAI.Notation;

namespace YokAI.Debugging
{
    public class DebugGameTab : DebuggerTab
    {
        private float _sectionsInterspace = DefaultDebugWindowSettings.SECTIONS_INTERSPACE;
        private float _staticLabelWidth = DefaultDebugWindowSettings.LABEL_WIDTH;

        public override void Open()
        {
            //GUILayout.Label(TabNames.GAME, EditorStyles.whiteLargeLabel);

            DisplaySection_GameInformation();
            GUILayout.Space(_sectionsInterspace);

            DisplaySection_MoveMaker();
            GUILayout.Space(_sectionsInterspace);

            DisplaySection_PositionLoader();

            GUILayout.FlexibleSpace();
            DisplayFooter();
        }

        public override void OnRefresh(bool hasSettings)
        {
            _sectionsInterspace = hasSettings ? _window.Settings.GameTabSectionsInterspace : DefaultDebugWindowSettings.SECTIONS_INTERSPACE;
            _staticLabelWidth = hasSettings ? _window.Settings.GameTabLabelWidth : DefaultDebugWindowSettings.LABEL_WIDTH;
        }

        public void DisplaySection_GameInformation()
        {
            GUILayout.Label(Labels.GAME_INFOS, EditorStyles.whiteLargeLabel);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.PLAYING_COLOR, GUILayout.Width(_staticLabelWidth));
            EditorGUILayout.LabelField(YokAIDebugger.GetPlayingColor());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.MOVE_NUMBER, GUILayout.Width(_staticLabelWidth));
            EditorGUILayout.LabelField(YokAIDebugger.GetMoveNumber());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.LAST_MOVE, GUILayout.Width(_staticLabelWidth));
            EditorGUILayout.LabelField(YokAIDebugger.GetLastMove());
            GUILayout.EndHorizontal();

            YokAIDebugger.GetAvailableMoves(ref _availableMoves);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Labels.AVAILABLE_MOVES, GUILayout.Width(_staticLabelWidth));
            EditorGUILayout.SelectableLabel(_availableMoves[0], GUILayout.Height(GUI.skin.textField.lineHeight));
            GUILayout.EndHorizontal();
            if (_availableMoves.Length > 1)
            {
                for (int i = 1; i < _availableMoves.Length; ++i)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Empty, GUILayout.Width(_staticLabelWidth));
                    EditorGUILayout.SelectableLabel(_availableMoves[i], GUILayout.Height(GUI.skin.textField.lineHeight));
                    GUILayout.EndHorizontal();
                }
            }
        }

        public void DisplaySection_MoveMaker()
        {
            GUILayout.Label(Labels.NEXT_MOVE, EditorStyles.whiteLargeLabel);

            float dynamicWidth = EditorUtility.GetUniformWidthFromWindow(_window.position.width, 4);

            GUILayout.BeginHorizontal();
            _userInputNotation = EditorGUILayout.TextArea(_userInputNotation, GUILayout.Width(dynamicWidth));
            bool doValidate = GUILayout.Button(Buttons.VALIDATE, GUILayout.Width(dynamicWidth));
            bool doPass = GUILayout.Button(Buttons.PASS_TURN, GUILayout.Width(dynamicWidth));
            bool doTakeBack = GUILayout.Button(Buttons.TAKE_BACK, GUILayout.Width(dynamicWidth));
            GUILayout.EndHorizontal();

            if (doValidate)
            {
                YokAIDebugger.SendUserInputMove(_userInputNotation);
            }
            if (doPass)
            {
                YokAIDebugger.PassTurn();
            }
            if (doTakeBack)
            {
                YokAIDebugger.TakeBack();
            }
        }

        public void DisplaySection_PositionLoader()
        {
            float dynamicWidth = EditorUtility.GetUniformWidthFromWindow(WinWidth, 3);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Labels.LOAD_POSITION, EditorStyles.whiteLargeLabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _userInputSFEN = EditorGUILayout.TextField(_userInputSFEN, GUILayout.Width(dynamicWidth));
            bool doLoad = GUILayout.Button(Buttons.LOAD_SFEN, GUILayout.Width(dynamicWidth));
            bool doSave = GUILayout.Button(Buttons.SAVE_SFEN, GUILayout.Width(dynamicWidth));
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

        public void DisplayFooter()
        {
        }

        private string[] _availableMoves = new string[1] { Symbol.INVALID.ToString() };
        private string _userInputNotation;
        private string _userInputSFEN;
    }
}
