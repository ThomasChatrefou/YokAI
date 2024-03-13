using System.Collections.Generic;
using YokAI.Main;
using YokAI.PieceProperties;
using YokAI.MoveProperties;

namespace YokAI.AI
{
    public static class AIController
    {
        public static int EvaluateCurrentPosition(int depth, Ban ban, out string bestMoveNotation)
        {
            bestMoveNotation = Decryptor.GetNotationFromMove(Evaluator.BestMove, ban);
            int eval = Evaluator.Search(depth, ref ban);
            return eval;
        }
    }

    public class Evaluator
    {
        public static uint BestMove { get; private set; } = Move.INVALID;

        public static int Search(int depth, ref Ban ban)
        {
            if (depth == 0)
            {
                return Evaluate(ref ban);
            }
            ban.GenerateMoves();

            int max = int.MinValue;
            uint[] availableMoves = ban.GetLastMoveGeneration();
            foreach (uint move in availableMoves)
            {
                ban.MakeMove(move);
                int score = -Search(depth - 1, ref ban);
                if (score > max)
                {
                    max = score;
                    BestMove = move;
                }
                ban.UnmakeMove(move);
            }
            return max;
        }

        public static int Evaluate(ref Ban ban)
        {
            int score = 0;
            for (byte pieceId = 0; pieceId < ban.PieceSet.Size; pieceId++)
            {
                uint piece = ban.PieceSet[pieceId];
                uint type = Type.Get(piece);
                uint color = Color.Get(piece);
                int coef = ban.PlayingColor == color ? 1 : -1;
                score += coef * PieceEvaluation.Get(type);
            }
            return score;
        }
    }

    public static class PieceEvaluation
    {
        public static int PAWN = 100;
        public static int BISHOP = 200;
        public static int ROOK = 200;
        public static int GOLD = 300;

        static PieceEvaluation()
        {
            _table = new()
            {
                { Type.PAWN, PAWN },
                { Type.BISHOP, BISHOP },
                { Type.ROOK, ROOK },
                { Type.GOLD, GOLD },
            };
        }

        public static int Get(uint pieceType)
        {
            if (!_table.ContainsKey(pieceType)) return 0;
            return _table[pieceType];
        }

        private static Dictionary<uint, int> _table;
    }
}