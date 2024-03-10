using UnityEngine;

/// <summary>
/// We use SFEN notation to represent a position :
/// 
/// </summary>

namespace YokAI.POC
{
    public static class PositionLoader
    {
        public const string STARTING_POSITION = "rkb/1p1/1P1/BKR w - 1";

        public static void LoadPositionFromSFEN(string sfen)
        {
            Ban.Reset();
            string[] splitedSfen = sfen.Split(" ");

            string ban = splitedSfen[0];
            string playingColor = splitedSfen[1];
            string pools = splitedSfen[2];
            string moveNumber = splitedSfen[3];

            int rank = Ban.RANKS - 1;

            string[] splitedBan = ban.Split("/");
            foreach (string rankSfen in splitedBan)
            {
                int file = 0;
                foreach (char symbol in rankSfen)
                {
                    if (char.IsDigit(symbol))
                    {
                        file += int.Parse(symbol.ToString());
                    }
                    else
                    {
                        int gridIndex = Ban.GetGridIndex(file, rank);
                        int piece = Decryptor.GetPieceFromSymbol(symbol);
                        Ban.Grid[gridIndex] = piece;
                        Ban.PiecesBitboards[(Piece.GetColor(piece) >> 8) - 1] |= 1 << gridIndex;

                        ++file;
                    }
                }
                --rank;
            }

            if (pools != "-")
            {
                int whitePoolIndex = 0;
                int blackPoolIndex = 0;
                foreach (char symbol in pools)
                {
                    int piece = Decryptor.GetPieceFromSymbol(symbol);
                    if (char.IsUpper(symbol))
                    {
                        Ban.WhitePool[whitePoolIndex++] = piece;
                    }
                    else
                    {
                        Ban.BlackPool[blackPoolIndex++] = piece;
                    }
                }
            }

            Ban.PlayingColor = playingColor == "w" ? Piece.WHITE : (playingColor == "b" ? Piece.BLACK : Piece.NONE);
            if (Ban.PlayingColor == Piece.NONE)
            {
                Debug.LogWarning("Invalid SFEN : unknown playing color. Should be 'w' or 'b' !");
                return;
            }

            Ban.MoveNumber = int.Parse(moveNumber);
        }
    }
}