using YokAI.Main;
using YokAI.Properties;

namespace YokAI.AI
{
    public static class MoveComparator
    {
        public static bool CompareMove(uint firstMove, uint secondMove)
        {
            return MoveScore(firstMove) < MoveScore(secondMove);
        }

        private static int MoveScore(uint move)
        {
            int score = 0;
            
            Move.Unpack(move
                , out byte movingPieceId, out byte capturedPieceId, out byte startCellId, out byte targetCellId
                , out bool isDrop, out bool hasPromoted, out bool hasUnpromoted);

            if (hasPromoted)
            {
                score += 20;
            }

            if (isDrop)
            {
                score += 10;
            }

            if (capturedPieceId != PieceSet.INVALID_PIECE_ID) // TODO : Evaluate Capture
            {
                score += 30;
            }


            return score;
        }
    }
}