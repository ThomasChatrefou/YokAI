namespace YokAI.Properties
{
    public static class Move
    {
        public const uint INVALID = uint.MaxValue;

        public static uint Create(byte movingPieceId, byte capturedPieceId, byte startCellId, byte targetCellId, bool isDrop, bool hasPromoted, bool hasUnpromoted)
        {
            uint move = 0u;
            MovingPiece.Set(ref move, movingPieceId);
            CapturedPiece.Set(ref move, capturedPieceId);
            StartCell.Set(ref move, startCellId);
            TargetCell.Set(ref move, targetCellId);
            Drop.Set(ref move, isDrop);
            Promote.Set(ref move, hasPromoted);
            Unpromote.Set(ref move, hasUnpromoted);
            return move;

        }

        public static void Unpack(uint move, out byte movingPieceId, out byte capturedPieceId, out byte startCellId, out byte targetCellId, out bool isDrop, out bool hasPromoted, out bool hasUnpromoted)
        {
            movingPieceId = MovingPiece.Get(move);
            capturedPieceId = CapturedPiece.Get(move);
            startCellId = StartCell.Get(move);
            targetCellId = TargetCell.Get(move);
            isDrop = Drop.Get(move);
            hasPromoted = Promote.Get(move);
            hasUnpromoted = Unpromote.Get(move);
        }
    }

    public static class MovingPiece
    {
        public const uint FILTER = 0xf;               // 0 0 0 0000 0000 0000 1111

        public static byte Get(uint move)
        {
            return (byte)(move & FILTER);
        }

        public static void Set(ref uint move, uint pieceId)
        {
            move &= ~FILTER;
            move |= pieceId;
        }
    }

    public static class CapturedPiece
    {
        public const byte SHIFT = 4;
        public const uint FILTER = 0xf << SHIFT;      // 0 0 0 0000 0000 1111 0000

        public static byte Get(uint move)
        {
            return (byte)((move & FILTER) >> SHIFT);
        }

        public static void Set(ref uint move, uint pieceId)
        {
            move &= ~FILTER;
            move |= pieceId << SHIFT;
        }
    }

    public static class StartCell
    {
        public const byte SHIFT = 8;
        public const uint FILTER = 0xf << SHIFT;      // 0 0 0 0000 1111 0000 0000

        public static byte Get(uint move)
        {
            return (byte)((move & FILTER) >> SHIFT);
        }

        public static void Set(ref uint move, uint cellId)
        {
            move &= ~FILTER;
            move |= cellId << SHIFT;
        }
    }

    public static class TargetCell
    {
        public const byte SHIFT = 12;
        public const uint FILTER = 0xf << SHIFT;      // 0 0 0 1111 0000 0000 0000

        public static byte Get(uint move)
        {
            return (byte)((move & FILTER) >> SHIFT);
        }

        public static void Set(ref uint move, uint cellId)
        {
            move &= ~FILTER;
            move |= cellId << SHIFT;
        }
    }

    public static class Drop
    {
        public const byte SHIFT = 16;
        public const uint FILTER = 1 << SHIFT;      // 0 0 1 0000 0000 0000 0000

        public static bool Get(uint move)
        {
            return ((move & FILTER) >> SHIFT) == 1;
        }

        public static void Set(ref uint move, bool doDrop)
        {
            move &= ~FILTER;
            move |= (doDrop ? 1u : 0u) << SHIFT;
        }
    }

    public static class Promote
    {
        public const byte SHIFT = 17;
        public const uint FILTER = 1 << SHIFT;      // 0 1 0 0000 0000 0000 0000

        public static bool Get(uint move)
        {
            return ((move & FILTER) >> SHIFT) == 1;
        }

        public static void Set(ref uint move, bool doPromote)
        {
            move &= ~FILTER;
            move |= (doPromote ? 1u : 0u) << SHIFT;
        }
    }

    public static class Unpromote
    {
        public const byte SHIFT = 18;
        public const uint FILTER = 1 << SHIFT;      // 1 0 0 0000 0000 0000 0000

        public static bool Get(uint move)
        {
            return ((move & FILTER) >> SHIFT) == 1;
        }

        public static void Set(ref uint move, bool doUnpromote)
        {
            move &= ~FILTER;
            move |= (doUnpromote ? 1u : 0u) << SHIFT;
        }
    }
}