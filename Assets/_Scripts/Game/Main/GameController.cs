using YokAI.Properties;
using YokAI.Debugging;
using YokAI.Notation;

namespace YokAI.Main
{
    public static class GameController
    {
        public static bool IsGameSet { get; private set; }
        public static int MoveNumber { get { return _moveNumber; } }
        public static uint LastMove { get; private set; }

        // Ban Proxy
        public static uint PlayingColor => _ban.PlayingColor;
        public static uint OpponentColor => _ban.OpponentColor;
        public static bool IsInCheck => _ban.IsInCheck(out bool _);
        public static bool IsMate => (IsInCheck && AvailableMoves.Length == 0) || _ban.IsOpponentKingOnPromotionZone();
        public static int PieceSetSize => _ban.PieceSet.Size;
        
        // Will generate and cache moves if ban state has changed
        public static uint[] AvailableMoves
        {
            get
            {
                if (_isDirty)
                {
                    _ban.GenerateMoves();
                    _availableMoves = _ban.GetLastMoveGeneration();
                    _isDirty = false;
                }
                return _availableMoves;
            }
        }

        public delegate void MakeMoveHandle();
        public static event MakeMoveHandle OnMoveMade;

        static GameController()
        {
            Clear();
        }

        public static void Clear()
        {
            IsGameSet = false;
            LastMove = Move.INVALID;
            _moveNumber = 0;
            _availableMoves = new uint[0];
            _isDirty = true;
            _ban = YokAIBan.Create(0);
        }

        #region Ban Access

        public static ref YokAIBan GetBan()
        {
            return ref _ban;
        }
        
        public static YokAIBan GetBanCopy()
        {
            return _ban.Copy();
        }

        public static bool TryGetKing(uint color, out byte kingId)
        {
            return _ban.TryGetKing(color, out kingId);
        }

        public static uint GetPiece(byte pieceId)
        {
            return _ban.PieceSet[pieceId];
        }

        #endregion Ban Access

        #region Move Making

        public static uint CreateMove(byte movingPieceId, int startCoordX, int startCoordY, int targetCoordX, int targetCoordY)
        {
            uint movingPiece = _ban.PieceSet[movingPieceId];
            bool isDrop = Location.Get(movingPiece) == Location.NONE;

            byte startCellId = isDrop ? Grid.INVALID_CELL_ID : Grid.GetCellId(startCoordX, startCoordY);
            byte targetCellId = Grid.GetCellId(targetCoordX, targetCoordY);
            byte capturedPieceId = Occupation.Get(_ban.Grid[targetCellId]);

            uint move = Move.Create(movingPieceId, capturedPieceId, startCellId, targetCellId, isDrop
                , hasPromoted: Type.Get(movingPiece) == Type.PAWN && PlayingColor == PromotionZone.Get(targetCellId) && !isDrop
                , hasUnpromoted: Type.Get(_ban.PieceSet[capturedPieceId]) == Type.GOLD);

            return move;
        }

        public static bool TryMakeMove(uint userMove)
        {
            if (!IsValid(userMove))
            {
                Logger.LogInvalidMove(nameof(GameController));
                return false;
            }
            MakeMove(userMove);
            return true;
        }

        public static bool TryMakeMove(string userMoveNotation)
        {
            if (!IsValid(userMoveNotation, out uint move))
            {
                Logger.LogInvalidMove(nameof(GameController));
                return false;
            }
            MakeMove(move);
            return true;
        }

        public static void TakeBack()
        {
            _ban.UnmakeMove(LastMove);
            _moveNumber--;
            _isDirty = true;
            OnMoveMade?.Invoke();
        }

        public static void PassTurn()
        {
            _ban.Pass();
            _moveNumber++;
            _isDirty = true;
            OnMoveMade?.Invoke();
        }

        private static bool IsValid(uint userMove)
        {
            foreach (uint move in AvailableMoves)
            {
                if (move == userMove)
                {
                    return true;
                }
            }
            return false;
        }
        
        private static bool IsValid(string userMoveNotation, out uint userMove)
        {
            userMove = Move.INVALID;
            foreach (uint move in AvailableMoves)
            {
                string moveNotation = Decryptor.GetNotationFromMove(move, ref _ban);
                if (moveNotation == userMoveNotation)
                {
                    userMove = move;
                }
            }
            return userMove != Move.INVALID;
        }

        private static void MakeMove(uint userMove)
        {
            _ban.MakeMove(userMove);
            LastMove = userMove;
            _moveNumber++;
            _isDirty = true;
            OnMoveMade?.Invoke();
        }

        #endregion Move Making

        #region Position Setup

        public static void ResetPosition()
        {
            Clear();
            PositionLoader.LoadPositionFromSFEN(ref _ban, ref _moveNumber, _startingPositionSFEN);
            IsGameSet = true;
        }

        public static void SetupPosition(string sfen)
        {
            _startingPositionSFEN = sfen;
            ResetPosition();
        }

        public static void SetupYokaiNoMori()
        {
            SetupPosition(PositionLoader.YOKAI_NO_MORI_STARTING_POSITION);
        }

        public static string SaveCurrentPosition()
        {
            return PositionLoader.SavePositionToSFEN(_ban, MoveNumber);
        }

        #endregion Position Setup

        private static string _startingPositionSFEN;
        private static int _moveNumber;
        private static uint[] _availableMoves;
        private static bool _isDirty;
        private static YokAIBan _ban;
    }
}