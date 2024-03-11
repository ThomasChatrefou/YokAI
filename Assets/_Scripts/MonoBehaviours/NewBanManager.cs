using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using YokAI.GridProperties;
using YokAI.Main;
using YokAI.PieceProperties;
using YokAI.MoveProperties;
using UColor = UnityEngine.Color;
using PColor = YokAI.PieceProperties.Color;
using YGrid = YokAI.Main.Grid;

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

        private Dictionary<byte, PieceController> _pieces = new();
        private Dictionary<(uint, uint), Transform> pools = new();

        public UnityEngine.Color IndicatorColor => indicatorColor;

        private void Start()
        {
            GameController.SetupYokaiNoMoriPosition();

            PieceSet pieceSet = GameController.Ban.PieceSet;
            for (byte pieceId = 0; pieceId < GameController.Ban.PieceSet.Size; pieceId++)
            {
                uint pieceType = Type.Get(pieceSet[pieceId]);
                GameObject go = (PColor.Get(pieceSet[pieceId]) == PColor.WHITE) ? whitePiecesBank[pieceType - 1] : blackPiecesBank[pieceType - 1];
                PieceController pieceController = Instantiate(go, Vector3.up * 10, quaternion.identity, transform).GetComponent<PieceController>();
                pieceController.PieceID = pieceId;
                _pieces.Add(pieceId, pieceController);
            }

            for (int i = 0; i < 3; i++)
            {
                uint type = (uint)(i + 1);
                pools[(Piece.Create(PColor.WHITE, type), PColor.WHITE)] = poolPositions[i + 3 * i];
                pools[(Piece.Create(PColor.BLACK, type), PColor.WHITE)] = poolPositions[i + 1 + 3 * i];
                pools[(Piece.Create(PColor.WHITE, type), PColor.BLACK)] = poolPositions[i + 2 + 3 * i];
                pools[(Piece.Create(PColor.BLACK, type), PColor.BLACK)] = poolPositions[i + 3 + 3 * i];
            }

            // foreach (var pos in pools)
            // {
            //     Debug.Log(Decryptor.GetSymbolFromPiece(pos.Key.Item1) + ", " + pos.Key.Item2 + " | " + pos.Value.position.ToString());
            // }

            InitBoard();
        }

        private void InitBoard()
        {
            Ban currentBan = GameController.Ban;
            for (byte cellId = 0; cellId < YGrid.SIZE; cellId++)
            {
                uint cell = currentBan.Grid[cellId];
                if (cell != Cell.EMPTY)
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
                if (capturedPieceId != PieceSet.INVALID_PIECE_ID)
                {
                    AddPieceToPool(capturedPieceId, PColor.Get(GameController.Ban.PieceSet[capturedPieceId]));
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
            uint piece = GameController.Ban.PieceSet[pieceId];

            Vector3 newPos = pools[(Piece.Create(PColor.Get(piece), Type.Get(piece)), poolColor)].position;

            _pieces[pieceId].transform.position = newPos;
            _pieces[pieceId].OriginalPosition = newPos;

            SpriteRenderer spriteRenderer = _pieces[pieceId].GetComponent<SpriteRenderer>();

            spriteRenderer.flipX = !spriteRenderer.flipX;
            spriteRenderer.flipY = !spriteRenderer.flipY;
        }
    }
}