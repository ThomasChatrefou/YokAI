using System.Collections.Generic;
using YokAI.Properties;
using YokAI.Main;
using YokAI.Debugging;

namespace YokAI.Notation
{
    public static class PositionLoader
    {
        public const string YOKAI_NO_MORI_STARTING_POSITION = "rkb/1p1/1P1/BKR w - 1";
        public const string EMPTY_POSITION = "3/3/3/3 - - 0";

        public const string SFEN_SEPARATOR = " ";
        public const string RANK_SEPARATOR = "/";

        public static void LoadPositionFromSFEN(ref YokAIBan trans_ban, ref int moveNumber, string sfen)
        {
            string[] splitedSfen = sfen.Split(SFEN_SEPARATOR);

            string banStr = splitedSfen[0];
            string playingColorSymbol = splitedSfen[1];
            string pools = splitedSfen[2];
            string moveNumberStr = splitedSfen[3];

            uint playingColor =
                playingColorSymbol == Symbol.WHITE.ToString() ? Color.WHITE : (
                playingColorSymbol == Symbol.BLACK.ToString() ? Color.BLACK : Color.NONE);

            if (playingColor == Color.NONE)
            {
                Logger.LogInvalidSFEN(nameof(PositionLoader));
                return;
            }

            int rank = Grid.RANKS - 1;

            List<uint> piecesList = new();

            string[] splitedBan = banStr.Split(RANK_SEPARATOR);
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
                        byte cellId = Grid.GetCellId(file, rank);
                        uint newPiece = Decryptor.CreatePieceFromSymbol(symbol);
                        Mobility.Set(ref newPiece, MobilityByPiece.Get(Color.Get(newPiece), Type.Get(newPiece)));
                        Location.Set(ref newPiece, cellId);
                        piecesList.Add(newPiece);
                        ++file;
                    }
                }
                --rank;
            }

            if (pools != Symbol.EMPTY.ToString())
            {
                foreach (char symbol in pools)
                {
                    uint piece = Decryptor.CreatePieceFromSymbol(symbol);
                    Mobility.Set(ref piece, Mobility.DROP);
                    Location.Set(ref piece, Grid.INVALID_CELL_ID);
                }
            }

            uint[] piecesArray = new uint[piecesList.Count];
            piecesList.CopyTo(piecesArray);

            trans_ban = YokAIBan.Create(piecesArray.Length);
            trans_ban.Setup(piecesArray, playingColor);

            moveNumber = int.Parse(moveNumberStr);
        }

        public static string SavePositionToSFEN(YokAIBan ban, int moveNumber)
        {
            string sfen = string.Empty;

            for (int rank = Grid.RANKS - 1; rank >= 0; rank--)
            {
                int emptyCellsCount = 0;
                for (int file = 0; file < Grid.FILES; file++)
                {
                    byte cellId = Grid.GetCellId(file, rank);
                    uint cell = ban.Grid[cellId];
                    if (Occupation.Get(cell) == Occupation.NONE)
                    {
                        ++emptyCellsCount;
                    }
                    else
                    {
                        if (emptyCellsCount > 0)
                        {
                            sfen += emptyCellsCount.ToString();
                            emptyCellsCount = 0;
                        }
                        byte pieceId = Occupation.Get(cell);
                        uint piece = ban.PieceSet[pieceId];
                        sfen += Decryptor.GetSymbolFromPiece(piece);
                    }
                }

                if (rank != 0)
                {
                    sfen += RANK_SEPARATOR;
                }
            }

            sfen += SFEN_SEPARATOR;
            sfen += ban.PlayingColor == Color.WHITE ? Symbol.WHITE : (ban.PlayingColor == Color.BLACK ? Symbol.BLACK : Symbol.INVALID);

            sfen += SFEN_SEPARATOR;
            for (byte pieceId = 0; pieceId < ban.PieceSet.Size; pieceId++)
            {
                uint piece = ban.PieceSet[pieceId];
                if (Location.Get(piece) == Location.NONE)
                {
                    sfen += Decryptor.GetSymbolFromPiece(piece);
                }
            }

            sfen += SFEN_SEPARATOR;
            sfen += moveNumber.ToString();

            return sfen;
        }
    }
}
