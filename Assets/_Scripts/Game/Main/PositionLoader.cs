using System.Collections.Generic;
using YokAI.Properties;
using YokAI.Main;
using YokAI.Debugging;

public static class PositionLoader
{
    public const string YOKAI_NO_MORI_STARTING_POSITION = "rkb/1p1/1P1/BKR w - 1";
    public const string EMPTY_POSITION = "3/3/3/3 - - 0";

    public static void LoadPositionFromSFEN(ref YokAIBan ban, ref int moveNumber, string sfen)
    {
        ban.Clear();

        string[] splitedSfen = sfen.Split(" ");

        string banStr = splitedSfen[0];
        string playingColorStr = splitedSfen[1];
        string pools = splitedSfen[2];
        string moveNumberStr = splitedSfen[3];

        uint playingColor = playingColorStr == "w" ? Color.WHITE : (playingColorStr == "b" ? Color.BLACK : Color.NONE);
        if (playingColor == Color.NONE)
        {
            Logger.LogInvalidSFEN(nameof(PositionLoader));
            return;
        }

        int rank = Grid.RANKS - 1;

        List<uint> piecesList = new();

        string[] splitedBan = banStr.Split("/");
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

        if (pools != "-")
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
        PieceSet newPieceSet = new(piecesArray);

        ban.Setup(newPieceSet, playingColor);

        moveNumber = int.Parse(moveNumberStr);
    }

    public static string SavePositionToSFEN(YokAIBan ban, int moveNumber)
    {
        string sfen = string.Empty;

        string ranksSeparator = "/";
        string sfenElementsSeparator = " ";

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
                sfen += ranksSeparator;
            }
        }

        sfen += sfenElementsSeparator;
        sfen += ban.PlayingColor == Color.WHITE ? "w" : (ban.PlayingColor == Color.BLACK ? "b" : "-");

        sfen += sfenElementsSeparator;
        for (byte pieceId = 0; pieceId < ban.PieceSet.Size; pieceId++)
        {
            uint piece = ban.PieceSet[pieceId];
            if (Location.Get(piece) == Location.NONE)
            {
                sfen += Decryptor.GetSymbolFromPiece(piece);
            }
        }

        sfen += sfenElementsSeparator;
        sfen += moveNumber.ToString();

        return sfen;
    }
}
