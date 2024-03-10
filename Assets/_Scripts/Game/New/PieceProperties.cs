using System.Collections.Generic;

namespace YokAI.PieceProperties
{
    public static class MobilityByPiece
    {
        private static Dictionary<uint, uint> _table;

        static MobilityByPiece()
        {
            _table = new Dictionary<uint, uint>()
            {
                { Piece.Create(Color.WHITE, Type.PAWN), Mobility.PAWN },
                { Piece.Create(Color.BLACK, Type.PAWN), Mobility.PAWN_INV },
                { Piece.Create(Color.WHITE, Type.GOLD), Mobility.GOLD },
                { Piece.Create(Color.BLACK, Type.GOLD), Mobility.GOLD_INV },
                { Piece.Create(Color.WHITE, Type.BISHOP), Mobility.BISHOP },
                { Piece.Create(Color.BLACK, Type.BISHOP), Mobility.BISHOP },
                { Piece.Create(Color.WHITE, Type.ROOK), Mobility.ROOK },
                { Piece.Create(Color.BLACK, Type.ROOK), Mobility.ROOK },
                { Piece.Create(Color.WHITE, Type.KING), Mobility.KING },
                { Piece.Create(Color.BLACK, Type.KING), Mobility.KING }
            };
        }

        public static uint Get(uint color, uint type)
        {
            return _table[Piece.Create(color, type)];
        }
    }

    public static class Piece
    {
        public const uint INVALID = (Color.NONE << Color.SHIFT)
                                    | (Type.NONE << Type.SHIFT)
                                    | (Mobility.NONE << Mobility.SHIFT)
                                    | Location.NONE;

        public static uint Create(uint color, uint type, uint mobility, uint location)
        {
            return (color << Color.SHIFT) | (type << Type.SHIFT) | (mobility << Mobility.SHIFT) | location;
        }

        public static uint Create(uint color, uint type)
        {
            return Create(color, type, Mobility.NONE, Location.NONE);
        }
    }

    public static class Color
    {
        public const byte SHIFT = 19;
        public const uint FILTER = 3 << SHIFT;     // 11 000 000000000000 0000

        public const uint NONE = 0;
        public const uint WHITE = 1;
        public const uint BLACK = 2;

        public static uint Get(uint piece)
        {
            return (piece & FILTER) >> SHIFT;
        }

        public static void Set(ref uint piece, uint color)
        {
            piece &= ~FILTER;
            piece |= color << SHIFT;
        }

        public static uint GetOpponent(uint color)
        {
            return color ^ 3;
        }
    }

    public static class Type
    {
        public const byte SHIFT = 16;
        public const uint FILTER = 7 << SHIFT;     // 00 111 000000000000 0000

        public const uint NONE = 0;
        public const uint PAWN = 1;
        public const uint BISHOP = 2;
        public const uint ROOK = 3;
        public const uint GOLD = 4;
        public const uint KING = 5;

        public static uint Get(uint piece)
        {
            return (piece & FILTER) >> SHIFT;
        }

        public static void Set(ref uint piece, uint type)
        {
            piece &= ~FILTER;
            piece |= type << SHIFT;
        }
    }

    public static class Mobility
    {
        public const byte SHIFT = 4;
        public const uint FILTER = 0xfff << SHIFT;     // 00 000 111111111111 0000

        public const uint NONE = 0;
        public const uint PAWN = 0x080;       // 000 010 000 000
        public const uint PAWN_INV = 0x002;   // 000 000 000 010
        public const uint BISHOP = 0x145;     // 000 101 000 101
        public const uint ROOK = 0x0aa;       // 000 010 101 010
        public const uint GOLD = 0x1ea;       // 000 111 101 010
        public const uint GOLD_INV = 0x0af;   // 000 010 101 111
        public const uint KING = 0x1ef;       // 000 111 101 111
        public const uint DROP = 0xfff;       // 111 111 111 111

        public static uint Get(uint piece)
        {
            return (piece & FILTER) >> SHIFT;
        }

        public static void Set(ref uint piece, uint mobility)
        {
            piece &= ~FILTER;
            piece |= mobility << SHIFT;
        }
    }

    public static class Location
    {
        public const uint FILTER = 0xf;     // 00 000 000000000000 1111

        public const byte NONE = 0xf;

        public static byte Get(uint piece)
        {
            return (byte)(piece & FILTER);
        }

        public static void Set(ref uint piece, uint location)
        {
            piece &= ~FILTER;
            piece |= location;
        }
    }
}