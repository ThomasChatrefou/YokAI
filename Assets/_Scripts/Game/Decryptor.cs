using System.Collections.Generic;
using UnityEngine;

namespace YokAI.POC
{
    public static class Decryptor
    {
        public const int ASCII_CODE_ALPHABET_START = 97;

        public static Dictionary<char, int> PieceBySymbol;
        public static Dictionary<int, char> SymbolByPiece;

        static Decryptor()
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
            int type = piece & Piece.TYPE_FILTER;
            if (!SymbolByPiece.TryGetValue(type, out char result))
            {
                Debug.Log("Decrypting unknown piece type");
                return '0';
            }

            int color = piece & Piece.COLOR_FILTER;
            if (color == Piece.WHITE)
            {
                result = char.ToUpper(result);
            }
            else if (color != Piece.BLACK && piece != Piece.NONE)
            {
                Debug.Log("Decrypting unknown piece color");
                return '0';
            }

            return result;
        }

        public static string GetNotationFromMove(Move move)
        {
            if (move.Piece == Piece.NONE)
            {
                return "None";
            }

            char pieceSymbol = GetSymbolFromPiece(move.Piece);
            Vector2Int startSquare = Ban.GetCoordinates(move.StartSquare);
            Vector2Int targetSquare = Ban.GetCoordinates(move.TargetSquare);

            char startFile = (char)(startSquare.x + ASCII_CODE_ALPHABET_START);
            char targetFile = (char)(targetSquare.x + ASCII_CODE_ALPHABET_START);

            string notation = string.Empty;
            notation += pieceSymbol;
            notation += startFile;
            notation += $"{startSquare.y + 1}";
            notation += targetFile;
            notation += $"{targetSquare.y + 1}";
            return notation;
        }

        public static Move GetMoveFromNotation(string notation)
        {
            if (notation.Length == 5)
            {
                int piece = GetPieceFromSymbol(notation[0]);

                int startFile = notation[1] - ASCII_CODE_ALPHABET_START;
                int startRank = int.Parse(notation[2].ToString()) - 1;
                int startSquare = Ban.GetGridIndex(startFile, startRank);

                int targetFile = notation[3] - ASCII_CODE_ALPHABET_START;
                int targetRank = int.Parse(notation[4].ToString()) - 1;
                int targetSquare = Ban.GetGridIndex(targetFile, targetRank);

                return new Move(piece, startSquare, targetSquare);
            }
            return Ban.INVALID_MOVE;
        }
    }
}