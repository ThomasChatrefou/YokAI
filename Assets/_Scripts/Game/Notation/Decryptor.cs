using System.Collections.Generic;
using YokAI.Properties;
using YokAI.Debugging;
using YokAI.Main;

namespace YokAI.Notation
{
    public static class Decryptor
    {
        public const int ASCII_CODE_ALPHABET_START = 97;

        public static Dictionary<char, uint> PieceTypeBySymbol;
        public static Dictionary<uint, char> SymbolByPieceType;

        public static bool UseCompleteNotation = false;
        public static bool UseReducedNotation = false;

        static Decryptor()
        {
            PieceTypeBySymbol = new Dictionary<char, uint>()
            {
                [Symbol.INVALID]    = Type.NONE,
                [Symbol.PAWN]       = Type.PAWN,
                [Symbol.BISHOP]     = Type.BISHOP,
                [Symbol.ROOK]       = Type.ROOK,
                [Symbol.GOLD]       = Type.GOLD,
                [Symbol.KING]       = Type.KING
            };
            SymbolByPieceType = new Dictionary<uint, char>()
            {
                [Type.NONE]     = Symbol.INVALID,
                [Type.PAWN]     = Symbol.PAWN,
                [Type.BISHOP]   = Symbol.BISHOP,
                [Type.ROOK]     = Symbol.ROOK,
                [Type.GOLD]     = Symbol.GOLD,
                [Type.KING]     = Symbol.KING,
            };
        }

        public static uint CreatePieceFromSymbol(char symbol)
        {
            uint color = char.IsUpper(symbol) ? Color.WHITE : Color.BLACK;
            if (!PieceTypeBySymbol.TryGetValue(char.ToLower(symbol), out uint type))
            {
                Logger.LogUnknownSymbol(nameof(Decryptor));
                return Piece.INVALID;
            }
            return Piece.Create(color, type);
        }

        public static char GetSymbolFromPiece(uint piece)
        {
            uint type = Type.Get(piece);
            if (!SymbolByPieceType.TryGetValue(type, out char result))
            {
                Logger.LogUnknownProperty(nameof(Decryptor));
                return Symbol.UNKNOWN;
            }

            uint color = Color.Get(piece);
            if (color == Color.WHITE)
            {
                result = char.ToUpper(result);
            }
            else if (color != Color.BLACK)
            {
                Logger.LogUnknownProperty(nameof(Decryptor));
                return Symbol.UNKNOWN;
            }

            return result;
        }

        public static string GetNotationFromMove(uint move, ref YokAIBan const_ban)
        {
            Move.Unpack(move
                , out byte movingPieceId, out byte capturedPieceId, out byte startCellId, out byte targetCellId
                , out bool isDrop, out bool hasPromoted, out bool hasUnpromoted);

            uint movingPiece = const_ban.PieceSet[movingPieceId];
            uint capturedPiece = const_ban.PieceSet[capturedPieceId];
            char movingPieceSymbol = GetSymbolFromPiece(movingPiece);

            Grid.GetCoordinates(startCellId, out int startCellFile, out int startCellRank);
            Grid.GetCoordinates(targetCellId, out int targetCellFile, out int targetCellRank);

            char startCellFileAscii = (char)(startCellFile + ASCII_CODE_ALPHABET_START);
            char targetCellFileAscii = (char)(targetCellFile + ASCII_CODE_ALPHABET_START);

            string notation = string.Empty;

            notation += movingPieceSymbol;
            if (hasPromoted)
            {
                notation += Symbol.PROMOTION;
                notation += Color.Get(movingPiece) == Color.WHITE ? char.ToUpper(Symbol.GOLD) : Symbol.GOLD;
            }
            if (!UseReducedNotation && startCellId != Grid.INVALID_CELL_ID)
            {
                notation += startCellFileAscii;
                notation += (startCellRank + 1).ToString();
            }

            if (isDrop)
            {
                notation += Symbol.DROP;
            }
            if (capturedPieceId != PieceSet.INVALID_PIECE_ID)
            {
                char capturedPieceSymbol = GetSymbolFromPiece(capturedPiece);
                notation += Symbol.CAPTURE;
                if (!UseReducedNotation & UseCompleteNotation)
                {
                    if (capturedPieceSymbol != Symbol.UNKNOWN)
                    {
                        notation += capturedPieceSymbol;
                    }
                    if (hasUnpromoted)
                    {
                        notation += Symbol.PROMOTION;
                        notation += Color.Get(capturedPiece) == Color.WHITE ? char.ToUpper(Symbol.PAWN) : Symbol.PAWN;
                    }
                }
            }
            notation += targetCellFileAscii;
            notation += (targetCellRank + 1).ToString();

            return notation;
        }
    }
}