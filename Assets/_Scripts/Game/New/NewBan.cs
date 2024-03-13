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
        public uint OpponentColor { get { return Color.GetOpponent(PlayingColor); } }
        public byte[] KingIds { get { return _kingIds; } }

        private uint[] _occupationBitboards;
        private uint[] _generatedMoves;
        private int _generatedMovesCount;

        private byte[] _kingIds;

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
            _kingIds = new byte[2] { PieceSet.INVALID_PIECE_ID, PieceSet.INVALID_PIECE_ID };
        }

        public void Setup(PieceSet pieceSet, uint playingColor)
        {
            PieceSet = pieceSet;
            for (byte pieceId = 0; pieceId < PieceSet.Size; ++pieceId)
            {
                uint currentPiece = PieceSet[pieceId];
                byte cellId = Location.Get(currentPiece);
                uint color = Color.Get(currentPiece);
                Occupation.Set(ref Grid.GetCellRef(cellId), pieceId);
                Bitboard.Combine(ref _occupationBitboards[color - 1], Bitboard.CreateSingle(cellId));
                if (Type.Get(currentPiece) == Type.KING)
                {
                    _kingIds[color - 1] = pieceId;
                }
            }
            PlayingColor = playingColor;
            UpdateControl();
            _generatedMovesCount = 0;
        }

        public bool IsInCheck(out bool isMultiple)
        {
            byte kingId = _kingIds[PlayingColor - 1];
            byte kingCellId = Location.Get(PieceSet[kingId]);
            byte checkingPieceId = Control.Get(Grid[kingCellId], OpponentColor);
            isMultiple = checkingPieceId == Control.MULTIPLE;
            return checkingPieceId != Control.NONE;
        }

        public bool IsOpponentKingOnPromotionZone()
        {
            byte opponentKingId = _kingIds[OpponentColor - 1];
            byte kingCellId = Location.Get(PieceSet[opponentKingId]);
            return PromotionZone.Get(kingCellId) == OpponentColor;
        }

        public void GenerateMoves()
        {
            _generatedMovesCount = 0;

            if (IsInCheck(out bool isMultiple))
            {
                byte kingId = _kingIds[PlayingColor - 1];
                GeneratePieceMoves(ref kingId, ref PieceSet.GetRef(kingId));

                if (isMultiple) return;

                for (byte pieceId = 0; pieceId < PieceSet.Size; pieceId++)
                {
                    if (pieceId == kingId) continue;

                    uint currentPiece = PieceSet[pieceId];
                    if (Color.Get(currentPiece) != PlayingColor) continue;
                    
                    ComputePieceReachableCells(out byte startCellId, out uint mobility, out uint reachableCells, ref currentPiece, PlayingColor);

                    byte kingCellId = Location.Get(PieceSet[kingId]);
                    byte checkingPieceId = Control.Get(Grid[kingCellId], OpponentColor);
                    byte checkingPieceCellId = Location.Get(PieceSet[checkingPieceId]);

                    if (Bitboard.Contains(reachableCells, checkingPieceCellId))
                    {
                        GenerateMove(pieceId, startCellId, checkingPieceCellId, mobility);
                    }
                }
            }
            else
            {
                for (byte pieceId = 0; pieceId < PieceSet.Size; pieceId++)
                {
                    uint currentPiece = PieceSet[pieceId];
                    if (Color.Get(currentPiece) == PlayingColor)
                    {
                        GeneratePieceMoves(ref pieceId, ref currentPiece);
                    }
                }
            }
        }

        public void GeneratePieceMoves(ref byte pieceId, ref uint movingPiece)
        {
            ComputePieceReachableCells(out byte startCellId, out uint mobility, out uint reachableCells, ref movingPiece, PlayingColor);

            for (byte targetCellId = 0; targetCellId < Grid.SIZE; ++targetCellId)
            {
                if (Bitboard.Contains(reachableCells, targetCellId))
                {
                    if (Type.Get(movingPiece) == Type.CHAD && Control.Get(Grid[targetCellId], OpponentColor) != Control.NONE)
                        continue;

                    GenerateMove(pieceId, startCellId, targetCellId, mobility);
                }
            }
        }

        public void ComputePieceReachableCells(out byte startCellId, out uint mobility, out uint reachableCells, ref uint movingPiece, uint currentColor, bool useOccupation = true, bool noDrop = false)
        {
            startCellId = Location.Get(movingPiece);
            mobility = Mobility.Get(movingPiece);

            uint geography = Geography.Get(startCellId);
            uint constrainedMobility = Bitboard.Filter(mobility, by: geography);

            uint occupiedCells = useOccupation ? _occupationBitboards[currentColor - 1] : 0u;

            if (mobility == Mobility.DROP)
            {
                if (noDrop)
                {
                    reachableCells = 0;
                    return;
                }

                Bitboard.Combine(ref occupiedCells, with: _occupationBitboards[Color.GetOpponent(currentColor) - 1]);
                reachableCells = Bitboard.FindReachableCells(occupiedCells, constrainedMobility);
            }
            else
            {
                Bitboard.AlignWithMobilityAtLocation(ref occupiedCells, startCellId);
                reachableCells = Bitboard.FindReachableCells(occupiedCells, constrainedMobility);
                Bitboard.AlignBackWithGrid(ref reachableCells, startCellId);
            }
        }

        public void GenerateMove(byte movingPieceId, byte startCellId, byte targetCellId, uint mobility)
        {
            byte capturedPieceId = Occupation.Get(Grid[targetCellId]);
            uint move = Move.Create(movingPieceId, capturedPieceId, startCellId, targetCellId
                , isDrop: mobility == Mobility.DROP
                , hasPromoted: Type.Get(PieceSet[movingPieceId]) == Type.PAWN && PlayingColor == PromotionZone.Get(targetCellId) && mobility != Mobility.DROP
                , hasUnpromoted: Type.Get(PieceSet[capturedPieceId]) == Type.GOLD);

            _generatedMoves[_generatedMovesCount++] = move;
        }

        public void UpdateControl()
        {
            for (byte targetCellId = 0; targetCellId < Grid.SIZE; ++targetCellId)
            {
                Control.Set(ref Grid.GetCellRef(targetCellId), Control.NONE, PlayingColor);
                Control.Set(ref Grid.GetCellRef(targetCellId), Control.NONE, OpponentColor);
            }

            for (byte pieceId = 0; pieceId < PieceSet.Size; pieceId++)
            {
                uint currentPiece = PieceSet[pieceId];
                uint currentColor = Color.Get(currentPiece);

                ComputePieceReachableCells(out byte _, out uint _, out uint reachableCells, ref currentPiece, currentColor, useOccupation: false, noDrop: true);

                for (byte targetCellId = 0; targetCellId < Grid.SIZE; ++targetCellId)
                {
                    byte controllingPieceId = Control.Get(Grid[targetCellId], currentColor);

                    if (Bitboard.Contains(reachableCells, targetCellId))
                    {
                        if (controllingPieceId == Control.NONE)
                        {
                            Control.Set(ref Grid.GetCellRef(targetCellId), pieceId, currentColor);
                        }
                        else if (controllingPieceId != Control.MULTIPLE)
                        {
                            Control.Set(ref Grid.GetCellRef(targetCellId), Control.MULTIPLE, currentColor);
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
            
            UpdateControl();
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

            UpdateControl();
            Pass();
        }

        public void Pass()
        {
            PlayingColor = Color.GetOpponent(PlayingColor);
        }

        public uint[] GetLastMoveGeneration()
        {
            uint[] result = new uint[_generatedMovesCount];
            System.Array.Copy(_generatedMoves, result, _generatedMovesCount);
            return result;
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
                if (cellId == INVALID_CELL_ID) return Cell.INVALID;
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

