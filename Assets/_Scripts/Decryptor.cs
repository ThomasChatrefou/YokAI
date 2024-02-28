using System.Collections.Generic;
using UnityEngine;

namespace YokAI
{
    public static class Decrpytor
    {
        public const int TYPE_FILTER = 7;
        public const int COLOR_FILTER = 24;

        public static Dictionary<char, int> PieceBySymbol;
        public static Dictionary<int, char> SymbolByPiece;

        static Decrpytor()
        {
            PieceBySymbol = new Dictionary<char, int>()
            {
                [' '] = Piece.NONE,
                ['p'] = Piece.PAWN,
                ['b'] = Piece.BISHOP,
                ['r'] = Piece.ROOK,
                ['g'] = Piece.GOLD,
                ['k'] = Piece.KING
            };
            SymbolByPiece = new Dictionary<int, char>()
            {
                [Piece.NONE] = ' ',
                [Piece.PAWN] = 'p',
                [Piece.BISHOP] = 'b',
                [Piece.ROOK] = 'r',
                [Piece.GOLD] = 'g',
                [Piece.KING] = 'k',
            };
        }

        public static int GetPieceFromSymbol(char symbol)
        {
            int color = char.IsUpper(symbol) ? Piece.WHITE : Piece.BLACK;
            if (!PieceBySymbol.TryGetValue(char.ToLower(symbol), out int type))
            {
                Debug.Log("Decrypting unkown symbol");
                return Piece.NONE;
            }
            return color | type;
        }

        public static char GetSymbolFromPiece(int piece)
        {
            int type = piece & TYPE_FILTER;
            if (!SymbolByPiece.TryGetValue(type, out char result))
            {
                Debug.Log("Decrypting unkown piece type");
                return '0';
            }

            int color = piece & COLOR_FILTER;
            if (color == Piece.WHITE)
            {
                result = char.ToUpper(result);
            }
            else if (color != Piece.BLACK && piece != Piece.NONE)
            {
                Debug.Log("Decrypting unkown piece color");
                return '0';
            }

            return result;
        }
    }
}