namespace YokAI
{
    /// <summary>
    /// we identify pieces with their color and type : 
    /// ccttt with c as color bit and t as type bit
    /// </summary>
    public static class Piece
    {
        public const int NONE = 0;
        public const int PAWN = 1;
        public const int BISHOP = 2;
        public const int ROOK = 3;
        public const int GOLD = 4;
        public const int KING = 5;

        public const int WHITE = 8;
        public const int BLACK = 16;
    }
}
