namespace YokAI.Properties
{
    public static class Bitboard
    {
        public static bool Contains(uint bitboard, byte cellId)
        {
            return ((bitboard >> cellId) & 1) == 1;
        }

        public static void Move(ref uint bitboard, byte startCellId, byte targetCellId)
        {
            bitboard ^= (1u << startCellId) | (1u << targetCellId);
        }

        public static void Capture(ref uint bitboard, byte startCellId, byte targetCellId)
        {
            uint bitboardMove = (1u << startCellId) | (1u << targetCellId);
            bitboard |= bitboardMove;
            bitboard ^= bitboardMove;
        }

        public static void Uncapture(ref uint bitboard, byte startCellId, byte targetCellId)
        {
            uint capturer = (1u << startCellId);
            bitboard ^= capturer | (1u << targetCellId);
            bitboard &= ~capturer;
        }

        public static void AlignWithMobilityAtLocation(ref uint bitboard, byte cellId)
        {
            bitboard <<= 4;
            bitboard >>= cellId;
        }

        public static void AlignBackWithGrid(ref uint bitboard, byte cellId)
        {
            bitboard <<= cellId;
            bitboard >>= 4;
        }

        public static uint FindReachableCells(uint bitboard, uint constraint)
        {
            return (constraint | bitboard) ^ bitboard;
        }

        public static void Combine(ref uint bitboard, uint with)
        {
            bitboard |= with;
        }

        public static uint Filter(uint bitboard, uint by)
        {
            return bitboard & by;
        }

        public static uint CreateSingle(byte cellId)
        {
            return 1u << cellId;
        }

        /* // Could be useful ?
        public static uint Add(uint bitboard, uint other)
        {
            return bitboard ^ other;
        }

        public static uint Reverse(uint bitboard)
        {
            return ~bitboard;
        }

        */
    }
}