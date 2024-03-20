using System.Collections.Generic;
using YokAI.Properties;

namespace YokAI.AI
{ 
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