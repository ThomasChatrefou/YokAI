using UnityEngine;

namespace YokAI
{
    public static class Ban
    {
        public const int FILES = 3; // X Axis
        public const int RANKS = 4; // Y Axis
        public const int PLAYERS = 2;
        public const int POOLABLES = 3;

        public static int[] Grid;
        public static int[] WhitePool;
        public static int[] BlackPool;

        public static int PlayingColor;
        public static int MoveNumber;

        static Ban()
        {
            Grid = new int[FILES * RANKS];
            WhitePool = new int[PLAYERS * POOLABLES];
            BlackPool = new int[PLAYERS * POOLABLES];
        }

        public static int GetPieceFromCoordinate(int x, int y)
        {
            return Grid[GetGridIndex(x, y)];
        }

        public static int GetGridIndex(int x, int y)
        {
            return y * FILES + x;
        }

        public static Vector2Int GetCoordinates(int index)
        {
            return new Vector2Int()
            {
                x = index % FILES,
                y = index / FILES,
            };
        }
    }
}