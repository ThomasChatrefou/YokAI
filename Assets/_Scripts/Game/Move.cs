using System.Collections.Generic;

namespace YokAI
{
    public readonly struct Move
    {
        public readonly int Piece;
        public readonly int StartSquare;
        public readonly int TargetSquare;

        public Move(int piece, int startSquare, int targetSquare)
        {
            Piece = piece;
            StartSquare = startSquare;
            TargetSquare = targetSquare;
        }
    }

    public class MoveGenerator
    {
        public static List<Move> Moves;

        public static List<Move> GenerateMoves()
        {
            Moves = new List<Move>();

            for (int square = 0; square < Ban.Size; ++square)
            {
                int piece = Ban.Grid[square];
                if (Piece.GetColor(piece) == Ban.PlayingColor)
                {
                    int currentPieceMoves = Piece.GetPossibleMoves(piece, square);

                    int availableSquares = Ban.GetPieceBitboard(Ban.PlayingColor);
                    int shift = square - 4;
                    if (shift > 0)
                    {
                        availableSquares >>= shift;
                    }
                    else if (shift < 0)
                    {
                        availableSquares <<= -shift;
                    }
                    availableSquares = ~availableSquares;

                    int bitIndex = -4;
                    AddNextPossibleMoves(ref bitIndex, ref currentPieceMoves, ref availableSquares, piece, square);
                    ++bitIndex;
                    availableSquares >>= 1;
                    AddNextPossibleMoves(ref bitIndex, ref currentPieceMoves, ref availableSquares, piece, square);
                }
            }

            return Moves;
        }

        private static void AddNextPossibleMoves(ref int bitIndex, ref int currentPieceMoves, ref int availableSquares, int piece, int square)
        {
            int stopIndex = bitIndex + 4;
            for (; bitIndex < stopIndex; ++bitIndex)
            {
                if ((currentPieceMoves & availableSquares & 1) == 1)
                {
                    Moves.Add(new Move(piece, square, square + bitIndex));
                }
                currentPieceMoves >>= 1;
                availableSquares >>= 1;
            }
        }
    }
}

