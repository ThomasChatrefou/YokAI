using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using YokAI.POC;

namespace YokAI
{
    public class BanManager : _Scripts.Utilities.Singleton<BanManager>
    {
        [SerializeField] private Color indicatorColor;
        [SerializeField] private Color moveIndicatorColor;
        
        [SerializeField] private GameObject[] whitePawnBank;
        [SerializeField] private GameObject[] blackPawnBank;
        [SerializeField] private Transform[] poolPositions;
        [SerializeField] private SpriteRenderer[] moveIndicators;

        private Dictionary<int, PieceMonobehaviour> _pieces = new ();
        private Dictionary<(int, int), Transform> pools = new ();

        public Color IndicatorColor => indicatorColor;

        public static List<int> Pieces = new() // this is temporary
        {
            { Piece.NONE },
            { Piece.PAWN  },
            { Piece.BISHOP},
            { Piece.ROOK  },
            { Piece.GOLD  },
            { Piece.KING  }
        };
        
        private void Start()
        {
            BanDebugger.SetupStartingPosition();

            for (int i = 0; i < whitePawnBank.Length; i++)
            {
                PieceMonobehaviour piece = Instantiate(whitePawnBank[i],Vector3.up * 10,quaternion.identity, transform).GetComponent<PieceMonobehaviour>();
                piece.PieceID = Pieces[i + 1] | Piece.WHITE;
                _pieces.Add(piece.PieceID, piece);
                
                piece = Instantiate(blackPawnBank[i],Vector3.up * 10,quaternion.identity, transform).GetComponent<PieceMonobehaviour>();
                piece.PieceID = Pieces[i + 1] | Piece.BLACK;
                _pieces.Add(piece.PieceID, piece);
            }

            for (int i = 0; i < 3; i++)
            {
                pools[(Pieces[i+1] | Piece.WHITE, Piece.WHITE)] = poolPositions[i   + 3*i];
                pools[(Pieces[i+1] | Piece.BLACK, Piece.WHITE)] = poolPositions[i+1 + 3*i];
                pools[(Pieces[i+1] | Piece.WHITE, Piece.BLACK)] = poolPositions[i+2 + 3*i];
                pools[(Pieces[i+1] | Piece.BLACK, Piece.BLACK)] = poolPositions[i+3 + 3*i];
            }

            // foreach (var pos in pools)
            // {
            //     Debug.Log(Decryptor.GetSymbolFromPiece(pos.Key.Item1) + ", " + pos.Key.Item2 + " | " + pos.Value.position.ToString());
            // }

            InitBoard(Ban.Grid);
        }

        private void InitBoard(int[] grid)
        {
            for (int i = 0; i < grid.Length; i++)
            {
                int piece = grid[i];
                if (piece != Piece.NONE)
                    MovePieceOnBoard(piece, Ban.GetCoordinates(i));
            }
        }

        public void MovePieceOnBoard(int piece, Vector2Int coordinates)
        {
            _pieces[piece].transform.position = new Vector3(coordinates.x, coordinates.y, 0);
        }

        public bool CheckIfCanMake(int piece, Vector2 startPos, Vector2 targetPos)
        {
            int startPosInt = Ban.GetGridIndex(Vector2Int.RoundToInt(startPos));
            int targetPosInt = Ban.GetGridIndex(Vector2Int.RoundToInt(targetPos));
            
            Move inputMove = new Move(piece, startPosInt, targetPosInt);
            if (Ban.IsSet && Ban.IsValid(inputMove))
            {
                Ban.MakeMove(inputMove);

                if (Ban.PlayingColor != Piece.WHITE)
                {
                    for (int i = 0; i < Ban.WhitePoolNextAvailableIndex; i++)
                    {
                        AddPieceToPool(Ban.WhitePool[i], Piece.WHITE);
                    }
                }
                else
                {
                    for (int i = 0; i < Ban.BlackPoolNextAvailableIndex; i++)
                    {
                        AddPieceToPool(Ban.BlackPool[i], Piece.BLACK);
                    }
                }
                
                return true;
            }

            return false;
        }

        public void MoveIndicator(int piece, bool toggle)
        {
            if (!toggle)
            {
                foreach (var moveIndicator in moveIndicators)
                {
                    moveIndicator.color = Color.clear;
                }
                return;
            }
                
            
            MoveGenerator.GenerateMoves();

            foreach (Move move in MoveGenerator.Moves)
            {
                if (move.Piece == piece)
                {
                    moveIndicators[move.TargetSquare].color = moveIndicatorColor ;
                }
            }
        }

        private void AddPieceToPool(int piece, int poolColor)
        {
            Vector3 newPos = pools[(piece, poolColor)].position;
            
            _pieces[piece].transform.position = newPos;
            _pieces[piece].OriginalPosition = newPos;

            SpriteRenderer spriteRenderer = _pieces[piece].GetComponent<SpriteRenderer>();

            spriteRenderer.flipX = !spriteRenderer.flipX;
            spriteRenderer.flipY = !spriteRenderer.flipY;

        }
        
    }
}