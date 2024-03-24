using YokAI.Properties;

namespace YokAI.Main
{
    public struct PieceSet
    {
        public const byte INVALID_PIECE_ID = Occupation.NONE;

        public int Size => _pieces.Length;

        public static PieceSet Create(int size)
        {
            return new() { _pieces = new uint[size] };
        }

        public PieceSet Copy()
        {
            PieceSet newSet = Create(Size);
            for (byte i = 0; i < Size; i++)
            {
                newSet[i] = _pieces[i];
            }
            return newSet;
        }

        public uint this[byte pieceId]
        {
            get
            {
                if (pieceId == INVALID_PIECE_ID) return Piece.INVALID;
                return _pieces[pieceId];
            }
            set
            {
                _pieces[pieceId] = value;
            }
        }

        public ref uint GetRef(byte pieceId)
        {
            if (pieceId == INVALID_PIECE_ID) return ref _invalidPiece;
            return ref _pieces[pieceId];
        }

        private static uint _invalidPiece;

        private uint[] _pieces;
    }
}