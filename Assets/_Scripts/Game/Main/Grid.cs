
using YokAI.Properties;

namespace YokAI.Main
{
    public struct Grid
    {
        public const byte FILES = 3;
        public const byte RANKS = 4;
        public const byte SIZE = FILES * RANKS;
        public const byte INVALID_CELL_ID = Location.NONE;

        public static Grid Create()
        {
            Grid newGrid = new() { _cells = new uint[SIZE] };
            for (int i = 0; i < SIZE; ++i)
            {
                newGrid._cells[i] = Cell.EMPTY;
            }
            return newGrid;
        }

        public Grid Copy()
        {
            Grid newGrid = new() { _cells = new uint[SIZE] };
            for (byte i = 0; i < SIZE; i++)
            {
                newGrid[i] = _cells[i];
            }
            return newGrid;
        }

        public uint this[byte cellId]
        {
            get
            {
                if (cellId == INVALID_CELL_ID) return Cell.INVALID;
                return _cells[cellId];
            }
            set
            {
                _cells[cellId] = value;
            }
        }

        public ref uint GetCellRef(byte cellId)
        {
            if (cellId == INVALID_CELL_ID) return ref _invalidCell;
            return ref _cells[cellId];
        }

        public static byte GetCellId(int x, int y)
        {
            return (byte)(y * FILES + x);
        }

        public static void GetCoordinates(byte cellId, out int x, out int y)
        {
            x = cellId % FILES;
            y = cellId / FILES;
        }

        private static uint _invalidCell;

        private uint[] _cells;
    }
}
