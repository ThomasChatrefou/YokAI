namespace YokAI
{
    /// <summary>
    /// we identify pieces with their color and type : 
    /// ccttt with c as color bit and t as type bit
    /// </summary>
    public static class Piece
    {
        public const int None = 0;
        public const int Pawn = 1;
        public const int Bishop = 2;
        public const int Rook = 3;
        public const int Gold = 4;
        public const int King = 5;

        public const int White = 8;
        public const int Black = 16;
    }

    public static class Ban
    {
        public static int[] Grid;
        public static int[] WhitePool;
        public static int[] BlackPool;

        static Ban()
        {
            Grid = new int[12];
            WhitePool = new int[6];
            BlackPool = new int[6];
        }
    }
}
