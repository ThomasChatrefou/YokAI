namespace YokAI.POC
{
    /// <summary>
    /// we identify pieces with their color and type : 
    /// cc tttttttt with c as color bits and t as type bits which also encodes how the piece moves :
    /// the 8 bits represents the 8 squares around the piece with the following Ban.Grid offsets : +4 +3 +2 +1 -1 -2 -3 -4
    /// For instance, a pawn on any square which is not in the last rank moves one square forward, which is stored 3 indexes further in the Ban.Grid list
    /// so we define it as the following binary : 0100 0000 (putting an empty space in the middle for easier reading and transposing to hexa)
    /// Then we also define the physical constraints of the board over the natural pieces moves, basically it is how the King would move on the edge.
    /// Finally, to know the real available moves of a piece, we just have to combine its natural move with the constraints of its position.
    /// </summary>
    public static class Piece
    {
        public const byte NONE   = 0;
        public const byte PAWN   = 0x40;    // 0100 0000 => 4 0
        public const byte BISHOP = 0xa5;    // 1010 0101 => a 5
        public const byte ROOK   = 0x5a;    // 0101 1010 => 5 a
        public const byte GOLD   = 0xfa;    // 1111 1010 => f a
        public const byte KING   = 0xff;    // 1111 1111 => f f

        public const ushort WHITE  = 0x100;     // 01 0000 0000
        public const ushort BLACK  = 0x200;     // 10 0000 0000

        public const ushort TYPE_FILTER = byte.MaxValue;    // 00 1111 1111
        public const ushort COLOR_FILTER = 0x300;           // 11 0000 0000

        public static readonly byte[] MOVE_CONSTRAINTS_ON_X_AXIS =
        {
            0xff,   // Middle : 1111 1111 => f f
            0x6b,   // East   : 0110 1011 => 6 b
            0xd6    // West   : 1101 0110 => d 6
        };

        public static readonly byte[] MOVE_CONSTRAINTS_ON_Y_AXIS =
        {
            0xff,   // Middle : 1111 1111 => f f
            0x1f,   // North  : 0001 1111 => 1 f
            0xf8    // South  : 1111 1000 => f 8
        };

        // 12 groups of 4 bits indicating positions on the board with respect to the edge. It is used as follows :
        // yyxx with xx the index in MOVE_CONSTRAINTS_ON_X_AXIS and yy the index in MOVE_CONSTRAINTS_ON_Y_AXIS
        public const ulong CONSTRAINT_MAP = 0x54610210298a;  // 0101 0100 0110 0001 0000 0010 0001 0000 0010 1001 1000 1010

        public static int GetPossibleMoves(int piece, int position)
        {
            int result = piece;

            byte dataSizeByCoord = 4;
            byte fourBitsFilter = 15;
            ulong currentConstraint = CONSTRAINT_MAP >> (position * dataSizeByCoord) & fourBitsFilter;

            ulong xIndex = currentConstraint & 3;
            byte boardXConstraint = MOVE_CONSTRAINTS_ON_X_AXIS[xIndex];
            result &= boardXConstraint;

            ulong yIndex = currentConstraint >> 2;
            byte boardYConstraint = MOVE_CONSTRAINTS_ON_Y_AXIS[yIndex];
            result &= boardYConstraint;

            return result;
        }

        public static int GetType(int piece)
        {
            return piece & TYPE_FILTER;
        }

        public static int GetColor(int piece)
        {
            return piece & COLOR_FILTER;
        }

        public static bool IsValid(int piece)
        {
            int pieceType = piece & TYPE_FILTER;
            int pieceColor = piece & COLOR_FILTER;

            return (pieceType == PAWN || pieceType == BISHOP || pieceType == ROOK || pieceType == GOLD || pieceType == KING)
                && (pieceColor == WHITE || pieceColor == BLACK);
        }
    }
}
