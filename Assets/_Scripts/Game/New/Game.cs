using System.Collections.Generic;
using YokAI.PieceProperties;
using YokAI.MoveProperties;
using YokAI.GridProperties;
using Unity.VisualScripting;

namespace YokAI.Main
{
    public static class GameController
    {
        public static string StartingPositionSFEN { get; set; }

        public static Ban Ban;
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
            set
            {
                _availableMoves = value;
            }
        }

        public static bool IsGameSet { get; private set; }
        public static bool IsInCheck { get { return Ban.IsInCheck(out bool _); } }

        public static bool IsMate { get { return (IsInCheck && AvailableMoves.Length == 0) || Ban.IsOpponentKingOnPromotionZone(); } }

        public static byte[] KingIds { get { return Ban.KingIds; } }

        static GameController()
        {
            Ban = new Ban();
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
                Logger.LogInvalidMove();
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
                Logger.LogInvalidMove();
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

    public static class PositionLoader
    {
        public const string YOKAI_NO_MORI_STARTING_POSITION = "rkb/1p1/1P1/BKR w - 1";
        public const string EMPTY_POSITION = "3/3/3/3 - - 0";

        public static void LoadPositionFromSFEN(ref Ban ban, ref int moveNumber, string sfen)
        {
            ban.Clear();

            string[] splitedSfen = sfen.Split(" ");

            string banStr = splitedSfen[0];
            string playingColorStr = splitedSfen[1];
            string pools = splitedSfen[2];
            string moveNumberStr = splitedSfen[3];

            uint playingColor = playingColorStr == "w" ? Color.WHITE : (playingColorStr == "b" ? Color.BLACK : Color.NONE);
            if (playingColor == Color.NONE)
            {
                Logger.LogInvalidSFEN();
                return;
            }

            int rank = Grid.RANKS - 1;

            List<uint> piecesList = new();

            string[] splitedBan = banStr.Split("/");
            foreach (string rankSfen in splitedBan)
            {
                int file = 0;
                foreach (char symbol in rankSfen)
                {
                    if (char.IsDigit(symbol))
                    {
                        file += int.Parse(symbol.ToString());
                    }
                    else
                    {
                        byte cellId = Grid.GetCellId(file, rank);
                        uint newPiece = Decryptor.CreatePieceFromSymbol(symbol);
                        Mobility.Set(ref newPiece, MobilityByPiece.Get(Color.Get(newPiece), Type.Get(newPiece)));
                        Location.Set(ref newPiece, cellId);
                        piecesList.Add(newPiece);
                        ++file;
                    }
                }
                --rank;
            }

            if (pools != "-")
            {
                foreach (char symbol in pools)
                {
                    uint piece = Decryptor.CreatePieceFromSymbol(symbol);
                    Mobility.Set(ref piece, Mobility.DROP);
                    Location.Set(ref piece, Grid.INVALID_CELL_ID);
                }
            }

            uint[] piecesArray = new uint[piecesList.Count];
            piecesList.CopyTo(piecesArray);
            PieceSet newPieceSet = new(piecesArray);

            ban.Setup(newPieceSet, playingColor);

            moveNumber = int.Parse(moveNumberStr);
        }

        public static string SavePositionToSFEN(Ban ban, int moveNumber)
        {
            string sfen = string.Empty;

            string ranksSeparator = "/";
            string sfenElementsSeparator = " ";

            for (int rank = Grid.RANKS - 1; rank >= 0; rank--)
            {
                int emptyCellsCount = 0;
                for (int file = 0; file < Grid.FILES; file++)
                {
                    byte cellId = Grid.GetCellId(file, rank);
                    uint cell = ban.Grid[cellId];
                    if (Occupation.Get(cell) == Occupation.NONE)
                    {
                        ++emptyCellsCount;
                    }
                    else
                    {
                        if (emptyCellsCount > 0)
                        {
                            sfen += emptyCellsCount.ToString();
                            emptyCellsCount = 0;
                        }
                        byte pieceId = Occupation.Get(cell);
                        uint piece = ban.PieceSet[pieceId];
                        sfen += Decryptor.GetSymbolFromPiece(piece);
                    }
                }

                if (rank != 0)
                {
                    sfen += ranksSeparator;
                }
            }

            sfen += sfenElementsSeparator;
            sfen += ban.PlayingColor == Color.WHITE ? "w" : (ban.PlayingColor == Color.BLACK ? "b" : "-");

            sfen += sfenElementsSeparator;
            for (byte pieceId = 0; pieceId < ban.PieceSet.Size; pieceId++)
            {
                uint piece = ban.PieceSet[pieceId];
                if (Location.Get(piece) == Location.NONE)
                {
                    sfen += Decryptor.GetSymbolFromPiece(piece);
                }
            }

            sfen += sfenElementsSeparator;
            sfen += moveNumber.ToString();

            return sfen;
        }
    }

    public static class Decryptor
    {
        public const int ASCII_CODE_ALPHABET_START = 97;

        public static Dictionary<char, uint> PieceTypeBySymbol;
        public static Dictionary<uint, char> SymbolByPieceType;

        public static bool UseCompleteNotation = false;
        public static bool UseReducedNotation = false;
        
        static Decryptor()
        {
            PieceTypeBySymbol = new Dictionary<char, uint>()
            {
                [' '] = Type.NONE,
                ['p'] = Type.PAWN,
                ['b'] = Type.BISHOP,
                ['r'] = Type.ROOK,
                ['g'] = Type.GOLD,
                ['k'] = Type.KING
            };
            SymbolByPieceType = new Dictionary<uint, char>()
            {
                [Type.NONE] = ' ',
                [Type.PAWN] = 'p',
                [Type.BISHOP] = 'b',
                [Type.ROOK] = 'r',
                [Type.GOLD] = 'g',
                [Type.KING] = 'k',
            };
        }

        public static uint CreatePieceFromSymbol(char symbol)
        {
            uint color = char.IsUpper(symbol) ? Color.WHITE : Color.BLACK;
            if (!PieceTypeBySymbol.TryGetValue(char.ToLower(symbol), out uint type))
            {
                Logger.LogUnknownSymbol();
                return Piece.INVALID;
            }
            return Piece.Create(color, type);
        }

        public static char GetSymbolFromPiece(uint piece)
        {
            uint type = Type.Get(piece);
            if (!SymbolByPieceType.TryGetValue(type, out char result))
            {
                Logger.LogUnknownProperty();
                return '?';
            }

            uint color = Color.Get(piece);
            if (color == Color.WHITE)
            {
                result = char.ToUpper(result);
            }
            else if (color != Color.BLACK)
            {
                Logger.LogUnknownProperty();
                return '?';
            }

            return result;
        }

        public static string GetNotationFromMove(uint move, Ban currentBan)
        {
            Move.Unpack(move
                , out byte movingPieceId, out byte capturedPieceId, out byte startCellId, out byte targetCellId
                , out bool isDrop, out bool hasPromoted, out bool hasUnpromoted);

            uint movingPiece = currentBan.PieceSet[movingPieceId];
            uint capturedPiece = currentBan.PieceSet[capturedPieceId];
            char movingPieceSymbol = GetSymbolFromPiece(movingPiece);
            char capturedPieceSymbol = GetSymbolFromPiece(capturedPiece);


            Grid.GetCoordinates(startCellId, out int startCellFile, out int startCellRank);
            Grid.GetCoordinates(targetCellId, out int targetCellFile, out int targetCellRank);

            char startCellFileAscii = (char)(startCellFile + ASCII_CODE_ALPHABET_START);
            char targetCellFileAscii = (char)(targetCellFile + ASCII_CODE_ALPHABET_START);

            string notation = string.Empty;

            notation += movingPieceSymbol;
            if (hasPromoted)
            {
                notation += '=';
                notation += Color.Get(movingPiece) == Color.WHITE ? 'G' : 'g';
            }
            if (!UseReducedNotation)
            {
                notation += startCellFileAscii;
                notation += $"{startCellRank + 1}";
            }

            if (isDrop)
            {
                notation += '*';
            }
            if (capturedPieceId != PieceSet.INVALID_PIECE_ID)
            {
                notation += 'x';
                if (!UseReducedNotation & UseCompleteNotation)
                {
                    if (capturedPieceSymbol != '?')
                    {
                        notation += capturedPieceSymbol;
                    }
                    if (hasUnpromoted)
                    {
                        notation += '=';
                        notation += Color.Get(capturedPiece) == Color.WHITE ? 'P' : 'p';
                    }
                }
            }
            notation += targetCellFileAscii;
            notation += $"{targetCellRank + 1}";

            // [ADD] '+' when Check 

            return notation;
        }
    }

    public static class Logger
    {
        public static void Log<SOURCE_TYPE>()
        {

        }

        public static void LogUnknownProperty()
        {

        }
        public static void LogUnknownSymbol()
        {

        }
        public static void LogInvalidSFEN()
        {

        }
        public static void LogInvalidMove()
        {

        }
    }
}