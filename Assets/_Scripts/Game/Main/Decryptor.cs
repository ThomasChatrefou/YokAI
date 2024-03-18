using System.Collections.Generic;
using YokAI.Properties;
using YokAI.Debugging;

namespace YokAI.Main
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
                [' '] = Type.NONE,
                ['p'] = Type.PAWN,
                ['b'] = Type.BISHOP,
                ['r'] = Type.ROOK,
                ['g'] = Type.GOLD,
                ['k'] = Type.KING
            };
            SymbolByPieceType = new Dictionary<uint, char>()
            {
                [Type.NONE] = ' ',
                [Type.PAWN] = 'p',
                [Type.BISHOP] = 'b',
                [Type.ROOK] = 'r',
                [Type.GOLD] = 'g',
                [Type.KING] = 'k',
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
                return '?';
            }

            uint color = Color.Get(piece);
            if (color == Color.WHITE)
            {
                result = char.ToUpper(result);
            }
            else if (color != Color.BLACK)
            {
                Logger.LogUnknownProperty(nameof(Decryptor));
                return '?';
            }

            return result;
        }

        public static string GetNotationFromMove(uint move, YokAIBan currentBan)
        {
            Move.Unpack(move
                , out byte movingPieceId, out byte capturedPieceId, out byte startCellId, out byte targetCellId
                , out bool isDrop, out bool hasPromoted, out bool hasUnpromoted);

            uint movingPiece = currentBan.PieceSet[movingPieceId];
            uint capturedPiece = currentBan.PieceSet[capturedPieceId];
            char movingPieceSymbol = GetSymbolFromPiece(movingPiece);
            char capturedPieceSymbol = GetSymbolFromPiece(capturedPiece);


            Grid.GetCoordinates(startCellId, out int startCellFile, out int startCellRank);
            Grid.GetCoordinates(targetCellId, out int targetCellFile, out int targetCellRank);

            char startCellFileAscii = (char)(startCellFile + ASCII_CODE_ALPHABET_START);
            char targetCellFileAscii = (char)(targetCellFile + ASCII_CODE_ALPHABET_START);

            string notation = string.Empty;

            notation += movingPieceSymbol;
            if (hasPromoted)
            {
                notation += '=';
                notation += Color.Get(movingPiece) == Color.WHITE ? 'G' : 'g';
            }
            if (!UseReducedNotation)
            {
                notation += startCellFileAscii;
                notation += $"{startCellRank + 1}";
            }

            if (isDrop)
            {
                notation += '*';
            }
            if (capturedPieceId != PieceSet.INVALID_PIECE_ID)
            {
                notation += 'x';
                if (!UseReducedNotation & UseCompleteNotation)
                {
                    if (capturedPieceSymbol != '?')
                    {
                        notation += capturedPieceSymbol;
                    }
                    if (hasUnpromoted)
                    {
                        notation += '=';
                        notation += Color.Get(capturedPiece) == Color.WHITE ? 'P' : 'p';
                    }
                }
            }
            notation += targetCellFileAscii;
            notation += $"{targetCellRank + 1}";

            // [ADD] '+' when Check 

            return notation;
        }
    }
}