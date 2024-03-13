using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using YokAI.GridProperties;
using YokAI.Main;
using YokAI.AI;
using YokAI.PieceProperties;
using YokAI.MoveProperties;
using UColor = UnityEngine.Color;
using PColor = YokAI.PieceProperties.Color;
using YGrid = YokAI.Main.Grid;
using Unity.VisualScripting;
using NaughtyAttributes;

namespace YokAI
{
    public class NewBanManager : _Scripts.Utilities.Singleton<NewBanManager>
    {
        [SerializeField] private UnityEngine.Color indicatorColor;
        [SerializeField] private UnityEngine.Color moveIndicatorColor;

        [SerializeField] private GameObject[] whitePiecesBank;
        [SerializeField] private GameObject[] blackPiecesBank;
        [SerializeField] private Transform[] poolPositions;
        [SerializeField] private SpriteRenderer[] moveIndicators;

        [SerializeField] public UColor WhiteColor;
        [SerializeField] public UColor BlackColor;
        
        private Dictionary<byte, PieceController> _pieces = new();
        private Dictionary<(byte, uint), Transform> pools = new();

        public UnityEngine.Color IndicatorColor => indicatorColor;

        public event System.Action<uint> OnMate;

        [SerializeField] private int _evaluationDepth = 0;

        [Button]
        public void End()
        {
            OnMate?.Invoke(GameController.OpponentColor);
        }
        [Button]
        public void EvaluateCurrentPosition()
        {
            int eval = AIController.EvaluateCurrentPosition(_evaluationDepth, GameController.Ban, out string bestMove);
            string evalStr = (eval > 0 ? "+" : string.Empty) + eval.ToString();
            Debug.Log($"Best Move : " + bestMove + "  |  Evaluation : " + evalStr);
        }

        private void Start()
        {
            GameController.SetupYokaiNoMoriPosition();

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
                
                if (pieceType == Type.PAWN || pieceType == Type.BISHOP || pieceType == Type.ROOK)
                {
                    
                    pools[(pieceId, PColor.WHITE)] = poolPositions[(pieceType - 1)*2 + pieceColor - 1];
                    pools[(pieceId, PColor.BLACK)] = poolPositions[(pieceType - 1)*2 + pieceColor - 1 + 6];
                }
            }

            InitBoard();
        }

        private void InitBoard()
        {
            Ban currentBan = GameController.Ban;
            for (byte cellId = 0; cellId < YGrid.SIZE; cellId++)
            {
                uint cell = currentBan.Grid[cellId];
                if (Occupation.Get(cell) != Occupation.NONE)
                {
                    byte pieceId = Occupation.Get(cell);
                    YGrid.GetCoordinates(cellId, out int x, out int y);
                    MovePieceOnBoard(pieceId, new Vector2Int(x, y));
                }
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
                byte capturedPieceId = CapturedPiece.Get(inputMove);

                if (Promote.Get(inputMove))
                {
                    _pieces[pieceId].ChangePromotionPawn(true);
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
                
                return true;
            }
            return false;
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
            Vector3 newPos = pools[(pieceId, poolColor)].position;

            _pieces[pieceId].transform.position = newPos;
            _pieces[pieceId].OriginalPosition = newPos;

            SpriteRenderer spriteRenderer = _pieces[pieceId].GetComponent<SpriteRenderer>();

            spriteRenderer.flipX = !spriteRenderer.flipX;
            spriteRenderer.flipY = !spriteRenderer.flipY;
            
            _pieces[pieceId].ChangeColor(poolColor == PColor.WHITE);
        }
    }
}