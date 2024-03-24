using YokAI.Properties;

namespace YokAI.Main
{
    public struct YokAIBan
    {
        public const byte MAX_AVAILABLE_MOVES = 64;

        public PieceSet PieceSet { get; private set; }
        public Grid Grid { get; private set; }
        public uint PlayingColor { get; private set; }

        public uint OpponentColor => Color.GetOpponent(PlayingColor);

        public YokAIBan Copy()
        {
            YokAIBan newBan = new()
            {
                PieceSet = PieceSet.Copy(),
                Grid = Grid.Copy(),
                PlayingColor = PlayingColor,
                _occupationBitboards = new uint[] { 0, 0 },
                _generatedMoves = new uint[MAX_AVAILABLE_MOVES],
                _generatedMovesCount = 0,
                _kingIds = new byte[] { PieceSet.INVALID_PIECE_ID, PieceSet.INVALID_PIECE_ID },
            };
            _occupationBitboards.CopyTo(newBan._occupationBitboards, 0);
            _kingIds.CopyTo(newBan._kingIds, 0);
            return newBan;
        }

        public static YokAIBan Create(int pieceSetSize)
        {
            return new()
            {
                PieceSet = PieceSet.Create(pieceSetSize),
                Grid = Grid.Create(),
                PlayingColor = Color.NONE,
                _occupationBitboards = new uint[] { 0, 0 },
                _generatedMoves = new uint[MAX_AVAILABLE_MOVES],
                _generatedMovesCount = 0,
                _kingIds = new byte[] { PieceSet.INVALID_PIECE_ID, PieceSet.INVALID_PIECE_ID },
            };
        }

        public void Setup(uint[] pieceSet, uint playingColor)
        {
            for (byte pieceId = 0; pieceId < PieceSet.Size; ++pieceId)
            {
                PieceSet.GetRef(pieceId) = pieceSet[pieceId];
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

        #region Data Access

        public uint[] GetLastMoveGeneration()
        {
            uint[] result = new uint[_generatedMovesCount];
            System.Array.Copy(_generatedMoves, result, _generatedMovesCount);
            return result;
        }

        public bool IsInCheck(out bool isMultiple)
        {
            if (TryGetKing(PlayingColor, out byte kingId))
            {
                byte kingCellId = Location.Get(PieceSet[kingId]);
                byte checkingPieceId = Control.Get(Grid[kingCellId], OpponentColor);
                isMultiple = checkingPieceId == Control.MULTIPLE;
                return checkingPieceId != Control.NONE;
            }
            isMultiple = false;
            return false;
        }

        public bool IsOpponentKingOnPromotionZone()
        {
            if (TryGetKing(OpponentColor, out byte opponentKingId))
            {
                byte kingCellId = Location.Get(PieceSet[opponentKingId]);
                return PromotionZone.Get(kingCellId) == OpponentColor;
            }
            return false;
        }

        public bool TryGetKing(uint color, out byte kingId)
        {
            if (_kingIds.Length >= color && color != Color.NONE)
            {
                kingId = GetKing(color);
                return true;
            }
            kingId = PieceSet.INVALID_PIECE_ID;
            return false;
        }

        private byte GetKing(uint color)
        {
            return _kingIds[color - 1];
        }

        #endregion Data Access

        #region Move Generation

        public void GenerateMoves()
        {
            _generatedMovesCount = 0;

            if (IsInCheck(out bool isMultiple))
            {
                byte kingId = GetKing(PlayingColor);
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
                        CreateMove(pieceId, startCellId, checkingPieceCellId, mobility);
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
        
        private void GeneratePieceMoves(ref byte pieceId, ref uint movingPiece)
        {
            ComputePieceReachableCells(out byte startCellId, out uint mobility, out uint reachableCells, ref movingPiece, PlayingColor);

            for (byte targetCellId = 0; targetCellId < Grid.SIZE; ++targetCellId)
            {
                if (Bitboard.Contains(reachableCells, targetCellId))
                {
                    if (Type.Get(movingPiece) == Type.CHAD && Control.Get(Grid[targetCellId], OpponentColor) != Control.NONE)
                        continue;

                    CreateMove(pieceId, startCellId, targetCellId, mobility);
                }
            }
        }
        
        private void ComputePieceReachableCells(out byte startCellId, out uint mobility, out uint reachableCells, ref uint movingPiece, uint currentColor, bool useOccupation = true, bool noDrop = false)
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
        
        private void CreateMove(byte movingPieceId, byte startCellId, byte targetCellId, uint mobility)
        {
            byte capturedPieceId = Occupation.Get(Grid[targetCellId]);
            uint move = Move.Create(movingPieceId, capturedPieceId, startCellId, targetCellId
                , isDrop: mobility == Mobility.DROP
                , hasPromoted: Type.Get(PieceSet[movingPieceId]) == Type.PAWN && PlayingColor == PromotionZone.Get(targetCellId) && mobility != Mobility.DROP
                , hasUnpromoted: Type.Get(PieceSet[capturedPieceId]) == Type.GOLD);

            _generatedMoves[_generatedMovesCount++] = move;
        }

        #endregion Move Generation

        #region Move Making

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

            Pass();

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

            if (capturedPieceId != PieceSet.INVALID_PIECE_ID) // T_T
            {
                Bitboard.Uncapture(ref opponentOccupationBitboard, startCellId, targetCellId);
            }

            UpdateControl();
        }
        
        public void Pass()
        {
            PlayingColor = Color.GetOpponent(PlayingColor);
        }
        
        private void UpdateControl()
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

        #endregion Move Making

        private uint[] _occupationBitboards;
        private uint[] _generatedMoves;
        private int _generatedMovesCount;
        private byte[] _kingIds;
    }
}

