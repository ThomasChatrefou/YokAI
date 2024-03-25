using YokAI.Main;
using YokAI.Properties;
using YokAI.Notation;
using YokAI.AI;

namespace YokAI.Debugging
{
    public static class YokAIDebugger
    {
        public const int MAX_MOVES_PER_LINE = 10;
        public const string MOVE_SEPARATOR = "  ";

        public static string GetPlayingColor()
        {
            if (GameController.IsGameSet)
            {
                return GameController.PlayingColor == Color.WHITE ? ColorName.WHITE : (GameController.PlayingColor == Color.BLACK ? ColorName.BLACK : ColorName.UNKNOWN);
            }
            return Symbol.INVALID.ToString();
        }

        public static string GetMoveNumber()
        {
            if (GameController.IsGameSet)
            {
                return GameController.MoveNumber.ToString();
            }
            return Symbol.INVALID.ToString();
        }

        public static string GetLastMove()
        {
            if (GameController.IsGameSet && GameController.LastMove != Move.INVALID)
            {
                return Decryptor.GetNotationFromMove(GameController.LastMove, ref GameController.GetBan());
            }
            return Symbol.INVALID.ToString();
        }

        public static bool SendUserInputMove(string inputNotation)
        {
            if (GameController.IsGameSet && GameController.TryMakeMove(inputNotation))
            {
                if (BoardManager.IsValid)
                {
                    BoardManager.Instance.AIMovePiece(GameController.LastMove);
                    BoardManager.Instance.MakeMoveOnTheBoard(GameController.LastMove);
                }
                return true;
            }
            return false;
        }

        public static void GetAvailableMoves(ref string[] cachedResult)
        {
            if (GameController.IsGameSet)
            {
                int moveCount = GameController.AvailableMoves.Length;
                int nbLines = moveCount / MAX_MOVES_PER_LINE + 1;
                cachedResult = new string[nbLines];
                for (int i = 0; i < moveCount; ++i)
                {
                    int lineIndex = i / MAX_MOVES_PER_LINE;
                    cachedResult[lineIndex] += Decryptor.GetNotationFromMove(GameController.AvailableMoves[i], ref GameController.GetBan());
                    cachedResult[lineIndex] += MOVE_SEPARATOR;
                }
                return;
            }
            cachedResult = new string[1] { Symbol.INVALID.ToString() };
        }

        public static void PassTurn()
        {
            GameController.PassTurn();
        }
        
        public static void TakeBack()
        {
            if (GameController.IsGameSet)
            {
                GameController.TakeBack();
                if (BoardManager.IsValid)
                {
                    BoardManager.Instance.AIMovePiece(GameController.LastMove);
                    BoardManager.Instance.MakeMoveOnTheBoard(GameController.LastMove);
                }
            }
        }

        public static void LoadPosition(string sfen)
        {
            GameController.SetupPosition(sfen);
            TrySetupBoard();
        }

        public static void RestartPosition()
        {
            GameController.ResetPosition();
            TrySetupBoard();
        }
        
        public static void SavePosition()
        {
            GameController.SaveCurrentPosition();
        }

        public static void EmptyPosition()
        {
            GameController.Clear();
            TrySetupBoard();
        }

        private static void TrySetupBoard()
        {
            if (BoardManager.IsValid)
            {
                BoardManager.Instance.ResetBoard();
                BoardManager.Instance.SetupBoard();
            }
        }


        // AI Tab
        public static string GiveMeAName()
        {
            if (AIManager.IsValid)
            {
                //return AIManager.Instance.playerOne.Evaluator.NbPositionReached.ToString();
            }
            return Symbol.INVALID.ToString();
        }
    }
}
