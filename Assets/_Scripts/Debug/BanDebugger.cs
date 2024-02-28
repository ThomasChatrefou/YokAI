using UnityEngine;

namespace YokAI
{
    public static class BanDebugger
    {
        public static void DebugLog()
        {
            string log = "BAN DEBUG LOG (click to see details)\n";
            log += "Grid :\n";
            for (int rank = Ban.RANKS - 1; rank >= 0; --rank)
            {
                log += "---------\n";
                log += "| ";
                for (int file = 0; file < Ban.FILES; ++file)
                {
                    if (file > 0)
                    {
                        log += " | ";
                    }
                    log += Decrpytor.GetSymbolFromPiece(Ban.Grid[Ban.GetGridIndex(file, rank)]);
                }
                log += " |\n";
            }
            log += "---------\n";
            log += $"White Pool : ";
            foreach (int piece in Ban.WhitePool)
            {
                log += Decrpytor.GetSymbolFromPiece(piece) + " ";
            }
            log += "\n";
            log += $"Black Pool : ";
            foreach (int piece in Ban.BlackPool)
            {
                log += Decrpytor.GetSymbolFromPiece(piece) + " ";
            }
            log += "\n";
            log += $"Playing Color : " + (Ban.PlayingColor == Piece.WHITE ? "White" : (Ban.PlayingColor == Piece.BLACK ? "Black" : "Unknown")) + "\n";
            log += $"Move Number : {Ban.MoveNumber}\n";
            Debug.Log(log);
        }
    }
}