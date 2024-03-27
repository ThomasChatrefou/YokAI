using System;
using _Scripts.Utilities;
using UnityEngine;
using YokAI.Main;
using YokAI.Properties;
using Color = YokAI.Properties.Color;
using Type = YokAI.Properties.Type;

namespace YokAI.AI
{
    public class AlphaBetaPruningEvaluator : Evaluator
    {
        public AlphaBetaPruningEvaluator(EvaluationParamSO evaluationParamSo)
        {
            _evaluationParamSo = evaluationParamSo;
        }

        public override int Search(int depth, ref YokAIBan ban, int alpha, int beta)
        {
            ++NbPositionReached;
            //TODO : this is temporary, yuk code
            ++AIManager.nbReachedPos;
            //end of yuk code
            if (depth == 0)
            {
                return Evaluate(ref ban);
            }

            ban.GenerateMoves();
            
            uint currentBestMove = Move.INVALID;

            uint[] availableMoves = ban.GetLastMoveGeneration();
            
            availableMoves.SortMoves(MoveComparator.CompareMove, ban);
            
            if (availableMoves.Length == 0)
            {
                BestMove = currentBestMove;
                if (ban.IsInCheck(out bool _))
                {
                    return -AIManager.maxEvalValue;
                }
                return 0;
            }
            
            foreach (uint move in availableMoves)
            {
                ban.MakeMove(move);
                int score = -Search(depth - 1, ref ban, -alpha, -beta);
                ban.UnmakeMove(move);

                if (score > beta)
                {
                    break;
                }
                
                if (score > alpha){ 
                    currentBestMove = move;
                }
                
                alpha = Mathf.Max(alpha, score);
                
                
                if (!CanRun)
                    break;
            }

            BestMove = currentBestMove;
            return alpha;
        }

        protected override int Evaluate(ref YokAIBan ban)
        {
            int score = 0;

            if (_evaluationParamSo.materialEval)
            {
                for (byte pieceId = 0; pieceId < ban.PieceSet.Size; pieceId++)
                {
                    uint piece = ban.PieceSet[pieceId];
                    uint type = Type.Get(piece);
                    uint color = Color.Get(piece);
                    int coef = ban.PlayingColor == color ? 1 : -1;
                    score += coef * PieceEvaluation.Get(type);
                }
            }

            return score;
        }
    }
}