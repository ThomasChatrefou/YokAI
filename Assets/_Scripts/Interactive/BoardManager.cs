using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using YokAI.Main;
using YokAI.Properties;
using UColor = UnityEngine.Color;
using PColor = YokAI.Properties.Color;
using YGrid = YokAI.Main.Grid;


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

        [SerializeField] private float autoPlayPieceAnimDuration = 0.2f;
        
        private Dictionary<byte, PieceController> _pieces = new();
        private Dictionary<(byte, uint), Transform> _pools = new();

        public UColor IndicatorColor => indicatorColor;

        public event System.Action<uint> OnMate;

        public delegate void AutoMoveHandle();
        public event AutoMoveHandle OnAutoMoveMade;

        public delegate void DisableHandle();
        public event DisableHandle OnDisabled;

        public bool IsReady = true;

        private void Start()
        {
            GameController.SetupYokaiNoMori();
            SetupBoard();
        }

        private void OnDisable()
        {
            GameController.Clear();
            OnDisabled?.Invoke();
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
            for (byte pieceId = 0; pieceId < GameController.PieceSetSize; pieceId++)
            {
                uint currentPiece = GameController.GetPiece(pieceId);
                uint pieceType = Type.Get(currentPiece);
                uint pieceColor = PColor.Get(currentPiece);
                GameObject go = (pieceColor == PColor.WHITE) ? whitePiecesBank[pieceType - 1] : blackPiecesBank[pieceType - 1];
                PieceController pieceController = Instantiate(go, Vector3.up * 10, quaternion.identity, transform).GetComponent<PieceController>();
                pieceController.PieceID = pieceId;
                pieceController.ChangeColor(pieceColor == PColor.WHITE);
                _pieces.Add(pieceId, pieceController);
            }
        }

        private void DefinePiecesPositionInPools()
        {
            for (byte pieceId = 0; pieceId < GameController.PieceSetSize; pieceId++)
            {
                uint currentPiece = GameController.GetPiece(pieceId);
                uint pieceType = Type.Get(currentPiece);
                uint pieceColor = PColor.Get(currentPiece);
                if (pieceType == Type.PAWN || pieceType == Type.BISHOP || pieceType == Type.ROOK)
                {
                    _pools[(pieceId, PColor.WHITE)] = poolPositions[(pieceType - 1) * 2 + pieceColor - 1];
                    _pools[(pieceId, PColor.BLACK)] = poolPositions[(pieceType - 1) * 2 + pieceColor - 1 + 6];
                }
            }
        }
        
        private void InitBoard()
        {
            for (byte pieceId = 0; pieceId < GameController.PieceSetSize; pieceId++)
            {
                byte cellId = Location.Get(GameController.GetPiece(pieceId));
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
                AddPieceToPool(capturedPieceId, PColor.Get(GameController.GetPiece(capturedPieceId)));
            }

            if (GameController.TryGetKing(GameController.PlayingColor, out byte playingKingId))
            {
                _pieces[playingKingId].SetCheckPiece(GameController.IsInCheck);
            }

            if (GameController.TryGetKing(GameController.OpponentColor, out byte opponentKingId))
            {
                _pieces[opponentKingId].SetCheckPiece(false);
            }

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

        public void AIMovePiece(uint move)
        {
            IsReady = false;
            StartCoroutine(AIMovePiece_Coroutine(move));
        }
        
        private IEnumerator AIMovePiece_Coroutine(uint move)
        {
            float time = 0;
            YGrid.GetCoordinates(TargetCell.Get(move), out int x, out int y);
            
            while (time < autoPlayPieceAnimDuration)
            {
                Vector3 start = _pieces[MovingPiece.Get(move)].OriginalPosition;
                Vector3 end = new (x, y, 0);
                float completion = time / autoPlayPieceAnimDuration;

                _pieces[MovingPiece.Get(move)].transform.position = Vector3.Lerp(start, end, completion);
                 time += Time.deltaTime;
                 yield return new WaitForEndOfFrame();
            }

            _pieces[MovingPiece.Get(move)].OriginalPosition = new Vector3(x, y, 0);
            MakeMoveOnTheBoard(move);
            IsReady = true;
            OnAutoMoveMade?.Invoke();
        }
    }
}