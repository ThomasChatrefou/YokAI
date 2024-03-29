namespace YokAI.Properties
{
    public static class Cell
    {
        public const uint INVALID = Occupation.NONE | (Control.NONE << Control.SHIFT) | (Control.NONE << (2 * Control.SHIFT));
        public const uint EMPTY = Occupation.NONE | (Control.NONE << Control.SHIFT) | (Control.NONE << (2 * Control.SHIFT));

        public static uint Create(uint occupation)
        {
            return occupation | (Control.NONE << Control.SHIFT) | (Control.NONE << (2 * Control.SHIFT));
        }
    }

    public static class Occupation
    {
        public const uint FILTER = 0xf;

        public const byte NONE = 0xf;

        public static byte Get(uint cell)
        {
            return (byte)(cell & FILTER);
        }

        public static void Set(ref uint cell, uint pieceId)
        {
            cell &= ~FILTER;
            cell |= pieceId;
        }
    }

    public static class Control
    {
        public const byte SHIFT = 4;
        public const uint FILTER = 0xf;

        public const byte MULTIPLE = 0xe;
        public const byte NONE = 0xf;

        public static byte Get(uint cell, uint color)
        {
            byte shift = (byte)(color * SHIFT);
            return (byte)((cell & (FILTER << shift)) >> shift);
        }

        public static void Set(ref uint cell, uint pieceId, uint color)
        {
            byte shift = (byte)(color * SHIFT);
            cell &= ~(FILTER << shift);
            cell |= pieceId << shift;
        }
    }

    // Static data
    public static class Geography
    {
        public static readonly uint[] EAST_WEST_CONSTRAINTS =
        {
                0xfff,   // Middle : 111 111 111 111
                0x0db,   // East   : 000 011 011 011
                0x1b6    // West   : 000 110 110 110
        };
        public static readonly uint[] NORTH_SOUTH_CONSTRAINTS =
        {
                0xfff,   // Middle : 111 111 111 111
                0x03f,   // North  : 000 000 111 111
                0x1f8    // South  : 000 111 111 000
        };

        // 12 groups of 4 bits indicating positions on the board with respect to the edge. It is used as follows :
        // yyxx with xx the index in EAST_WEST_CONSTRAINTS and yy the index in NORTH_SOUTH_CONSTRAINTS
        public const ulong MAP = 0x54610210298a;  // 0101 0100 0110 | 0001 0000 0010 | 0001 0000 0010 | 1001 1000 1010

        public const byte BITS_BY_CELL = 4;
        public const byte CELL_FILTER = 0xf;

        public static uint Get(byte cellId)
        {
            ulong cellGeography = MAP >> (cellId * BITS_BY_CELL) & CELL_FILTER;
            uint boardXConstraints = EAST_WEST_CONSTRAINTS[cellGeography & 3];
            uint boardYConstraints = NORTH_SOUTH_CONSTRAINTS[cellGeography >> 2];

            return boardXConstraints & boardYConstraints;
        }
    }

    // Static data : Associated with PieceProperties Color
    public static class PromotionZone
    {
        public const uint MAP = 0x54002a;  // 01 01 01 | 00 00 00 | 00 00 00 | 10 10 10

        public const byte BITS_BY_CELL = 2;
        public const byte CELL_FILTER = 3;

        public static uint Get(byte cellId)
        {
            return MAP >> (cellId * BITS_BY_CELL) & CELL_FILTER;
        }
    }
}