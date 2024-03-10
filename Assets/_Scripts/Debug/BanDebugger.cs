using UnityEngine;

namespace YokAI.POC
{
    public static class BanDebugger
    {
        public static int ASCII_CODE_ALPHABET_START = 97;

        public static void DebugLog()
        {
            string log = "BAN DEBUG LOG (click to see details)\n";

            log += "Grid :\n";
            DrawBanInDebugLog(ref log);

            log += $"White Pool : ";
            foreach (int piece in Ban.WhitePool)
            {
                log += Decryptor.GetSymbolFromPiece(piece) + " ";
            }
            log += "\n";

            log += $"Black Pool : ";
            foreach (int piece in Ban.BlackPool)
            {
                log += Decryptor.GetSymbolFromPiece(piece) + " ";
            }
            log += "\n";

            log += $"Playing Color : " + GetPlayingColor() + "\n";
            log += $"Move Number : {Ban.MoveNumber}\n";
            Debug.Log(log);
        }

        public static string GetPlayingColor()
        {
            return Ban.PlayingColor == Piece.WHITE ? "White" : (Ban.PlayingColor == Piece.BLACK ? "Black" : "Unknown");
        }

        public static string GetMoveNumber()
        {
            return $"{Ban.MoveNumber}";
        }

        public static string GetLastMove()
        {
            return Decryptor.GetNotationFromMove(Ban.LastMove);
        }

        public static void GetAvailableMoves(ref string cachedResult)
        {
            cachedResult = string.Empty;
            if (Ban.IsSet)
            {
                MoveGenerator.GenerateMoves();
                foreach (Move move in MoveGenerator.Moves)
                {
                    cachedResult += Decryptor.GetNotationFromMove(move);
                    cachedResult += "  ";
                }
            }
        }

        public static bool SendUserInputMove(string inputNotation)
        {
            if (Ban.IsSet)
            {
                Move inputMove = Decryptor.GetMoveFromNotation(inputNotation);
                if (Ban.IsSet && Ban.IsValid(inputMove))
                {
                    Ban.MakeMove(inputMove);
                    return true;
                }
            }
            return false;
        }

        public static void PassTurn()
        {
            Ban.PassTurn();
        }

        public static void LoadSFEN(string sfen)
        {
            PositionLoader.LoadPositionFromSFEN(sfen);
        }
        
        public static void SetupEmptyPosition()
        {
            Ban.Reset();
        }
        
        public static void SetupStartingPosition()
        {
            PositionLoader.LoadPositionFromSFEN(PositionLoader.STARTING_POSITION);
        }

        public static void DrawBanInConsole(string source = null)
        {
            string log = "[DEBUG] Draw\n";
            if (source != null) log += " from " + source;
            DrawBanInDebugLog(ref log);
            Debug.Log(log);
        }

        private static void DrawBanInDebugLog(ref string log)
        {
            for (int rank = Ban.RANKS - 1; rank >= 0; --rank)
            {
                log += "     --------------\n";
                log += $"{rank + 1}  |   ";
                for (int file = 0; file < Ban.FILES; ++file)
                {
                    if (file > 0)
                    {
                        log += "   |   ";
                    }
                    char symbol = Decryptor.GetSymbolFromPiece(Ban.Grid[Ban.GetGridIndex(file, rank)]);
                    if (symbol == ' ')
                    {
                        log += " ";
                    }
                    log += symbol;
                }
                log += "  |\n";
            }
            log += "    ---------------\n";
            log += "        a        b       c  \n";
        }
    }
}