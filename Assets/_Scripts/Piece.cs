namespace YokAI
{
    public static class Piece
    {
        
        public const int None = 0;
        public const int Pawn = 1;
        public const int Bishop = 2;
        public const int Rook = 3;
        public const int Gold = 4;
        public const int King = 5;
    }

    public static class Color
    {
        public const int White = 0;
        public const int Black = 1;
    }

    public class Move
    {
        public const int INVALID_MOVE = 0;

        public int MyTest()
        {
            int[] grid = new int[12];
            int next = 8;

            int previous = 5;
            int piece = grid[previous];

            int move = previous | next | Color.White | piece;
            return move;
        }
    }

}
