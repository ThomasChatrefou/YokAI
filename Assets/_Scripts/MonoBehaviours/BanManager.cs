using System;
using System.Collections.Generic;
using _Scripts.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace YokAI
{
    public class BanManager : Singleton<BanManager>
    {
        [SerializeField] private Color indicatorColor;
        [SerializeField] private Color moveIndicatorColor;
        
        [SerializeField] private GameObject[] whitePawnBank;
        [SerializeField] private GameObject[] blackPawnBank;
        [SerializeField] private GameObject[] moveIndicators;

        private Dictionary<int, PieceMonobehaviour> _pieces = new ();

        public Color IndicatorColor => indicatorColor;

        public static List<int> Pieces = new()
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
                return true;
            }

            return false;
        }

        public void MoveIndicator(int piece, bool toggle)
        {
            MoveGenerator.GenerateMoves();

            foreach (Move move in MoveGenerator.Moves)
            {
                if (move.Piece == piece)
                {
                    moveIndicators[move.TargetSquare].GetComponent<SpriteRenderer>().color = toggle ? moveIndicatorColor : Color.clear;
                }
            }
        }
        
    }
}