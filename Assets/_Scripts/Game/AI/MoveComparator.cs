using YokAI.Main;
using YokAI.Properties;

namespace YokAI.AI
{
    public static class MoveComparator
    {
        public static bool CompareMove(uint firstMove, uint secondMove, YokAIBan ban)
        {
            return MoveScore(firstMove, ban) < MoveScore(secondMove, ban);
        }

        private static int MoveScore(uint move, YokAIBan ban)
        {
            int score = 0;
            
            Move.Unpack(move
                , out byte movingPieceId, out byte capturedPieceId, out byte startCellId, out byte targetCellId
                , out bool isDrop, out bool hasPromoted, out bool hasUnpromoted);

            if (hasPromoted)
            {
                score += 200;
            }

            if (isDrop)
            {
                score += 100;
            }

            if (capturedPieceId != PieceSet.INVALID_PIECE_ID)
            {
                score += 300 + 4 * GetPieceValue(capturedPieceId, ban) - GetPieceValue(movingPieceId, ban);
            }

            byte controllingPieceId = Control.Get(ban.Grid[targetCellId], ban.OpponentColor);
            
            switch (controllingPieceId)
            {
                case Control.NONE:
                    score += 50;
                    break;
                case Control.MULTIPLE:
                    score -= 50;
                    break;
            }

            return score;
        }

        private static int GetPieceValue(byte pieceId, YokAIBan ban)
        {
            uint piece = ban.PieceSet[pieceId];
            uint pieceType = Type.Get(piece);
            int pieceValue = PieceEvaluation.Get(pieceType);

            return pieceValue;
        }
    }
}