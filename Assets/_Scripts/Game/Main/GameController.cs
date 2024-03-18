using YokAI.Properties;
using YokAI.Debugging;

namespace YokAI.Main
{
    public static class GameController
    {
        public static string StartingPositionSFEN { get; set; }

        public static YokAIBan Ban;
        public static uint PlayingColor { get { return Ban.PlayingColor; } }
        public static uint OpponentColor { get { return Ban.OpponentColor; } }

        public static int MoveNumber { get { return _moveNumber; } }
        public static uint LastMove { get; private set; }

        public static bool IsGenerationDirty { get; private set; } = true;
        public static uint[] AvailableMoves
        {
            get
            {
                if (IsGenerationDirty) GenerateMoves();
                return _availableMoves;
            }
        }

        public static bool IsGameSet { get; private set; }
        public static bool IsInCheck { get { return Ban.IsInCheck(out bool _); } }

        public static bool IsMate { get { return (IsInCheck && AvailableMoves.Length == 0) || Ban.IsOpponentKingOnPromotionZone(); } }

        public static byte[] KingIds { get { return Ban.KingIds; } }

        static GameController()
        {
            Ban = new YokAIBan();
            IsGameSet = false;
        }

        public static uint CreateMove(byte movingPieceId, int startCoordX, int startCoordY, int targetCoordX, int targetCoordY)
        {
            uint movingPiece = Ban.PieceSet[movingPieceId];
            bool isDrop = Location.Get(movingPiece) == Location.NONE;

            byte startCellId = isDrop ? Grid.INVALID_CELL_ID : Grid.GetCellId(startCoordX, startCoordY);
            byte targetCellId = Grid.GetCellId(targetCoordX, targetCoordY);
            byte capturedPieceId = Occupation.Get(Ban.Grid[targetCellId]);

            uint move = Move.Create(movingPieceId, capturedPieceId, startCellId, targetCellId, isDrop
                , hasPromoted: Type.Get(movingPiece) == Type.PAWN && PlayingColor == PromotionZone.Get(targetCellId) && !isDrop
                , hasUnpromoted: Type.Get(Ban.PieceSet[capturedPieceId]) == Type.GOLD);

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

        public static bool IsValid(uint userMove)
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

        public static bool IsValid(string userMoveNotation, out uint userMove)
        {
            userMove = Move.INVALID;
            foreach (uint move in AvailableMoves)
            {
                string moveNotation = Decryptor.GetNotationFromMove(move, Ban);
                if (moveNotation == userMoveNotation)
                {
                    userMove = move;
                }
            }
            return userMove != Move.INVALID;
        }

        public static void TakeBack()
        {
            Ban.UnmakeMove(LastMove);
            _moveNumber--;
            IsGenerationDirty = true;
        }

        public static void PassTurn()
        {
            Ban.Pass();
            _moveNumber++;
            IsGenerationDirty = true;
        }

        public static void EmptyPosition()
        {
            PositionLoader.LoadPositionFromSFEN(ref Ban, ref _moveNumber, PositionLoader.EMPTY_POSITION);
            IsGameSet = false;
            IsGenerationDirty = true;
        }

        public static void SetupYokaiNoMoriPosition()
        {
            StartingPositionSFEN = PositionLoader.YOKAI_NO_MORI_STARTING_POSITION;
            SetupStartingPosition();
        }

        public static void SetupStartingPosition()
        {
            PositionLoader.LoadPositionFromSFEN(ref Ban, ref _moveNumber, StartingPositionSFEN);
            IsGameSet = true;
            IsGenerationDirty = true;
        }
        
        public static string SaveCurrentPosition()
        {
            return PositionLoader.SavePositionToSFEN(Ban, MoveNumber);
        }

        private static void GenerateMoves()
        {
            Ban.GenerateMoves();
            _availableMoves = Ban.GetLastMoveGeneration();
            IsGenerationDirty = false;
        }

        private static void MakeMove(uint userMove)
        {
            Ban.MakeMove(userMove);
            LastMove = userMove;
            _moveNumber++;
            IsGenerationDirty = true;
        }

        private static int _moveNumber = 0;
        private static uint[] _availableMoves;
    }
}