using NaughtyAttributes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using YokAI.AI;
using YokAI.Main;
using YokAI.Properties;
using UColor = UnityEngine.Color;
using PColor = YokAI.Properties.Color;
using YGrid = YokAI.Main.Grid;
using Debug = UnityEngine.Debug;

namespace YokAI
{
    public class BoardManager : _Scripts.Utilities.Singleton<BoardManager>
    {
        [SerializeField] private UColor indicatorColor;
        [SerializeField] private UColor moveIndicatorColor;

        [SerializeField] private GameObject[] whitePiecesBank;
        [SerializeField] private GameObject[] blackPiecesBank;
        [SerializeField] private Transform[] poolPositions;
        [SerializeField] private SpriteRenderer[] moveIndicators;

        [SerializeField] public UColor WhiteColor;
        [SerializeField] public UColor BlackColor;
        
        private Dictionary<byte, PieceController> _pieces = new();
        private Dictionary<(byte, uint), Transform> _pools = new();

        public UColor IndicatorColor => indicatorColor;

        public event System.Action<uint> OnMate;


        [SerializeField][Range(0,10)] private int _evaluationDepth = 0;

        [Button]
        public void End()
        {
            OnMate?.Invoke(GameController.OpponentColor);
        }

        [Button]
        public void EvaluateCurrentPosition()
        {
            Task.Run(() =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                int eval = AIController.EvaluateCurrentPosition(_evaluationDepth, GameController.Ban, out string bestMove);
                string evalStr = (eval > 0 ? "+" : string.Empty) + eval.ToString();
                stopwatch.Stop();
                Debug.Log($"Best Move : " + bestMove + "  |  Evaluation : " + evalStr + "  |  In : " + stopwatch.ElapsedMilliseconds/1000f +"s"); 
           });
        }
        
        private void Start()
        {
            GameController.SetupYokaiNoMoriPosition();
            SetupBoard();
        }

        private void OnDisable()
        {
            GameController.Reset();
        }

        public void ResetBoard()
        {
            foreach (PieceController pieceController in _pieces.Values)
            {
                Destroy(pieceController.transform.gameObject);
            }
            _pieces = new();
            _pools = new();
        }

        public void SetupBoard()
        {
            CreatePieces();
            DefinePiecesPositionInPools();
            InitBoard();
        }

        private void CreatePieces()
        {
            PieceSet pieceSet = GameController.Ban.PieceSet;
            for (byte pieceId = 0; pieceId < GameController.Ban.PieceSet.Size; pieceId++)
            {
                uint pieceType = Type.Get(pieceSet[pieceId]);
                uint pieceColor = PColor.Get(pieceSet[pieceId]);
                GameObject go = (PColor.Get(pieceSet[pieceId]) == PColor.WHITE) ? whitePiecesBank[pieceType - 1] : blackPiecesBank[pieceType - 1];
                PieceController pieceController = Instantiate(go, Vector3.up * 10, quaternion.identity, transform).GetComponent<PieceController>();
                pieceController.PieceID = pieceId;
                pieceController.ChangeColor(pieceColor == PColor.WHITE);
                _pieces.Add(pieceId, pieceController);
            }
        }

        private void DefinePiecesPositionInPools()
        {
            PieceSet pieceSet = GameController.Ban.PieceSet;
            for (byte pieceId = 0; pieceId < GameController.Ban.PieceSet.Size; pieceId++)
            {
                uint pieceType = Type.Get(pieceSet[pieceId]);
                uint pieceColor = PColor.Get(pieceSet[pieceId]);
                if (pieceType == Type.PAWN || pieceType == Type.BISHOP || pieceType == Type.ROOK)
                {

                    _pools[(pieceId, PColor.WHITE)] = poolPositions[(pieceType - 1) * 2 + pieceColor - 1];
                    _pools[(pieceId, PColor.BLACK)] = poolPositions[(pieceType - 1) * 2 + pieceColor - 1 + 6];
                }
            }
        }
        
        private void InitBoard()
        {
            for (byte pieceId = 0; pieceId < GameController.Ban.PieceSet.Size; pieceId++)
            {
                byte cellId = Location.Get(GameController.Ban.PieceSet[pieceId]);
                YGrid.GetCoordinates(cellId, out int x, out int y);
                MovePieceOnBoard(pieceId, new Vector2Int(x, y));
            }
        }

        public void MovePieceOnBoard(byte pieceId, Vector2Int coordinates)
        {
            _pieces[pieceId].transform.position = new Vector3(coordinates.x, coordinates.y, 0);
        }

        public bool CheckIfCanMake(byte pieceId, Vector2 startPos, Vector2 targetPos)
        {
            Vector2Int startCoord = Vector2Int.RoundToInt(startPos);
            Vector2Int targetCoord = Vector2Int.RoundToInt(targetPos);

            uint inputMove = GameController.CreateMove(pieceId, startCoord.x, startCoord.y, targetCoord.x, targetCoord.y);
            if (GameController.IsGameSet && GameController.TryMakeMove(inputMove))
            {
                MakeMoveOnTheBoard(inputMove);
                return true;
            }
            return false;
        }

        public void MakeMoveOnTheBoard(uint inputMove)
        {
            byte movingPieceId = MovingPiece.Get(inputMove);
            byte capturedPieceId = CapturedPiece.Get(inputMove);

            if (Promote.Get(inputMove))
            {
                _pieces[movingPieceId].ChangePromotionPawn(true);
            }
            if (Unpromote.Get(inputMove))
            {
                _pieces[capturedPieceId].ChangePromotionPawn(false);
            }

            if (capturedPieceId != PieceSet.INVALID_PIECE_ID)
            {
                AddPieceToPool(capturedPieceId, PColor.Get(GameController.Ban.PieceSet[capturedPieceId]));
            }

            _pieces[GameController.KingIds[GameController.PlayingColor - 1]].SetCheckPiece(GameController.IsInCheck);
            _pieces[GameController.KingIds[GameController.OpponentColor - 1]].SetCheckPiece(false);

            if (GameController.IsMate)
            {
                OnMate?.Invoke(GameController.OpponentColor);
            }
        }

        public void MoveIndicator(byte pieceId, bool toggle)
        {
            if (!toggle)
            {
                foreach (var moveIndicator in moveIndicators)
                {
                    moveIndicator.color = UColor.clear;
                }
                return;
            }

            foreach (uint move in GameController.AvailableMoves)
            {
                if (MovingPiece.Get(move) == pieceId)
                {
                    moveIndicators[TargetCell.Get(move)].color = moveIndicatorColor;
                }
            }
        }

        private void AddPieceToPool(byte pieceId, uint poolColor)
        {
            Vector3 newPos = _pools[(pieceId, poolColor)].position;

            _pieces[pieceId].transform.position = newPos;
            _pieces[pieceId].OriginalPosition = newPos;

            SpriteRenderer spriteRenderer = _pieces[pieceId].GetComponent<SpriteRenderer>();

            spriteRenderer.flipX = !spriteRenderer.flipX;
            spriteRenderer.flipY = !spriteRenderer.flipY;
            
            _pieces[pieceId].ChangeColor(poolColor == PColor.WHITE);
        }
    }
}