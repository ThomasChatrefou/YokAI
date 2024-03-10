using YokAI.GridProperties;
using YokAI.MoveProperties;
using YokAI.PieceProperties;

namespace YokAI.Main
{
    public class Ban
    {
        public const byte MAX_AVAILABLE_MOVES = 64;

        public PieceSet PieceSet { get; private set; }
        public Grid Grid { get; private set; }
        public uint PlayingColor { get; private set; }

        private uint[] _occupationBitboards;
        private uint[] _generatedMoves;
        private int _generatedMovesCount;

        public Ban()
        {
            Grid = new Grid();
            _generatedMoves = new uint[MAX_AVAILABLE_MOVES];
            Clear();
        }

        public void Clear()
        {
            PieceSet = new PieceSet(new uint[0]);
            Grid.Empty();
            PlayingColor = Color.NONE;

            _occupationBitboards = new uint[] { 0, 0 };
            _generatedMovesCount = 0;
        }

        public void Setup(PieceSet pieceSet, uint playingColor)
        {
            PieceSet = pieceSet;
            for (byte pieceId = 0; pieceId < PieceSet.Size; ++pieceId)
            {
                uint currentPiece = PieceSet[pieceId];
                byte cellId = Location.Get(currentPiece);
                Grid[cellId] = pieceId;
                Bitboard.Combine(ref _occupationBitboards[Color.Get(currentPiece) - 1], Bitboard.CreateSingle(cellId));
            }
            PlayingColor = playingColor;
            _generatedMovesCount = 0;
        }

        public void GenerateMoves()
        {
            _generatedMovesCount = 0;
            for (byte pieceId = 0; pieceId < PieceSet.Size; pieceId++)
            {
                uint currentPiece = PieceSet[pieceId];
                if (Color.Get(currentPiece) == PlayingColor)
                {
                    uint mobility = Mobility.Get(currentPiece);
                    byte startCellId = Location.Get(currentPiece);
                    uint geography = Geography.Get(startCellId);

                    uint occupiedCells = _occupationBitboards[PlayingColor - 1];
                    Bitboard.Combine(ref occupiedCells, with: mobility == Mobility.DROP ? _occupationBitboards[Color.GetOpponent(PlayingColor) - 1] : 0u);
                    Bitboard.AlignWithMobilityAtLocation(ref occupiedCells, startCellId);

                    uint reachableCells = Bitboard.FindReachableCells(occupiedCells, mobility & geography);

                    for (byte targetCellId = 0; targetCellId < Grid.SIZE; ++targetCellId)
                    {
                        if (Bitboard.Contains(reachableCells, targetCellId))
                        {
                            byte capturedPieceId = Occupation.Get(Grid[targetCellId]);
                            uint move = Move.Create(pieceId, capturedPieceId, startCellId, targetCellId
                                , isDrop: mobility == Mobility.DROP
                                , hasPromoted: Type.Get(currentPiece) == Type.PAWN && PlayingColor == PromotionZone.Get(targetCellId) && mobility != Mobility.DROP
                                , hasUnpromoted: Type.Get(PieceSet[capturedPieceId]) == Type.GOLD);

                            _generatedMoves[_generatedMovesCount++] = move;
                        }
                    }
                }
            }
        }

        public void MakeMove(uint move)
        {
            Move.Unpack(move
                , out byte movingPieceId, out byte capturedPieceId, out byte startCellId, out byte targetCellId
                , out bool isDrop, out bool hasPromoted, out bool hasUnpromoted);

            uint opponentColor = Color.GetOpponent(PlayingColor);
            ref uint movingPiece = ref PieceSet.GetRef(movingPieceId);
            ref uint capturedPiece = ref PieceSet.GetRef(capturedPieceId);
            ref uint startCell = ref Grid.GetCellRef(startCellId);
            ref uint targetCell = ref Grid.GetCellRef(targetCellId);
            ref uint playerOccupationBitboard = ref _occupationBitboards[PlayingColor - 1];
            ref uint opponentOccupationBitboard = ref _occupationBitboards[opponentColor - 1];

            Location.Set(ref movingPiece, targetCellId);
            Type.Set(ref movingPiece, hasPromoted ? Type.GOLD : Type.Get(movingPiece));
            Mobility.Set(ref movingPiece, MobilityByPiece.Get(Color.Get(movingPiece), Type.Get(movingPiece)));

            Bitboard.Move(ref playerOccupationBitboard, startCellId, targetCellId);

            Occupation.Set(ref startCell, Occupation.NONE);
            Occupation.Set(ref targetCell, movingPieceId);

            Location.Set(ref capturedPiece, Location.NONE);
            Color.Set(ref capturedPiece, PlayingColor);
            Type.Set(ref capturedPiece, hasUnpromoted ? Type.PAWN : Type.Get(capturedPiece));
            Mobility.Set(ref capturedPiece, Mobility.DROP);

            Bitboard.Capture(ref opponentOccupationBitboard, startCellId, targetCellId);

            Pass();
        }

        public void UnmakeMove(uint move)
        {
            Move.Unpack(move
                , out byte movingPieceId, out byte capturedPieceId, out byte startCellId, out byte targetCellId
                , out bool isDrop, out bool hasPromoted, out bool hasUnpromoted);

            uint opponentColor = Color.GetOpponent(PlayingColor);
            ref uint movingPiece = ref PieceSet.GetRef(movingPieceId);
            ref uint capturedPiece = ref PieceSet.GetRef(capturedPieceId);
            ref uint startCell = ref Grid.GetCellRef(startCellId);
            ref uint targetCell = ref Grid.GetCellRef(targetCellId);
            ref uint playerOccupationBitboard = ref _occupationBitboards[PlayingColor - 1];
            ref uint opponentOccupationBitboard = ref _occupationBitboards[opponentColor - 1];

            Location.Set(ref movingPiece, startCellId);
            Type.Set(ref movingPiece, hasPromoted ? Type.PAWN : Type.Get(movingPiece));
            Mobility.Set(ref movingPiece, isDrop ? Mobility.DROP : Mobility.Get(movingPiece));

            Bitboard.Move(ref playerOccupationBitboard, startCellId, targetCellId);

            Occupation.Set(ref startCell, movingPieceId);
            Occupation.Set(ref targetCell, capturedPieceId);

            Location.Set(ref capturedPiece, targetCellId);
            Color.Set(ref capturedPiece, opponentColor);
            Type.Set(ref capturedPiece, hasUnpromoted ? Type.GOLD : Type.Get(capturedPiece));
            Mobility.Set(ref capturedPiece, MobilityByPiece.Get(Color.Get(capturedPiece), Type.Get(capturedPiece)));

            Bitboard.Uncapture(ref opponentOccupationBitboard, startCellId, targetCellId);

            Pass();
        }

        public void Pass()
        {
            PlayingColor = Color.GetOpponent(PlayingColor);
        }
    }

    public class Grid
    {
        public const byte INVALID_CELL_ID = Location.NONE;

        public const byte FILES = 3;
        public const byte RANKS = 4;
        public const byte SIZE = FILES * RANKS;

        public uint[] Cells { get; private set; }

        public Grid()
        {
            Cells = new uint[SIZE];
        }

        public void Empty()
        {
            for (int i = 0; i < SIZE; ++i)
            {
                Cells[i] = Cell.EMPTY;
            }
        }

        public uint this[byte cellId]
        {
            get
            {
                if (cellId == INVALID_CELL_ID) return Cell.EMPTY;
                return Cells[cellId];
            }
            set
            {
                Cells[cellId] = value;
            }
        }

        public ref uint GetCellRef(byte cellId)
        {
            if (cellId == INVALID_CELL_ID) return ref _invalidCell;
            return ref Cells[cellId];
        }

        public static int GetIndex(int x, int y)
        {
            return y * FILES + x;
        }

        public static void GetCoordinates(int index, out int x, out int y)
        {
            x = index % FILES;
            y = index / FILES;
        }

        private static uint _invalidCell;
    }

    public struct PieceSet
    {
        public const byte INVALID_PIECE_ID = Occupation.NONE;
        public const byte MAX_PIECES_COUNT = Grid.SIZE;

        public uint[] Pieces { get; private set; }
        public int Size => Pieces.Length;

        public PieceSet(uint[] pieces)
        {
            Pieces = new uint[System.Math.Min(pieces.Length, MAX_PIECES_COUNT)];
            for (int i = 0; i < Pieces.Length; ++i)
            {
                Pieces[i] = pieces[i];
            }
        }

        public uint this[byte pieceId]
        {
            get
            {
                if (pieceId == INVALID_PIECE_ID) return Piece.INVALID;
                return Pieces[pieceId];
            }
            set
            {
                Pieces[pieceId] = value;
            }
        }

        public ref uint GetRef(byte pieceId)
        {
            if (pieceId == INVALID_PIECE_ID) return ref _invalidPiece;
            return ref Pieces[pieceId];
        }

        private static uint _invalidPiece;
    }
}

